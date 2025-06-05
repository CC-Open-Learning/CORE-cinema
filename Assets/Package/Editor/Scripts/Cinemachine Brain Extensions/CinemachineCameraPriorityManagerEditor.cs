using UnityEditor;
using UnityEngine;

namespace VARLab.CORECinema
{
    [CustomEditor(typeof(CinemachineCameraPriorityManager))]
    public class CinemachineCameraPriorityManagerEditor : Editor
    {

        /// <summary>
        ///     If AutoFindCameras is not enabled, expose the set of 
        ///     managed Cameras to be serialized.
        /// </summary>
        /// <remarks>
        ///     When the application is playing, the set of cameras is 
        ///     shown in the inspector regardless of the AutoFindCameras flag
        /// </remarks>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoFindCameras"));

            if (Application.isPlaying || !serializedObject.FindProperty("AutoFindCameras").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Cameras"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
