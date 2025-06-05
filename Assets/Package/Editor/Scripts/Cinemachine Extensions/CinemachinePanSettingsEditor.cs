using UnityEditor;

namespace VARLab.CORECinema
{
    [CustomEditor(typeof(CinemachinePanSettings))]
    public class CinemachinePanSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OverrideDefaults"));

            if (serializedObject.FindProperty("OverrideDefaults").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HorizontalPanLimit"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("VerticalPanLimit"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MouseSensitivity"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseZoomScaling"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}