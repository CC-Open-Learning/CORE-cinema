using UnityEditor;

namespace VARLab.CORECinema
{
    [CustomEditor(typeof(CinemachinePathMovementSettings))]
    public class CinemachinePathMovementSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OverrideDefaults"));

            if (serializedObject.FindProperty("OverrideDefaults").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MovementSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("InvertMovementDirection"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
