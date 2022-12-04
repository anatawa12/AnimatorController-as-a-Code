using System;
using System.IO;
using System.Text;
using Anatawa12.AnimatorControllerAsACode.Generator;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Editor
{
    public sealed class AnimatorControllerGenerator : ScriptableObject
    {
        // target information
        public GUID targetGuid;
        // must be relative to generator asset
        public string targetPath;

        // generator information
        public GeneratorLayerBase[] generators;

        private AnimatorController _targetResolved;

        public void DoGenerate()
        {
            if (!TryLoadController())
            {
                CreateControllerAtPath();
            }
            else
            {
                // if we loaded previous controller, clear previous controller
                _targetResolved.layers = Array.Empty<AnimatorControllerLayer>();
                _targetResolved.parameters = Array.Empty<AnimatorControllerParameter>();
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_targetResolved)))
                {
                    if (asset != _targetResolved)
                        DestroyImmediate(asset, true);
                }
            }

            foreach (var generator in generators)
            {
                generator.Generate(new ACaaC(generator.GeneratorName, _targetResolved));
            }
            EditorUtility.SetDirty(_targetResolved);
        }

        private bool TryLoadController()
        {
            // try load paths
            if (_targetResolved) return true;
            if (TryLoadViaGuid()) return true;
            return false;
        }

        private bool TryLoadViaGuid()
        {
            var path = AssetDatabase.GUIDToAssetPath(targetGuid.ToString());
            if (string.IsNullOrEmpty(path)) return false;
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (!(asset is AnimatorController controller))
                throw new InvalidOperationException($"{targetGuid} is not a AnimatorController");
            _targetResolved = controller;
            return true;
        }

        private void CreateControllerAtPath()
        {
            if (targetGuid.Empty()) {
                targetGuid = GUID.Generate();
                EditorUtility.SetDirty(this);
            }
            var path = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(path))
                throw new InvalidOperationException($"AnimatorControllerGenerator must be saved on disk to generate animator");
            var baseDir = path.Substring(0, path.LastIndexOf('/') + 1);
            var assetPath = baseDir + targetPath;
            var metaPath = $"{assetPath}.meta";
            File.WriteAllText(assetPath, EmptyAnimatorController, Encoding.UTF8);
            File.WriteAllText(metaPath, EmptyAnimatorControllerMeta.Replace("{GUID}", targetGuid.ToString()), Encoding.UTF8);
            AssetDatabase.Refresh(ImportAssetOptions.Default);
            _targetResolved = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
            Debug.Assert(_targetResolved != null, "created controller cannot be loaded");
        }

        private const string EmptyAnimatorController =
            "%YAML 1.1\n"
            + "%TAG !u! tag:unity3d.com,2011:\n"
            + "--- !u!91 &9100000\n"
            + "AnimatorController:\n"
            + "  m_ObjectHideFlags: 0\n"
            + "  m_CorrespondingSourceObject: {fileID: 0}\n"
            + "  m_PrefabInstance: {fileID: 0}\n"
            + "  m_PrefabAsset: {fileID: 0}\n"
            + "  m_Name: New Animator Controller\n"
            + "  serializedVersion: 5\n"
            + "  m_AnimatorParameters: []\n"
            + "  m_AnimatorLayers: []\n";

        // please replace "GUID" with actual guid without '-'
        private const string EmptyAnimatorControllerMeta =
            "fileFormatVersion: 2\n"
            + "guid: {GUID}\n"
            + "NativeFormatImporter:\n"
            + "  externalObjects: {}\n"
            + "  mainObjectFileID: 9100000\n"
            + "  userData:\n"
            + "  assetBundleName:\n"
            + "  assetBundleVariant:\n";
    }
}