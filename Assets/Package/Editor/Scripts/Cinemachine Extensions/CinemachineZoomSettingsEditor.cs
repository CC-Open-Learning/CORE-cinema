using UnityEditor;

namespace VARLab.CORECinema
{
    [CustomEditor(typeof(CinemachineZoomSettings))]
    public class CinemachineZoomSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OverrideDefaults"));

            if (serializedObject.FindProperty("OverrideDefaults").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ZoomMultiplier"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
