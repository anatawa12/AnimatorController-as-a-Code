using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace Anatawa12.AnimatorControllerAsACode.Editor
{
    // TODO: Implement watch for assets
    [InitializeOnLoad]
    internal static class AutomaticGenerationCaller
    {
        /// <summary>
        /// List of compile identifier object. Added in compilationStarted and removed in compilationFinished
        /// </summary>
        private static readonly HashSet<Identity> CompilingList = new HashSet<Identity>();

        /// <summary>
        /// List of name of compiled assemblies
        /// </summary>
        private static readonly HashSet<string> CompiledAssemblies = new HashSet<string>();

        /// <summary>
        /// List of name of compiled assemblies
        /// </summary>
        private static readonly HashSet<string> CompileErrorAssemblies = new HashSet<string>();

        // TODO: consider rewrite with GUID instead of paths to avoid move before & move after confusion?
        /// <summary>
        /// Key: Path to watching target asset, Value: List of watching generator path
        /// </summary>
        private static readonly Dictionary<string, ImmutableHashSet<string>> WatchingToGeneratorMap = new Dictionary<string, ImmutableHashSet<string>>();

        /// <summary>
        /// Key: Path to generator, Value: List of watching asset path
        /// </summary>
        private static readonly Dictionary<string, ImmutableHashSet<string>> GeneratorToWatchingMap = new Dictionary<string, ImmutableHashSet<string>>();

        /// <summary>
        /// Key: Path to generator, Value: Path to generated AnimatorController
        /// </summary>
        private static readonly Dictionary<string, string> GeneratorToGeneratedMap = new Dictionary<string, string>();

        /// <summary>
        /// Key: Path to generated AnimatorController, Value: Path to generator
        /// </summary>
        private static readonly Dictionary<string, string> GeneratedToGeneratorMap = new Dictionary<string, string>();

        /// <summary>
        /// Set of AnimatorControllerGenerator that will be generated in next Update.
        /// </summary>
        private static readonly HashSet<AnimatorControllerGenerator> willGenerate =
            new HashSet<AnimatorControllerGenerator>();

        private static State _state = State.Initialized;
        private const string StateJsonPath = "Temp/com.anatawa12.animator-as-a-code.state.json";

        /// <summary>
        /// List name of ACC core modules. If those modules are reloaded, All AnimatorController will be regenerated.
        /// </summary>
        private static readonly ReadOnlyCollection<string> ACCCoreModules = new List<string>
        {
            "com.anatawa12.animator-controller-as-a-code.editor",
            "com.anatawa12.animator-controller-as-a-code.framework",
        }.AsReadOnly();

        static AutomaticGenerationCaller()
        {
            CompilationPipeline.compilationStarted += CompilationStarted;
            CompilationPipeline.compilationFinished += CompilationFinished;
            CompilationPipeline.assemblyCompilationFinished += AssemblyCompilationFinished;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += AfterAssemblyReload;
            EditorApplication.update += Update;
        }

        internal class AssetPostprocessorImpl : AssetPostprocessor
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
                string[] movedAssets,
                string[] movedFromAssetPaths) =>
                AutomaticGenerationCaller.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets,
                    movedFromAssetPaths);
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            var moveMapping = movedFromAssetPaths.Zip(movedAssets, (k, v) => new KeyValuePair<string, string>(k, v)).ToImmutableDictionary();
            // if generator is imported, it's modified
            var modifiedGenerators = new HashSet<string>(importedAssets);

            // if watching asset is updated, the generator is modified
            foreach (var importedPath in importedAssets.Concat(movedFromAssetPaths))
            {
                if (WatchingToGeneratorMap.TryGetValue(importedPath, out var generators))
                {
                    modifiedGenerators.UnionWith(generators.Select(moveMapping.GetOrKey));
                }
            }

            // if watching target or generator is moved or removed, remove from watching mapping.
            foreach (var deleted in movedFromAssetPaths.Concat(deletedAssets))
            {
                // remove the asset from watching mapping
                WatchingToGeneratorMap.Remove(deleted);
                if (GeneratorToWatchingMap.TryGetValue(deleted, out var watchingAssets))
                {
                    foreach (var watchingAsset in watchingAssets)
                    {
                        // maybe removed in this foreach 
                        if (WatchingToGeneratorMap.TryGetValue(watchingAsset, out var watchingGenerators))
                            watchingGenerators.Remove(deleted);
                    }
                }
            }

            foreach (var (path, generator) in modifiedGenerators
                         .Select(path => (path, generator: AssetDatabase.LoadAssetAtPath<AnimatorControllerGenerator>(path)))
                         .Where(x => x.generator != null))
            {
                // first, regenerate
                willGenerate.Add(generator);
                // then, update generator watching target
                var watchingTarget = generator.WatchingObjects.Select(AssetDatabase.GetAssetPath)
                    .Where(x => !string.IsNullOrEmpty(x)).ToImmutableHashSet();
                if (watchingTarget.Count != 0)
                {
                    foreach (var target in watchingTarget)
                    {
                        WatchingToGeneratorMap[target] =
                            (WatchingToGeneratorMap.GetOrDefault(target) ?? ImmutableHashSet<string>.Empty).Add(path);
                    }
                    GeneratorToWatchingMap[path] = watchingTarget;
                }
            }

            ///////////////////////////////////
            // Process Generator move and deletion
            foreach (var deleted in deletedAssets)
            {
                // if generator is actually deleted, remove from mapping
                if (GeneratorToGeneratedMap.TryGetValue(deleted, out var generated))
                {
                    GeneratorToGeneratedMap.Remove(deleted);
                    GeneratedToGeneratorMap.Remove(generated);
                }
            }

            var proceedGenerators = new HashSet<string>();

            // if generate target is moved, update target path
            foreach (var (moveFrom, moveTo) in moveMapping)
            {
                if (GeneratedToGeneratorMap.TryGetValue(moveFrom, out var foundGeneratorPath))
                {
                    proceedGenerators.Add(foundGeneratorPath);
                    var actualGeneratorPath = moveMapping.GetOrKey(foundGeneratorPath);
                    var generator = AssetDatabase.LoadAssetAtPath<AnimatorControllerGenerator>(actualGeneratorPath);
                    generator.UpdateTargetPath(moveTo);

                    GeneratedToGeneratorMap.Remove(moveFrom);
                    GeneratedToGeneratorMap[moveTo] = actualGeneratorPath;
                    GeneratorToGeneratedMap[actualGeneratorPath] = moveTo;
                }
            }
            
            foreach (var (moveFrom, moveTo) in moveMapping)
            {
                // it's already updated
                if (proceedGenerators.Contains(moveFrom)) continue;
                // if generator is moved, move generated
                if (GeneratorToGeneratedMap.TryGetValue(moveFrom, out var generatedPath))
                {
                    var generator = AssetDatabase.LoadAssetAtPath<AnimatorControllerGenerator>(moveTo);
                    AssetDatabase.MoveAsset(generatedPath, generator.TargetPath);
                }
            }

            Debug.Log($"OnPostprocessAllAssets:\n" +
                      $"importedAssets: {string.Join(", ", importedAssets)}\n" +
                      $"deletedAssets: {string.Join(", ", deletedAssets)}\n" +
                      $"movedAssets: {string.Join(", ", movedAssets)}\n" +
                      $"movedFromAssetPaths: {string.Join(", ", movedFromAssetPaths)}");
        }

        private static void Update()
        {
            foreach (var generator in willGenerate)
                DoGenerateWithErrorCheck(generator);
            willGenerate.Clear();
        }

        private static void CompilationStarted(object obj)
        {
            CompilingList.Add(new Identity(obj));
            _state = State.Compiling;
        }

        private static void CompilationFinished(object obj)
        {
            CompilingList.Remove(new Identity(obj));
            if (CompilingList.Count == 0)
                _state = State.WaitingForAssemblyReload;
        }

        private static void AssemblyCompilationFinished(string name, CompilerMessage[] messages)
        {
            Debug.Assert(_state == State.Compiling, $"Compiling is expected but it's {_state}");
            var assemblyName = Path.GetFileNameWithoutExtension(name);
            CompiledAssemblies.Add(assemblyName);
            if (messages.Any(x => x.type == CompilerMessageType.Error))
                CompileErrorAssemblies.Add(assemblyName);
            else
                CompileErrorAssemblies.Remove(assemblyName);
        }

        private static void BeforeAssemblyReload()
        {
            // save data to file
            File.WriteAllText(StateJsonPath, JsonConvert.SerializeObject(new JsonState
            {
                CompiledAssemblies = CompiledAssemblies,
                CompileErrorAssemblies = CompileErrorAssemblies,
            }));
        }

        static async void AfterAssemblyReload()
        {
            // load data from file
            try
            {
                var json = File.ReadAllText(StateJsonPath);
                File.Delete(StateJsonPath);
                var state = JsonConvert.DeserializeObject<JsonState>(json);
                state.CompileErrorAssemblies.RemoveWhere(CompiledAssemblies.Contains);
                CompiledAssemblies.UnionWith(state.CompiledAssemblies);
                CompileErrorAssemblies.UnionWith(state.CompileErrorAssemblies);
            }
            catch (IOException e)
            {
                Debug.LogErrorFormat("Error loading State Json. {0}", e);
                return;
            }
            catch (JsonException e)
            {
                Debug.LogErrorFormat("Error loading State Json. {0}", e);
                return;
            }

            if (_state != State.Initialized)
                return;

            if (CompileErrorAssemblies.Count != 0)
            {
                Debug.Log("Compiling detected but compile error detected!");
                if (_state == State.Initialized)
                    _state = State.Initialized;
                return;
            }

            _state = State.AssemblyReloaded;

            await Task.Delay(1 * 1000);

            // this means next compilation is started within one second.
            if (_state != State.AssemblyReloaded) return;

            // now, Search & Regenerate controllers!
            Debug.Log("Compiling detected & no compiling detected for one seconds! Regenerating AnimatorController");
            FindAndGenerate();
        }

        private static void FindAndGenerate()
        {
            var regenerateAll = ACCCoreModules.Any(CompiledAssemblies.Contains);
            var generatorPaths = AssetDatabase.FindAssets($"t:{typeof(AnimatorControllerGenerator).FullName}");
            var generators = generatorPaths.Select(guid => AssetDatabase.LoadAssetAtPath<AnimatorControllerGenerator>(AssetDatabase.GUIDToAssetPath(guid)));
            var regeneratedCount = 0;

            if (regenerateAll)
            {
                Debug.Log("Compiling ACC core modules detected! All AnimatorControllers are regenerated.");
                // any assemblies of Generator type is reloaded
                foreach (var generator in generators)
                {
                    DoGenerateWithErrorCheck(generator);
                    regeneratedCount++;
                }
            }
            else
            {
                foreach (var generator in generators)
                {
                    // any assemblies of Generator type is reloaded
                    if (generator.generators.Any(x => IsAssemblyCompiled(x.GetType().Assembly)))
                    {
                        DoGenerateWithErrorCheck(generator);
                        regeneratedCount++;
                    }
                }
            }

            Debug.Log($"regenerated {regeneratedCount} assets");

            CompiledAssemblies.Clear();
        }

        private static bool IsAssemblyCompiled(Assembly assembly) => CompiledAssemblies.Contains(assembly.GetName().Name);

        private static void DoGenerateWithErrorCheck(AnimatorControllerGenerator generator)
        {
            try
            {
                generator.DoGenerate();
            }
            catch (Exception e)
            {
                var path = AssetDatabase.GetAssetPath(generator);
                Debug.LogErrorFormat("Exception generating {0}: {1}", path, e);
            }
        }

        private enum State
        {
            Initialized,
            Compiling,
            WaitingForAssemblyReload,
            AssemblyReloaded,
        }

        // ReSharper disable MemberHidesStaticFromOuterClass
        [JsonObject("JsonState")]
        private class JsonState
        {
            [JsonProperty] public HashSet<string> CompiledAssemblies = new HashSet<string>();
            [JsonProperty] public HashSet<string> CompileErrorAssemblies = new HashSet<string>();
        }
        // ReSharper restore MemberHidesStaticFromOuterClass
    }

    internal readonly struct Identity
    {
        public readonly object Body;

        public Identity(object body)
        {
            Body = body;
        }

        public bool Equals(Identity other)
        {
            return Body == other.Body;
        }

        public override bool Equals(object obj)
        {
            return obj is Identity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Body != null ? RuntimeHelpers.GetHashCode(Body) : 0;
        }

        public static bool operator ==(Identity a, Identity b) => a.Equals(b);
        public static bool operator !=(Identity a, Identity b) => !(a == b);
    }
}
