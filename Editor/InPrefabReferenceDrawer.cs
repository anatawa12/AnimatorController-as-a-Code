using Anatawa12.AnimatorControllerAsACode.Framework;
using UnityEditor;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Editor
{
    [CustomPropertyDrawer(typeof(InPrefabReferenceAttribute))]
    public class InPrefabReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
#if ANIMATOR_CONTROLLER_AS_A_CODE_DEBUG
            position.height = EditorGUIUtility.singleLineHeight;
#endif
            if (property.serializedObject.targetObjects.Length != 1)
            {
                EditorGUI.LabelField(position, label, Styles.MultiEditing, EditorStyles.boldLabel);
                return;
            }
            var targetObject = property.serializedObject.targetObjects[0];
            if (!(targetObject is GeneratorLayerBase layer))
            {
                EditorGUI.LabelField(position, label, Styles.GeneratorLayer, EditorStyles.boldLabel);
                return;
            }

            var generator = layer.GetGenerator();
            if (!generator)
            {
                EditorGUI.LabelField(position, label, Styles.NoGenerator, EditorStyles.boldLabel);
                return;
            }

            var targetGameObject = generator.target != null ? generator.target.gameObject : null;

            if (!targetGameObject || !PrefabUtility.IsPartOfPrefabAsset(targetGameObject))
            {
                EditorGUI.LabelField(position, label, Styles.InvalidGenerator, EditorStyles.boldLabel);
                return;
            }

            var value = property.objectReferenceValue;
            using (var s = new EditorGUI.PropertyScope(position, label, property))
            {
                label = s.content;
                EditorGUI.BeginChangeCheck();
                value = EditorGUI.ObjectField(position, label, value, fieldInfo.FieldType, true);
                if (EditorGUI.EndChangeCheck())
                {
                    var prefabAssetValue = ResolvePrefabAssetObject(targetGameObject, value);
                    if (prefabAssetValue)
                    {
                        value = property.objectReferenceValue = prefabAssetValue;
                    }
                }
            }
#if ANIMATOR_CONTROLLER_AS_A_CODE_DEBUG
            var id = GlobalObjectId.GetGlobalObjectIdSlow(value);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.IntField(position, "type", id.identifierType);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.TextField(position, "guid", id.assetGUID.ToString());
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.TextField(position, "targetId", id.targetObjectId.ToString());
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.TextField(position, "prefabId", id.targetPrefabId.ToString());
#endif
        }

        private Object ResolvePrefabAssetObject(GameObject target, Object value)
        {
            // null is allowed for everyone
            if (!value) return value;
            // Persistent value is allowed
            if (EditorUtility.IsPersistent(value)) return value;
            // now, resolved should be non-Persistent, scene objects.
            GameObject go;
            if (value is GameObject cast) go = cast;
            else if (value is Component component) go = component.gameObject;
            else return null; // unknown objects. false by default.

            // scene rootCount is one for preview scene
            if (go.scene.rootCount != 1) return null;
            var goRootId = GlobalObjectId.GetGlobalObjectIdSlow(go.scene.GetRootGameObjects()[0]);
            var goRootFileId = goRootId.targetObjectId ^ goRootId.targetPrefabId;
            var prefabRootId = GlobalObjectId.GetGlobalObjectIdSlow(target);
            if (goRootFileId != prefabRootId.targetObjectId) return null;

            var valueId = GlobalObjectId.GetGlobalObjectIdSlow(value);
            var valueFileId = valueId.targetObjectId ^ valueId.targetPrefabId;
            return ResolveObject(prefabRootId.assetGUID.ToString(), valueFileId);
        }

        public static Object ResolveObject(string guid, ulong file)
        {
            GlobalObjectId.TryParse($"GlobalObjectId_V1-1-{guid}-{file}-0", out var id);
            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
        }
/*
  GlobalObjectId.GetGlobalObjectIdSlow(target).ToString()
  "GlobalObjectId_V1-1-9d1412f596bc44e888848d8572958895-4031904467837862739-0"

  GlobalObjectId.GetGlobalObjectIdSlow(go).ToString()
  "GlobalObjectId_V1-2-00000000000000000000000000000000-1980236449848966504-0"

  GlobalObjectId.GetGlobalObjectIdSlow(go.transform.parent.gameObject).ToString()
  "GlobalObjectId_V1-2-00000000000000000000000000000000-4031904467837862739-0"
 */
#if ANIMATOR_CONTROLLER_AS_A_CODE_DEBUG
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight
            + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4;
#endif
        
        private static class Styles
        {
            public static readonly GUIContent MultiEditing = new GUIContent("Multi Editing not supported");
            public static readonly GUIContent GeneratorLayer = new GUIContent("PrefabReference must be on GeneratorLayer");
            public static readonly GUIContent NoGenerator = new GUIContent("Not belongs to Generator");
            public static readonly GUIContent InvalidGenerator = new GUIContent("Belongs to invalid Generator");
        }
    }
}
