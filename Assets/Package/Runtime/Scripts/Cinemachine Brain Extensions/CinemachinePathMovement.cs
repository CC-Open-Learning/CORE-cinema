using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VARLab.CORECinema
{

    /// <summary>
    ///     Handles movement around a dolly track path a <see cref="CinemachineBrain"/>.
    /// </summary>
    /// <remarks>
    ///     A given <see cref="CinemachineVirtualCamera"/> must have its `body` property set
    ///     to Tracked Dolly and have an associated path and <see cref="CinemachinePathMovementSettings"/>
    ///     extension component in order for movement to be supported at that specific vcam.
    /// </remarks>
    [DisallowMultipleComponent]
    public class CinemachinePathMovement : CinemachineBrainExtension
    {
        /// <summary>
        ///     Provides a reference to the dolly attached to the 'body' 
        ///     of the current virtual camera. If null, no movement 
        ///     will be attempted.
        /// </summary>
        protected CinemachinePathMovementSettings settings;


        // Fields
        [SerializeField, Tooltip("Movement speed relative to a normalized track path. A movement speed of 1f means the user will move across the entire length of the path in 1 second")]
        private float movementSpeed = 0.1f;

        [SerializeField, Tooltip("Swaps left-right movement. Useful if a path has been set up counter-clockwise")]
        private bool invertMovementDirection = false;


        [Header("Input")]
        [SerializeField, Tooltip("Left Arrow and 'A' by default")]
        private KeyCode[] LeftMovementKeys = { KeyCode.A, KeyCode.LeftArrow };

        [SerializeField, Tooltip("Right Arrow and 'D' by default")]
        private KeyCode[] RightMovementKeys = { KeyCode.D, KeyCode.RightArrow };



        // Properties which can be overridden by a corresponding settings component on a per-vcam basis
        public float MovementSpeed
        {
            get => settings && settings.OverrideDefaults ? settings.MovementSpeed : movementSpeed;
            set => movementSpeed = value;
        }

        public bool InvertMovementDirection
        {
            get => settings && settings.OverrideDefaults ? settings.InvertMovementDirection : invertMovementDirection;
            set => invertMovementDirection = value;
        }

        /// <summary>
        ///     If the current virtual camera has no <see cref="CinemachinePathMovementSettings"/>, 
        ///     no movement will be attempted
        /// </summary>
        protected virtual void Update()
        {
            if (!settings) { return; }

            if (!settings.enabled) { return; }

            HandleUserInput();
        }


        /// <summary>
        ///     Looks for a <see cref="CinemachinePathMovementSettings"/> on the <paramref name="incoming"/> camera
        /// </summary>
        /// <param name="incoming">Camera with the current highest priority</param>
        /// <param name="outgoing">Camera with the previous highest priority</param>
        public override void HandleVirtualCameraChanged(ICinemachineCamera incoming, ICinemachineCamera outgoing)
        {
            if (incoming == null || incoming.VirtualCameraGameObject == null)
            {
                settings = null;
                return;
            }

            settings = incoming?.VirtualCameraGameObject.GetComponentInChildren<CinemachinePathMovementSettings>();

            if (settings)
            {
                Debug.Log("Track movement enabled for this virtual camera");
            }
            else if (settings && !settings.enabled)
            {
                Debug.Log("Track movement available but disabled for this virtual camera");
            }
            else
            {
                Debug.Log("Track movement unavailable for this virtual camera");
            }
        }

        /// <summary>
        ///     Determines which input key(s) are currently held down.
        ///     'Left' movement is prioritized over 'right' movment
        /// </summary>
        protected virtual void HandleUserInput()
        {
            foreach (KeyCode key in LeftMovementKeys)
            {
                if (Input.GetKey(key))
                {
                    Move(MoveDirection.Left);
                    return;
                }
            }

            foreach (KeyCode key in RightMovementKeys)
            {
                if (Input.GetKey(key))
                {
                    Move(MoveDirection.Right);
                    return;
                }
            }
        }


        /// <summary>
        ///     Moves the dolly camera along the path. This method can be called 
        ///     every frame to get smooth movement.
        /// </summary>
        /// <remarks>
        ///     Valid directions include <see cref="MoveDirection.Left"/>, <see cref="MoveDirection.Right"/>
        /// </remarks>
        /// <param name="direction">
        ///     The direction that the camera should move.
        ///     Valid directions include <see cref="MoveDirection.Left"/>, <see cref="MoveDirection.Right"/>
        /// </param>
        public virtual void Move(MoveDirection direction)
        {
            if (direction != MoveDirection.Left && direction != MoveDirection.Right)
            {
                return;
            }

            CinemachineTrackedDolly dolly = settings.TrackedDolly;

            if (!dolly || !dolly.m_Path) { return; }

            // Reset Z-Damping which can be modified when switching between vcams
            dolly.m_ZDamping = 1;

            float upperBound = dolly.m_PositionUnits switch
            {
                CinemachinePathBase.PositionUnits.PathUnits => dolly.m_Path.MaxPos,
                CinemachinePathBase.PositionUnits.Distance => dolly.m_Path.PathLength,
                _ => 1f
            };

            float step = Time.deltaTime                         // Normalize over time
                * MovementSpeed                                 // Base movement speed is relative to a 'normalized' path
                * upperBound                                    // Set based on position units, normalized is 1f
                * (direction == MoveDirection.Right ? -1 : 1)   // Sign indicates direction, all non-right directions are left
                * (InvertMovementDirection ? -1 : 1);           // Invert sign again if necessary


            if (dolly.m_Path.Looped)
            {
                dolly.m_PathPosition += step;
                return;
            }

            // Constrain dolly position to the min and max bounds of the path when non-looped
            dolly.m_PathPosition = Mathf.Clamp(dolly.m_PathPosition + step, dolly.m_Path.MinPos, upperBound);
        }
    }
}