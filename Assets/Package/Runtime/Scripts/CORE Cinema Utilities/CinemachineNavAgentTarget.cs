using UnityEngine;

namespace VARLab.CORECinema
{

    /// <summary>
    ///     Defines a target for a <see cref="CinemachineNavAgent"/>
    /// </summary>
    /// <remarks>
    ///     The <see cref="GameObject.transform"/> of this object is used 
    ///     as the camera position, while the <see cref="LookAtTarget"/> 
    ///     is assigned to the LookAt field of the virtual camera.
    ///     
    /// </remarks>
    public class CinemachineNavAgentTarget : MonoBehaviour
    {
        public Transform LookAtTarget;
    }
}