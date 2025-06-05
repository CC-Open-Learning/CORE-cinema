using Cinemachine;
using UnityEngine;

namespace VARLab.CORECinema
{
    /// <summary>
    ///     Extends the functionality of the <see cref="CinemachineBrain"/> with user interactions
    ///     that can manipulate the currently active <see cref="CinemachineVirtualCamera"/>. 
    /// </summary>
    public interface ICinemachineBrainExtension
    {

        /// <summary>Get the <see cref="CinemachineBrain"/> to which this extension is attached</summary>
        public CinemachineBrain Brain { get; }


        /// <summary>
        ///     Event listener which corresponds to the <see cref="CinemachineBrain.m_CameraActivatedEvent"/> event.
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="outgoing"></param>
        public abstract void HandleVirtualCameraChanged(ICinemachineCamera incoming, ICinemachineCamera outgoing);
    }
}