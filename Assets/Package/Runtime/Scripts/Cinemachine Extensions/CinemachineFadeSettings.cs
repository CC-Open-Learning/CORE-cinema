using Cinemachine;

namespace VARLab.CORECinema
{
    public class CinemachineFadeSettings : CinemachineExtension
    {

        public bool FadeIncoming = true;
        public bool FadeOutgoing = true;

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
    }
}
