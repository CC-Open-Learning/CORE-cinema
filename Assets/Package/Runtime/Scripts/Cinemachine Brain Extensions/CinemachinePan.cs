using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace VARLab.CORECinema
{

    /// <summary>
    ///     Handles user panning with mouse movement for a <see cref="CinemachineBrain"/>.
    /// </summary>
    /// <remarks>
    ///     A given <see cref="CinemachineVirtualCamera"/> must have a 
    ///     <see cref="CinemachinePanSettings"/> extension component in order for panning
    ///     to be supported at that specific vcam.
    /// </remarks>
    [DisallowMultipleComponent]
    public class CinemachinePan : CinemachineBrainExtension
    {
        protected const int LeftMouseButton = 0;

        protected const int NoDamping = 1;
        protected const float DampingScale = 5f;
        protected const float AnimationTime = 0.4f;

        /// <summary>
        ///     Provides references to override settings and the <see cref="CinemachineRecomposer"/>
        ///     used to apply camera panning
        /// </summary>
        protected CinemachinePanSettings settings;

        protected bool shouldPan;

        private float lastDampingHorizontal;
        private float lastDampingVertical;


        [SerializeField, Tooltip("Maximum distance camera can rotate right and left, in degrees. " +
            "Any non-positive value means horizontal panning is unrestricted.")]
        protected float horizontalPanLimit = 70;

        [SerializeField, Tooltip("Maximum distance camera can pan up and down, in degrees. " +
            "Any non-positive value means vertical panning is unrestricted.")]
        protected float verticalPanLimit = 30;

        [SerializeField, Tooltip("Affects the speed at which the camera pans"), Range(0.1f, 2f)]
        protected float mouseSensitivity = 1f;

        [SerializeField, Tooltip("If true, panning sensitivity is proportional to the zoom modifier")]
        protected bool useZoomScaling = true;

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


        /// <summary>
        ///     Defines a platform-specific camera panning sensitivity modifier.
        /// </summary>
        /// <remarks>
        ///     This is due to the fact that the WebGL player seems to nearly quadruple 
        ///     the magnitude of mouse X and Y inputs as the cursor moves, so the 
        ///     sensitivity needs to be reduced by that factor.
        /// </remarks>
        public float PlatformSensitivityModifier =>
            Application.platform == RuntimePlatform.WebGLPlayer
            ? 0.25f
            : 1f;

        public float HorizontalPanLimit
        {
            get => settings && settings.OverrideDefaults ? settings.HorizontalPanLimit : horizontalPanLimit;
            set => horizontalPanLimit = value;
        }

        public float VerticalPanLimit
        {
            get => settings && settings.OverrideDefaults ? settings.VerticalPanLimit : verticalPanLimit;
            set => verticalPanLimit = value;
        }

        public float MouseSensitivity
        {
            get => settings && settings.OverrideDefaults ? settings.MouseSensitivity : mouseSensitivity;
            set => mouseSensitivity = value;
        }

        public bool UseZoomScaling
        {
            get => settings && settings.OverrideDefaults ? settings.UseZoomScaling : useZoomScaling;
            set => useZoomScaling = value;
        }


        [Header("Events")]

        [Tooltip("Invoked when the user starts or stops panning the camera")]
        public UnityEvent<bool> OnPanning;

        [Tooltip("Invoked when the current active vcam changes, " +
            "indicating if panning functionality is available to the user")]
        public UnityEvent<bool> OnPanningAvailable;

        protected virtual void Awake() { }

        /// <summary>
        ///     Sets the <see cref="focusGainedFrame"/> flag whenever the application
        ///     gains focus
        /// </summary>
        /// <param name="focus">
        ///     Indicates whether focus is gained (<c>true</c>) or lost (<c>false</c>)
        /// </param>
        protected virtual void OnApplicationFocus(bool focus)
        {
            focusGainedFrame = focus;
        }

        /// <summary>
        ///     Each frame tracks panning from mouse drag if the corresponding
        ///     <see cref="CinemachinePanSettings"/> component is present and enabled
        /// </summary>
        protected virtual void Update()
        {
            if (!settings) { return; }

            if (!settings.enabled) { return; }

            // Suppresses the first frame after application gains focus
            if (focusGainedFrame) 
            { 
                focusGainedFrame = false; 
                return; 
            }

            HandlePanningFromMouseDrag();
        }


        /// <summary>
        ///     Detects if the currently active virtual camera supports panning functionality
        /// </summary>
        /// <param name="incoming">The newly active vcam</param>
        /// <param name="outgoing">The previously active vcam</param>
        public override void HandleVirtualCameraChanged(ICinemachineCamera incoming, ICinemachineCamera outgoing)
        {
            ResetDamping();

            if (incoming == null || incoming.VirtualCameraGameObject == null)
            {
                settings = null;
                return;
            }

            settings = incoming?.VirtualCameraGameObject.GetComponentInChildren<CinemachinePanSettings>();

            // Ensures panning settings are reset when changing cameras
            if (settings) { settings.Reset(); }

            // Invokes OnPanningAvailable with 'settings' as boolean
            OnPanningAvailable?.Invoke(settings);


#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // DEBUG only
            if (settings)
            {
                Debug.Log("Panning enabled for this virtual camera");
            }
            else if (settings && !settings.enabled)
            {
                Debug.Log("Panning available but disabled for this virtual camera");
            }
            else
            {
                Debug.Log("Panning unavailable for this virtual camera");
            }
#endif
        }


        /// <summary>
        ///     Stops any active damping coroutines and clears cached damping values.
        /// </summary>
        /// <remarks>
        ///     Useful when changing between active virtual cameras.
        /// </remarks>
        protected virtual void ResetDamping()
        {
            StopAllCoroutines();
            lastDampingHorizontal = NoDamping;
            lastDampingVertical = NoDamping;
        }


        /// <summary>
        ///     Manage the panning of the camera as the user clicks and drags the mouse
        ///     across the screen by modifying the properties of the current virtual camera's
        ///     <see cref="CinemachineRecomposer"/>
        /// </summary>
        protected virtual void HandlePanningFromMouseDrag()
        {

            // Handle initial mouse press
            if (Input.GetMouseButtonDown(LeftMouseButton))
            {
                shouldPan = !IsMouseOverUI();

                if (shouldPan)
                {
                    OnPanning?.Invoke(true);
                }
            }


            // Handle panning each frame, if the user did not initially click on a UI element
            if (Input.GetMouseButton(LeftMouseButton) && shouldPan)
            {
                float sensitivity = MouseSensitivity * PlatformSensitivityModifier * (UseZoomScaling ? settings.Recomposer.m_ZoomScale : 1);

                //get the input from the mouse and set rotations
                settings.Recomposer.m_Pan -= Input.GetAxis("Mouse X") * sensitivity
                    * CalculateDamping(settings.Recomposer.m_Pan, HorizontalPanLimit, ref lastDampingHorizontal);

                settings.Recomposer.m_Tilt += Input.GetAxis("Mouse Y") * sensitivity
                    * CalculateDamping(settings.Recomposer.m_Tilt, VerticalPanLimit, ref lastDampingVertical);
            }


            // Handle the last frame of panning, animating the camera back into the
            // panning bounds if the user has gone past the bounds
            if (Input.GetMouseButtonUp(LeftMouseButton))
            {
                //ensure that transitions to other virtual cameras won't spin around 
                settings.Recomposer.m_Pan = FixRotation(settings.Recomposer.m_Pan);
                settings.Recomposer.m_Tilt = FixRotation(settings.Recomposer.m_Tilt);

                // Animate each camera axis back into bounds separately
                if (lastDampingHorizontal != NoDamping)
                {
                    StartCoroutine(AnimateAxisBackIntoBounds(Axis.X, settings.Recomposer.m_Pan, HorizontalPanLimit));
                }

                if (lastDampingVertical != NoDamping)
                {
                    StartCoroutine(AnimateAxisBackIntoBounds(Axis.Y, settings.Recomposer.m_Tilt, VerticalPanLimit));
                }

                if (shouldPan)
                {
                    // Event indicates that panning has stopped
                    OnPanning?.Invoke(false);
                }
            }
        }


        /// <summary>
        ///     Coroutine used to smoothly animate the camera back into bounds if 
        ///     it has been dragged outside the allowed angles
        /// </summary>
        /// <param name="axis">Supports Axis.X for pan, Axis.Y for tilt</param>
        /// <param name="start">Current angle for the property (pan or tilt)</param>
        /// <param name="limit">Maximum allowed angle for the property (pan or tilt)</param>
        /// <returns>Iterator used for Unity coroutines</returns>
        protected virtual IEnumerator AnimateAxisBackIntoBounds(Axis axis, float start, float limit)
        {
            float end = limit > 0
                ? Mathf.Clamp(start, -limit, limit)
                : start;

            float startingTime = Time.time;
            float endingTime = startingTime + AnimationTime;

            while (Time.time <= endingTime)
            {
                float timeRatio = (Time.time - startingTime) / AnimationTime;

                switch (axis)
                {
                    case Axis.X: // pan
                        settings.Recomposer.m_Pan = Mathf.SmoothStep(start, end, timeRatio);
                        break;
                    case Axis.Y: // tilt
                        settings.Recomposer.m_Tilt = Mathf.SmoothStep(start, end, timeRatio);
                        break;
                    default:
                        break;
                }

                yield return new WaitForEndOfFrame();
            }

            //ensure that it ends exactly at the ending points.
            switch (axis)
            {
                case Axis.X: // pan
                    settings.Recomposer.m_Pan = end;
                    lastDampingHorizontal = NoDamping;
                    break;
                case Axis.Y: // tilt
                    settings.Recomposer.m_Tilt = end;
                    lastDampingVertical = NoDamping;
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        ///     Determines how the camera panning speed should be slowed down as it is 
        ///     dragged out of the max camera panning bounds. The value that was calculated 
        ///     in the previous frame is used to determine if the camera is being dragged 
        ///     back into the bounds. If this is happening, then no damping is a applied.
        /// </summary>
        /// <param name="value">Current camera angle, either tilt or pan</param>
        /// <param name="max">Maximum camera angle, either tilt or pan</param>
        /// <param name="last">Damping calculated in the previous frame</param>
        /// <returns>
        ///     Value between 0 - 1 which can be multiplied with the current panning 
        ///     value to slow camera movement. If no damping is needed, the constant
        ///     <see cref="NoDamping"/> (1) is returned.
        /// </returns>
        protected virtual float CalculateDamping(float value, float max, ref float last)
        {
            if (max <= 0 || Mathf.Abs(value) <= max)
            {
                return NoDamping;
            }

            //check if they are out of bounds
            //calculate how far the passed the limit the camera is
            float overscan = Mathf.Clamp(Mathf.Abs(value) - max, 0, float.PositiveInfinity);

            //A simple equations the where the higher the totalPassedLimit, the lower the output is between 1 and 0
            float damping = 1 / (DampingScale * overscan + 1);

            //only use the new value if it is less than the old value
            // so only if they are panning further outside the view
            float output =
                damping <= last
                ? damping
                : NoDamping;

            //save the dampening 
            last = damping;

            return output;
        }


        #region Static Methods

        /// <summary>
        ///     For a given <paramref name="value"/>, returns an equivalent angle 
        ///     in degrees between -180 and 180
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        ///     An angle equivalent to <paramref name="value"/> between -180 and 180
        /// </returns>
        public static float FixRotation(float value)
        {
            value %= 360;

            if (Mathf.Abs(value) > 180)
            {
                bool wasNegative = false;

                if (value < 0)
                {
                    wasNegative = true;
                    value *= -1;
                }

                if (value > 180)
                {
                    value = -180 + (value - 180);
                }

                if (wasNegative)
                {
                    value *= -1;
                }
            }

            return value;
        }


        /// <summary>
        ///     Determines whether the mouse is over a UI element by
        ///     checking for the presence of a <see cref="RectTransform"/>
        /// </summary>
        /// <remarks>
        ///     Solution suggested by this 
        ///     <see href="https://forum.unity.com/threads/eventsystems-how-to-detect-if-the-mouse-is-over-ui-elements.541083/">
        ///         Unity forum
        ///     </see> post
        /// </remarks>
        /// <returns>
        ///     Boolean indicating whether the mouse is over a UI element
        /// </returns>
        public static bool IsMouseOverUI()
        {
            PointerEventData eventData = new(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(eventData, results);

            // for-loop avoids using Linq library
            for (int item = 0; item < results.Count; item++)
            {
                if (results[item].gameObject.GetComponent<RectTransform>())
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
