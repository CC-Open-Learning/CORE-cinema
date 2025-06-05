using Cinemachine;
using UnityEngine;
using UnityEngine.AI;


namespace VARLab.CORECinema
{

    /// <summary>
    ///     Uses a <see cref="NavMeshAgent"/> to direct a virtual camera around a 3D space
    ///     and avoid colliding with or clipping through objects as it moves.
    /// </summary>
    [AddComponentMenu("")] // Can only be added as an extension to a virtual camera
    [ExecuteAlways]
    public class CinemachineNavAgent : CinemachineExtension
    {
        [Tooltip("NavMeshAgent responsible for moving the camera around the scene")]
        public NavMeshAgent NavAgent;

        public CinemachineNavAgentTarget ActiveTarget;


        public float RefreshRate = 0.2f;
        protected float m_RefreshElapsed = 0f;


        
        /// <summary>
        ///     Attempts to attach a <see cref="NavMeshAgent"/> to this script from a
        ///     parent object
        /// </summary>
        protected virtual void Reset()
        {
            NavAgent = GetComponentInParent<NavMeshAgent>();
        }

        /// <summary>
        ///     Sets position and LookAt Target
        /// </summary>
        protected virtual void OnValidate()
        {
            if (NavAgent && VirtualCamera && ActiveTarget)
            {
                VirtualCamera.LookAt = ActiveTarget.LookAtTarget;
                if (!Application.isPlaying) { NavAgent.Warp(ActiveTarget.transform.position); }
            }
        }


        protected virtual void Update()
        {

#if UNITY_EDITOR

            // This block of code runs only in the Editor to ensure that
            // the NavAgent "snaps" to the current target position as it 
            // is moved around the scene/
            if (!Application.isPlaying && ActiveTarget && ActiveTarget.transform.hasChanged)
            {
                SwitchCameraTarget(ActiveTarget, true);
                return;
            }
#endif

            // Checking the tracked target less than once per frame ensures
            // that the target is still reasonably tracked but avoids some 
            // wasteful operations, as it is possible but unlikely that the 
            // target position for the NavAgent will be updated
            m_RefreshElapsed += Time.deltaTime;

            if (m_RefreshElapsed >= RefreshRate)
            {
                TrackTarget();
                m_RefreshElapsed = 0f;
            }

        }


        /// <summary>
        ///     Assigns the position and LookAt target of the current virtual camera
        ///     based on the active <see cref="CinemachineNavAgentTarget"/>
        /// </summary>
        /// <remarks>
        ///     Assumes that the NavAgent is on a NavMesh and will assign it a new
        ///     target position to move towards.
        /// </remarks>
        public virtual void TrackTarget()
        {
            if (!ActiveTarget) { return; }

            if (VirtualCamera.LookAt != ActiveTarget.LookAtTarget)
            {
                VirtualCamera.LookAt = ActiveTarget.LookAtTarget;
            }

            if (NavAgent && NavAgent.isOnNavMesh)
            {
                NavAgent.SetDestination(ActiveTarget.transform.position);
            }
        }


        /// <summary>
        ///     Receives a new <see cref="CinemachineNavAgentTarget"/> to track.
        ///     The NavAgent is not warped to the target and instead attempts to
        ///     move there in realtime.
        /// </summary>
        /// <param name="target">
        ///     The new target position to move towards, also provides a LookAt target
        /// </param>
        public void SwitchCameraTarget(CinemachineNavAgentTarget target)
        {
            SwitchCameraTarget(target, false);
        }


        /// <summary>
        ///     Receives a new <see cref="CinemachineNavAgentTarget"/> to track.
        ///     NavAgent is optionally warped directly to the target position if the
        ///     <paramref name="warp"/> flag is set.
        /// </summary>
        /// <param name="target">
        ///     The new target position to move towards, also provides a LookAt target
        /// </param>
        /// <param name="warp">
        ///     Indicates whether the NavAgent should be instantly moved to the target position
        /// </param>
        public void SwitchCameraTarget(CinemachineNavAgentTarget target, bool warp)
        {
            ActiveTarget = target;

            if (target && !target.LookAtTarget)
            {
                Debug.LogWarning("Moving to a NavCamera position with no LookAt target. " +
                    "This may produce unexpected behaviour.");
            }

            if (warp) 
            {
                NavAgent.Warp(target.transform.position);
            }

            TrackTarget();
        }


        public virtual void NavAgentCameraLiveHandler(ICinemachineCamera incoming, ICinemachineCamera outgoing)
        {
            if (incoming == VirtualCamera as ICinemachineCamera)
            {
                Debug.Log("Cinemachine enabled the NavAgent Camera");
            }
            else
            {
                Debug.LogWarning("Cinemachine enabled the NavAgent Camera at the wrong time");
            }
        }


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
