using MelodySuite.Outfit.Runtime;
using UnityEditor;
using UnityEngine;

namespace MelodySuite.Outfit.Editor
{
    [CustomEditor(typeof(OutfitController))]
    [CanEditMultipleObjects]
    public class OutfitControllerEditor : UnityEditor.Editor
    {
        private bool needsApply;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            if (needsApply)
            {
                EditorGUILayout.HelpBox(
                    "Outfit changes have not been applied.\nClick 'Apply Outfits' to update the scene.",
                    MessageType.Warning
                );
                if (GUILayout.Button("Apply Outfits"))
                    ApplyOutfits();
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Apply Outfits");
                EditorGUI.EndDisabledGroup();
            }
            
            GUILayout.Space(8);
            
            EditorGUI.BeginChangeCheck();

            DrawDefaultInspector();

            if (EditorGUI.EndChangeCheck())
            {
                needsApply = true;
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void ApplyOutfits()
        {
            var controller = (OutfitController)target;
            
            Undo.RegisterFullObjectHierarchyUndo(
                controller.gameObject,
                "Apply Outfits"
            );

            controller.ApplyAllOutfits();
            
            EditorUtility.SetDirty(controller);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                controller.gameObject.scene
            );

            needsApply = false;
        }
    }
}