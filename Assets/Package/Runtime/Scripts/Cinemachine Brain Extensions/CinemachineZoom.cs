using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.CORECinema
{

    /// <summary>
    ///     Handles user camera zoom with mouse wheel for a <see cref="CinemachineBrain"/>.
    /// </summary>
    /// <remarks>
    ///     A given <see cref="CinemachineVirtualCamera"/> must have a 
    ///     <see cref="CinemachineZoomSettings"/> extension component in order for user zoom
    ///     to be supported at that specific vcam.
    /// </remarks>
    [DisallowMultipleComponent]
    public class CinemachineZoom : CinemachineBrainExtension
    {
        public const float Sensitivity = 0.04f;

        protected CinemachineZoomSettings settings;

        [SerializeField, Range(1f, 8f),
            Tooltip("Maximum zoom multiplier. The value is inverted internally " +
            "(ie. 8x zoom is represented as 0.125")]
        private float zoomMultiplier = 2f;



        [Header("Events")]

        [Tooltip("Invoked each time the zoom multiplier is changed, " +
            "providing the inverse of the zoom multiplier as 'zoom scale'")]
        public UnityEvent<float> OnZoomMultiplierChanged;

        [Tooltip("Invoked each time the camera zoom is changed, " +
            "providing the 'zoom scale' value")]
        public UnityEvent<float> OnCameraZoom;

        [Tooltip("Invoked each time the virtual camera changes, " +
            "indicating whether zoom functionality is available")]
        public UnityEvent<bool> OnZoomAvailable;

        private bool isFocused;

        /// <summary>
        ///     Used to indicate the frame that the application gains focus again.
        ///     Helps to avoid camera jitter due to strange cached mouse inputs.
        /// </summary>
        /// <remarks>
        ///     Eventually would be good to replace the use of the <see cref="Input"/> 
        ///     system in favour of the newer Input Action system. It seems the issue
        ///     that this fixes may be resolved naturally.
        /// </remarks>
        private bool focusGainedFrame = false;


        public float ZoomMultiplier
        {
            get => settings && settings.OverrideDefaults ? settings.ZoomMultiplier : zoomMultiplier;
            set => zoomMultiplier = value;
        }

        /// <value> Inverse of the <see cref="ZoomMultiplier"/> as long as it is valid </value>
        public float ZoomInverse => (ZoomMultiplier > 1) ? (1 / ZoomMultiplier) : 1;

        public override void HandleVirtualCameraChanged(ICinemachineCamera incoming, ICinemachineCamera outgoing)
        {
            if (incoming == null || incoming.VirtualCameraGameObject == null)
            {
                settings = null;
                return;
            }

            settings = incoming?.VirtualCameraGameObject.GetComponent<CinemachineZoomSettings>();

            // Invokes the OnZoomAvailable event with 'settings' as a boolean
            OnZoomAvailable?.Invoke(settings);

            if (settings)
            {
                OnZoomMultiplierChanged?.Invoke(ZoomInverse);
                OnCameraZoom?.Invoke(settings.Recomposer.m_ZoomScale);
            }

            

            // For debug purposes only
            if (settings)
            {
                Debug.Log("Zoom enabled for this virtual camera");
            }
            else if (settings && !settings.enabled)
            {
                Debug.Log("Zoom available but disabled for this virtual camera");
            }
            else
            {
                Debug.Log("Zoom unavailable for this virtual camera");
            }
        }


        void Update()
        {
            // No behaviour if the current vcam has not provided zoom settings
            if (!settings) { return; }

            // No behaviour if the current vcam settings have been disabled
            if (!settings.enabled) { return; }

            // No behaviour if the game window is not focused
            if (!isFocused) { return; }

            // Suppresses the first frame after application gains focus
            if (focusGainedFrame)
            {
                focusGainedFrame = false;
                return;
            }

            HandleScroll();
        }

        public void OnApplicationFocus(bool focus)
        {
            isFocused = focus;
            focusGainedFrame = focus;
        }

        /// <summary>
        ///     Allows the user to zoom the camera using the scroll wheel
        /// </summary>
        public void HandleScroll()
        {
            if (zoomMultiplier < 1) { return; }

            float movement = -Input.mouseScrollDelta.y;

            //Get current zoom
            float currentZoom = settings.Recomposer.m_ZoomScale;

            //Add to current zoom the scroll offset reduced to the appropriate speed
            currentZoom += movement * Sensitivity;

            //Eliminate chances that the zoom value could be out of range
            currentZoom = Mathf.Clamp(currentZoom, ZoomInverse, 1f);

            //Change the zoom
            settings.Recomposer.m_ZoomScale = currentZoom;

            if (Mathf.Abs(movement) > Cinemachine.Utility.UnityVectorExtensions.Epsilon)
            {
                OnCameraZoom?.Invoke(currentZoom);
            }
        }

        public void SetZoomScale(float zoom)
        {
            SetZoomScaleWithoutNotify(zoom);
            OnCameraZoom?.Invoke(settings.Recomposer.m_ZoomScale);
        }

        public void SetZoomScaleWithoutNotify(float zoom)
        {
            settings.Recomposer.m_ZoomScale = Mathf.Clamp(zoom, ZoomInverse, 1f);
        }
    }
}