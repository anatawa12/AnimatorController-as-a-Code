using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace Anatawa12.AnimatorControllerAsACode.Editor
{
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

        /// <summary>
        /// Key: watching target asset, Value: List of watching generator
        /// </summary>
        private static readonly Dictionary<GUID, HashSet<GUID>> WatchingToGeneratorMap = new Dictionary<GUID, HashSet<GUID>>();

        /// <summary>
        /// Key: generator, Value: List of watching asset
        /// </summary>
        private static readonly Dictionary<GUID, HashSet<GUID>> GeneratorToWatchingMap = new Dictionary<GUID, HashSet<GUID>>();

        /// <summary>
        /// Key: generated AnimatorController, Value: generator
        /// </summary>
        private static readonly Dictionary<GUID, GUID> GeneratedToGeneratorMap = new Dictionary<GUID, GUID>();

        /// <summary>
        /// Set of AnimatorControllerGenerator that will be generated in next Update.
        /// </summary>
        private static readonly Queue<Action> RunNextUpdate = new Queue<Action>();

        private static State _state = State.Initialized;
        private const string SessionKeyPrefix = "com.anatawa12.animator-controller-as-a-code.";


        /// <summary>
        /// List name of Acc core modules. If those modules are reloaded, All AnimatorController will be regenerated.
        /// </summary>
        private static readonly ReadOnlyCollection<string> AccCoreModules = new List<string>
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
            var importedGuids = importedAssets.Select(Utils.AssetPathToGUID).ToArray();
            var movedGuids = movedAssets.Select(Utils.AssetPathToGUID).ToArray();
            var deletedGuids = deletedAssets.Select(path => GUID.TryParse(AssetDatabase.AssetPathToGUID(path), out var guid) ? guid : default).ToArray();

            // if generator is imported, it's modified
            var modifiedGenerators = new HashSet<GUID>(importedGuids);

            // if watching asset is updated, the generator is modified
            foreach (var importedPath in importedGuids.Concat(movedGuids))
            {
                if (WatchingToGeneratorMap.TryGetValue(importedPath, out var generators))
                    modifiedGenerators.UnionWith(generators);
            }

            // if watching target or generator is moved or removed, remove from watching mapping.
            foreach (var deleted in deletedGuids)
            {
                // if watching asset is removed, the generator is modified
                if (WatchingToGeneratorMap.TryGetValue(deleted, out var generators))
                {
                    modifiedGenerators.UnionWith(generators);
                    WatchingToGeneratorMap.Remove(deleted);
                }
                // remove the asset from watching mapping
                if (GeneratorToWatchingMap.TryGetValue(deleted, out var watchingAssets))
                {
                    foreach (var watchingAsset in watchingAssets)
                    {
                        // maybe removed in this foreach so use TryGet
                        if (WatchingToGeneratorMap.TryGetValue(watchingAsset, out var watchingGenerators))
                            watchingGenerators.Remove(deleted);
                    }
                }
            }

            foreach (var generator in modifiedGenerators
                         .Select(Utils.LoadAssetAtGUID<AnimatorControllerGenerator>)
                         .Where(x => x != null))
            {
                // first, regenerate
                RunNextUpdate.Enqueue(() => DoGenerateWithErrorCheck(generator));
                // then, update generator watching target
                SaveGeneratorInfoToMap(generator);
            }

            ///////////////////////////////////
            var proceedGenerators = new HashSet<GUID>();

            // if generate target is moved, update target path
            for (var i = 0; i < movedGuids.Length; i++)
            {
                var movedGuid = movedGuids[i];
                var movedToPath = movedAssets[i];
                if (GeneratedToGeneratorMap.TryGetValue(movedGuid, out var generatorGuid))
                {
                    proceedGenerators.Add(generatorGuid);
                    var generator = Utils.LoadAssetAtGUID<AnimatorControllerGenerator>(generatorGuid);
                    if (generator == null)
                    {
                        // Generator is removed. remove from mapping and continue;
                        GeneratedToGeneratorMap.Remove(movedGuid);
                        continue;
                    }
                    generator.UpdateNameIfNeeded();
                    generator.UpdateTargetPath(movedToPath);
                }
            }
            
            foreach (var movedGuid in movedGuids)
            {
                // it's already updated
                if (proceedGenerators.Contains(movedGuid)) continue;
                // if generator is moved, move generated
                var generator = Utils.LoadAssetAtGUID<AnimatorControllerGenerator>(movedGuid);
                if (generator != null)
                {
                    generator.UpdateNameIfNeeded();
                    var targetPath = generator.TargetPath;
                    RunNextUpdate.Enqueue(() =>
                    {
                        AssetDatabase.MoveAsset(AssetDatabase.GUIDToAssetPath(generator.TargetGUID.ToString()), targetPath);
                        AssetDatabase.Refresh();
                    });
                }
            }
        }

        private static void Update()
        {
            while (RunNextUpdate.Count != 0)
                RunNextUpdate.Dequeue()?.Invoke();
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

        private const char Separator = '\x1F'; // Information Separator One

        private static IEnumerable<string> GetStringsSessionState(string key) =>
            SessionState.GetString(SessionKeyPrefix + key, "")
                .Split(Separator)
                .Skip(1);

        private static void SetStringsSessionState(string key, IEnumerable<string> value)
        {
            var builder = new StringBuilder();
            foreach (var element in value) builder.Append(Separator).Append(element);
            SessionState.SetString(SessionKeyPrefix + key, builder.ToString());
        }

        private static void BeforeAssemblyReload()
        {
            SessionState.SetBool(SessionKeyPrefix + "AssemblyReload", true);
            SetStringsSessionState(nameof(CompiledAssemblies), CompiledAssemblies);
            SetStringsSessionState(nameof(CompileErrorAssemblies), CompileErrorAssemblies);
        }

        static async void AfterAssemblyReload()
        {
            if (!SessionState.GetBool(SessionKeyPrefix + "AssemblyReload", false))
            {
                await Task.Delay(1 * 1000);
                Debug.Log("Startup Detected!: Regenerating all");
                RegenerateAll();
                return;
            }
            CompiledAssemblies.UnionWith(GetStringsSessionState(nameof(CompiledAssemblies)));
            CompileErrorAssemblies.UnionWith(GetStringsSessionState(nameof(CompileErrorAssemblies)));

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

        private static IEnumerable<AnimatorControllerGenerator> FindAllAnimatorControllerGenerator() =>
            AssetDatabase.FindAssets($"t:{typeof(AnimatorControllerGenerator).FullName}")
                .Select(Utils.LoadAssetAtGUID<AnimatorControllerGenerator>);

        private static void SaveGeneratorInfoToMap(AnimatorControllerGenerator generator)
        {
            var guid = Utils.GetAssetGUID(generator);
            var watchingTarget = new HashSet<GUID>(generator.WatchingObjects.Select(Utils.GetAssetGUID));
            if (watchingTarget.Count != 0)
            {
                foreach (var target in watchingTarget)
                {
                    if (WatchingToGeneratorMap.TryGetValue(target, out var set))
                        set.Add(guid);
                    else
                        WatchingToGeneratorMap[target] = new HashSet<GUID>(new[] { guid });
                }

                GeneratorToWatchingMap[guid] = watchingTarget;
            }

            GeneratedToGeneratorMap[generator.TargetGUID] = guid;
        }

        private static void FindAndGenerate()
        {
            var regenerateAll = AccCoreModules.Any(CompiledAssemblies.Contains);

            if (regenerateAll)
            {
                Debug.Log("Compiling Acc core modules detected! All AnimatorControllers are regenerated.");
                // any assemblies of Generator type is reloaded
                RegenerateAll();
            }
            else
            {
                RegenerateIfRecompiled();
            }

            CompiledAssemblies.Clear();
        }

        internal static void RegenerateAll()
        {
            var generators = FindAllAnimatorControllerGenerator();
            var regeneratedCount = 0;
            foreach (var generator in generators)
            {
                SaveGeneratorInfoToMap(generator);
                DoGenerateWithErrorCheck(generator);
                regeneratedCount++;
            }
            Debug.Log($"regenerated {regeneratedCount} assets");
        }

        internal static void RegenerateIfRecompiled()
        {
            var generators = FindAllAnimatorControllerGenerator();
            var regeneratedCount = 0;
            foreach (var generator in generators)
            {
                SaveGeneratorInfoToMap(generator);
                // any assemblies of Generator type is reloaded
                if (generator.generators.Any(x => x != null && IsAssemblyCompiled(x.GetType().Assembly)))
                {
                    DoGenerateWithErrorCheck(generator);
                    regeneratedCount++;
                }
            }
            Debug.Log($"regenerated {regeneratedCount} assets");
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
