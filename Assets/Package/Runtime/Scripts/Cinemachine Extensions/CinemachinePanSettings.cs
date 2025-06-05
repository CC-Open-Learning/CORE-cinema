using Cinemachine;
using UnityEngine;

namespace VARLab.CORECinema
{

    /// <summary>
    ///     Provides user panning functionality to a specific <see cref="CinemachineVirtualCamera"/> 
    ///     through a <see cref="CinemachineRecomposer"/>
    /// </summary>
    [RequireComponent(typeof(CinemachineRecomposer))]
    public class CinemachinePanSettings : CinemachineExtension
    {

        public float InitialPan { get; protected set; } = 0f;
        public float InitialTilt { get; protected set; } = 0f;


        /// <summary>
        ///     Reference to the required <see cref="CinemachineRecomposer"/>
        ///     used to apply panning settings
        /// </summary>
        public CinemachineRecomposer Recomposer { get; private set; }

        [Tooltip("Override the default settings specified on the CinemachineBrain")]
        public bool OverrideDefaults = false;

        [Tooltip("Maximum distance camera can rotate right and left, in degrees. " +
            "Any non-positive value means horizontal panning is unrestricted.")]
        public float HorizontalPanLimit = 70;

        [Tooltip("Maximum distance camera can pan up and down, in degrees. " +
            "Any non-positive value means vertical panning is unrestricted.")]
        public float VerticalPanLimit = 30;

        [Tooltip("Affects the speed at which the camera pans"), Range(0.1f, 2f)]
        public float MouseSensitivity = 1f;

        [Tooltip("If true, panning sensitivity is proportional to the zoom modifier")]
        public bool UseZoomScaling = true;


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

            InitialPan = Recomposer.m_Pan;
            InitialTilt = Recomposer.m_Tilt;
        }

        /// <summary>
        ///     Resets <see cref="Recomposer"/> pan settings back to their default values to ensure
        ///     the intended target is still centered in the screen (though not using CinemachineComposer targetting).
        /// </summary>
        public virtual void Reset()
        {
            if (Recomposer)
            {
                Recomposer.m_Pan = InitialPan;
                Recomposer.m_Tilt = InitialTilt;
            }
        }
    }
}