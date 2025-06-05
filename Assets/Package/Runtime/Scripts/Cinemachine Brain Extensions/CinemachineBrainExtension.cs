using Cinemachine;
using UnityEngine;

namespace VARLab.CORECinema
{
    /// <summary>
    ///     Extends the functionality of the <see cref="CinemachineBrain"/> with user interactions
    ///     that can manipulate the currently active <see cref="CinemachineVirtualCamera"/>. 
    /// </summary>
    [RequireComponent(typeof(CinemachineBrain))]
    public abstract class CinemachineBrainExtension : MonoBehaviour, ICinemachineBrainExtension
    {
        /// <summary> 
        ///     Private field for the CinemachineBrain reference 
        ///     that must be attached to this GameObject
        /// </summary>
        private CinemachineBrain brain;

        /// <summary>Get the <see cref="CinemachineBrain"/> to which this extension is attached</summary>
        public CinemachineBrain Brain
        {
            get
            {
                if (!brain) { brain = GetComponent<CinemachineBrain>(); }
                return brain;
            }
        }

        /// <summary>
        ///     Event listener which corresponds to the <see cref="CinemachineBrain.m_CameraActivatedEvent"/> event.
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="outgoing"></param>
        public abstract void HandleVirtualCameraChanged(ICinemachineCamera incoming, ICinemachineCamera outgoing);


        /// <summary>
        ///     Subscribes to <see cref="Brain.m_CameraActivatedEvent"/> on startup.
        /// </summary>
        /// <remarks>
        ///     Child classes must call this base method
        /// </remarks>
        protected virtual void OnEnable()
        {
            Subscribe();
        }

        /// <summary>
        ///     Unsubscribes from <see cref="Brain.m_CameraActivatedEvent"/> on deletion.
        /// </summary>
        /// <remarks>
        ///     Child classes must call this base method
        /// </remarks>
        protected virtual void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>
        ///     Subscribes to the <see cref="CinemachineBrain.m_CameraActivatedEvent"/> event
        /// </summary>
        protected virtual void Subscribe()
        {
            Brain.m_CameraActivatedEvent?.AddListener(HandleVirtualCameraChanged);
        }

        /// <summary>
        ///     Unsubscribes from the <see cref="CinemachineBrain.m_CameraActivatedEvent"/> event
        /// </summary>
        protected virtual void Unsubscribe()
        {
            Brain.m_CameraActivatedEvent?.RemoveListener(HandleVirtualCameraChanged);
        }
    }
}