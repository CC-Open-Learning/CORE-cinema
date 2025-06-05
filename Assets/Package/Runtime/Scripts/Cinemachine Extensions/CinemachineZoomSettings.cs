using Cinemachine;
using UnityEngine;

namespace VARLab.CORECinema
{
    /// <summary>
    ///     Provides user zoom functionality to a specific <see cref="CinemachineVirtualCamera"/> 
    ///     through a <see cref="CinemachineRecomposer"/>
    /// </summary>
    [RequireComponent(typeof(CinemachineRecomposer))]
    public class CinemachineZoomSettings : CinemachineExtension
    {

        /// <summary>
        ///     Reference to the required <see cref="CinemachineRecomposer"/>
        ///     used to apply camera zoom
        /// </summary>
        public CinemachineRecomposer Recomposer { get; private set; }

        public bool OverrideDefaults = false;

        [Tooltip("Maximum zoom multiplier"), Range(1f, 8f)]
        public float ZoomMultiplier = 2f;


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
            Recomposer = GetComponent<CinemachineRecomposer>();
        }
    }
}