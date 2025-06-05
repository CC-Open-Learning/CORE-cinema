using Cinemachine;
using UnityEngine;

namespace VARLab.CORECinema
{

    /// <summary>
    ///     Provides movement along a path specific <see cref="CinemachineVirtualCamera"/> 
    ///     with an attached a <see cref="CinemachineTrackedDolly"/>.
    /// </summary>
    /// <remarks>
    ///     The `body` of the vcam must be set to Tracked Dolly and a `path` must be provided.
    /// </remarks>
    public class CinemachinePathMovementSettings : CinemachineExtension
    {

        /// <summary>
        ///     Reference to the required <see cref="CinemachineTrackedDolly"/>
        ///     used to move the camera around the path
        /// </summary>
        public CinemachineTrackedDolly TrackedDolly { get; private set; }

        // Properties
        [Tooltip("Override the default settings specified on the CinemachineBrain")]
        public bool OverrideDefaults = false;

        [Tooltip("Movement speed relative to a normalized track path. A movement speed of 1f means the user will move across the entire length of the path in 1 second")]
        public float MovementSpeed = 0.1f;

        [Tooltip("Swaps left-right movement. Useful if a path has been set up counter-clockwise")]
        public bool InvertMovementDirection = false;


        /// <summary>
        ///     Required method for <see cref="CinemachineExtension"/>. 
        ///     No pipeline stage needed at this time.
        /// </summary>
        /// <param name="vcam"></param>
        /// <param name="stage"></param>
        /// <param name="state"></param>
        /// <param name="deltaTime"></param>
        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            //
        }

        protected override void Awake()
        {
            base.Awake();
            TrackedDolly = GetComponentInChildren<CinemachineTrackedDolly>();
        }
    }
}