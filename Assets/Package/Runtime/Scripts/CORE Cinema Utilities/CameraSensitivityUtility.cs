using Cinemachine;
using System.Text;
using UnityEngine;

namespace VARLab.CORECinema
{

    /// <summary>
    ///     A debug utility for testing camera panning settings in Editor vs WebGL.
    ///     Uses IMGUI to draw a simple draggable debug window.
    /// </summary>
    public class CameraSensitivityUtility : MonoBehaviour
    {
        private static readonly Rect DefaultRect = new(8, 8, 220, 0);

        private readonly int id = 1024;

        public CinemachineBrain CameraBrain;

        public bool Active = false;

        private CinemachinePan panController;

        private Rect windowRect = DefaultRect;

        public void Awake()
        {
            panController = CameraBrain.GetComponent<CinemachinePan>();
            if (!panController) { Destroy(this); }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.U)) { Active = !Active; }
        }

        public void OnGUI()
        {
            if (!Active) { return; }

            windowRect = GUILayout.Window(id,
                windowRect,
                WindowCallback,
                "Camera Pan Settings",
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));
        }

        private void WindowCallback(int id)
        {
            if (id != this.id) { return; }

            float platform = panController.MouseSensitivity * panController.PlatformSensitivityModifier;

            float xInput = Input.GetAxis("Mouse X");
            float yInput = Input.GetAxis("Mouse Y");

            StringBuilder builder = new();
            builder.AppendLine("Sensitivity:");
            builder.AppendLine($"  platform\t{platform}");
            builder.AppendLine($"  raw\t{panController.MouseSensitivity}");
            builder.AppendLine($"  modifier\t{panController.PlatformSensitivityModifier}");
            builder.AppendLine($"X Input:\t{xInput}");
            builder.AppendLine($"Y Input:\t{yInput}");
            builder.AppendLine($"X Applied:\t{xInput * platform}");
            builder.AppendLine($"Y Applied:\t{yInput * platform}");

            // Camera sensitivity ranges from 0.1 to 4 so this slider will use the same.
            // This manipulates the underlying sensitivity value directly
            panController.MouseSensitivity = GUILayout.HorizontalSlider(panController.MouseSensitivity, 0.1f, 4f);

            GUILayout.Label(builder.ToString());

            Active = !GUILayout.Button("Close");

            // Allows the window to be dragged around the screen to be repositioned

            GUI.DragWindow();
        }
    }
}
