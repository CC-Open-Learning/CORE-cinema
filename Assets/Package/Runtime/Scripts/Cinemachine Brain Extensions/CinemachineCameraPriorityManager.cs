using Cinemachine;
using System.Linq;
using UnityEngine;

namespace VARLab.CORECinema
{

    /// <summary>
    ///     Manages the priority of a set of <see cref="CinemachineVirtualCameraBase"/> 
    ///     objects for a <see cref="CinemachineBrain"/>
    /// </summary>
    public class CinemachineCameraPriorityManager : CinemachineBrainExtension
    {
        /// <summary> Default priority to use for all 'standby' vcams. </summary>
        protected readonly int BasePriority = 10;

        /// <summary> Priority used by the 'live' active virtual camera. </summary>
        protected readonly int HighPriority = 100;


        [SerializeField, Tooltip("If enabled, the Camera Priority Manager will find " +
            "and manage all vcams in the scene on startup")]
        protected bool AutoFindCameras = true;

        // Consider exposing this set, or managing it entirely internally
        [Tooltip("The set of managed Cinemachine cameras in the scene.")]
        public CinemachineVirtualCameraBase[] Cameras;



        /// <summary>
        ///     All managed cameras are set to the base priority on startup, 
        ///     with the exception of the highest priority camera in the scene which
        ///     remains the active virtual camera for the <see cref="CinemachineBrain"/>.
        /// </summary>
        /// <remarks>
        ///     If <see cref="AutoFindCameras"/> is enabled, the <see cref="Cameras"/> set 
        ///     will consist of all vcams in the scene. Otherwise the set of Cameras is defined
        ///     by the serialized values provided in the scene setup, and may not contain the
        ///     current active vcam.
        /// </remarks>
        protected virtual void Start()
        {
            if (AutoFindCameras)
            {
                Cameras = FindObjectsOfType<CinemachineVirtualCameraBase>();
            }

            // Level-set all managed cameras to the same base priority
            for (int index = 0; index < Cameras.Length; index++)
            {
                if (Cameras[index] as ICinemachineCamera != Brain.ActiveVirtualCamera)
                {
                    Cameras[index].Priority = BasePriority;
                }
            }
        }

        /// <summary>
        ///     Used to validate the camera being set as active.
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="outgoing"></param>
        public override void HandleVirtualCameraChanged(ICinemachineCamera incoming, ICinemachineCamera outgoing)
        {
            Debug.Log($"Current active camera is {incoming.Name}");
        }


        /// <summary>
        ///     Sets the priority of the <paramref name="camera"/> higher than all other 
        ///     managed cameras. This will likely make it the currently active vcam, unless
        ///     a camera that is not managed by the <see cref="CinemachineCameraPriorityManager"/>
        ///     has a higher priority.
        /// </summary>
        /// <remarks>
        ///     To ensure all vcams in the scene are managed, enable <see cref="AutoFindCameras"/>
        /// </remarks>
        /// <param name="camera">The vcam to be prioritised</param>
        public void ActivateCamera(ICinemachineCamera camera)
        {
            if (camera == null)
            {
                Debug.LogWarning($"Invalid object provided to the {nameof(CinemachineCameraPriorityManager)}");
                return;
            }

            if (!Cameras.Contains(camera))
            {
                Debug.LogWarning($"{camera.Name} is not managed by the {nameof(CinemachineCameraPriorityManager)}");
                return;
            }

            Brain.ActiveVirtualCamera.Priority = BasePriority;
            camera.Priority = HighPriority;
        }


        /// <summary>
        ///     Sets the priority of the camera at <paramref name="index"/> in the 
        ///     <see cref="Cameras"/> set to be higher than all other managed cameras.
        /// </summary>
        /// <param name="index">Index in the <see cref="Cameras"/> set to be prioritised</param>
        public void ActivateCamera(int index)
        {
            if (index > Cameras.Length - 1)
            {
                Debug.LogWarning($"Invalid camera index provided to the {nameof(CinemachineCameraPriorityManager)}");
                return;
            }

            ActivateCamera(Cameras[index]);
        }

        /// <summary>
        ///     Sets the priority of the camera in the <see cref="Cameras"/> set with 
        ///     the specified <paramref name="name"/> to be higher than all other managed cameras.
        /// </summary>
        /// <param name="name">Name of the vcam in the <see cref="Cameras"/> set to be prioritised</param>
        public void ActivateCamera(string name)
        {
            ActivateCamera(Cameras.DefaultIfEmpty(null).FirstOrDefault((camera) => camera.Name.Equals(name)));
        }


        /// <summary>
        ///     Sets the priority of the <paramref name="camera"/> higher than all other 
        ///     managed cameras. This will likely make it the currently active vcam, unless
        ///     a camera that is not managed by the <see cref="CinemachineCameraPriorityManager"/>
        ///     has a higher priority.
        /// </summary>
        /// <param name="camera"></param>
        public void ActivateCamera(CinemachineVirtualCameraBase camera)
        {
            ActivateCamera(camera as ICinemachineCamera);
        }
    }
}