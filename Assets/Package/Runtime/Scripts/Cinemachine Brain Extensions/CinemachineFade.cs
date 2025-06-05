using UnityEngine;
using System.Collections;
using Cinemachine;

namespace VARLab.CORECinema
{

    /// <summary>
    ///     CinemachineFade supports fading in and out the camera using a solid color
    ///     overlaid using immediate-mode GUI (IMGUI), which also draws over any UI 
    ///     elements in the camera view.
    /// </summary>
    /// <remarks>
    ///     It may not be ideal to use IMGUI, an alternative would be to instantiate 
    ///     and stretch an image using UGUI in a layer behind the main GUI elements.
    /// </remarks>
    public class CinemachineFade : CinemachineBrainExtension
    {

        /// <summary>
        ///     Default percentage of total blend time to wait in between fading out and back in.
        /// </summary>
        protected const float DefaultWaitPercentage = 0.1f;

        [Tooltip("Percentage of total blend time to wait in between fading out and back in")]
        [Range(0f, 1f)]
        [SerializeField] protected float waitPercentage = DefaultWaitPercentage;

        [Tooltip("Overlay color used for camera fading")]
        [SerializeField] protected Color fadeColor = Color.black;

        protected float alpha = 0;
        protected Texture2D texture;

        public Texture2D FadeTexture
        {
            get
            {
                if (texture == null)
                {
                    texture = CreateFadeTexture();
                }
                return texture;
            }
        }

        public void FadeIn(float time) { StartCoroutine(CoroutineFadeIn(time)); }

        public void FadeOut(float time) { StartCoroutine(CoroutineFadeOut(time)); }

        public void FadeBetween(float time)
        {
            float wait = time * waitPercentage;
            float fade = (time - wait) / 2f;

            StartCoroutine(CoroutineFadeBetween(fade, wait));
        }

        private IEnumerator CoroutineFadeIn(float time)
        {
            alpha = 1;
            if (time <= 0) { alpha = 0; }
            while (alpha > 0) { yield return null; alpha -= (1 / time) * Time.deltaTime; }
        }

        private IEnumerator CoroutineFadeOut(float time)
        {
            alpha = 0;
            if (time <= 0) { alpha = 1; }
            while (alpha < 1) { yield return null; alpha += (1 / time) * Time.deltaTime; }
        }


        /// <summary>
        ///     Coroutine which fades the camera to a solid color, waits for 
        ///     a predetermined <paramref name="wait"/> time, and fades back
        ///     to the camera.
        /// </summary>
        /// <param name="time">
        ///     Time in seconds of a single fade operation
        /// </param>
        /// <param name="wait">
        ///     Time in seconds to wait between fading out and fading in
        /// </param>
        /// <returns> Standard return type for Unity Coroutines </returns>
        private IEnumerator CoroutineFadeBetween(float time, float wait)
        {
            yield return CoroutineFadeOut(time);
            yield return new WaitForSeconds(wait);
            yield return CoroutineFadeIn(time);
        }


        /// <summary>
        ///     Uses immediate-mode GUI (IMGUI) to draw a single color 
        ///     texture with transparency over the entire camera output
        /// </summary>
        private void OnGUI()
        {
            if (alpha <= 0) { return; }

            GUI.color = new Color(1, 1, 1, alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), FadeTexture);
            GUI.color = Color.white;
        }

        private Texture2D CreateFadeTexture()
        {
            Texture2D texture = new(1, 1);
            texture.SetPixel(1, 1, fadeColor);
            texture.Apply();

            return texture;
        }


        /// <summary>
        ///     When changing between vcams, the camera will fade if the 
        ///     <paramref name="incoming"/> camera wants to fade in, 
        ///     or if the <paramref name="outgoing"/> camera wants to fade out.
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="outgoing"></param>
        public override void HandleVirtualCameraChanged(ICinemachineCamera incoming, ICinemachineCamera outgoing)
        {
            // Fading is ignored if there is no active blend
            if (Brain.ActiveBlend == null) { return; }

            var settingsIn = incoming?.VirtualCameraGameObject.GetComponentInChildren<CinemachineFadeSettings>();
            var settingsOut = outgoing?.VirtualCameraGameObject.GetComponentInChildren<CinemachineFadeSettings>();

            // Apply fading only if both incoming and outgoing virtual
            // cameras have 'Fade Settings' attached.
            if (!settingsIn || !settingsOut) { return; }

            // Incoming camera must support incoming fade, and similar for outgoing camera
            if (settingsIn.FadeIncoming && settingsOut.FadeOutgoing)
            {
                FadeBetween(Brain.ActiveBlend.Duration);
            }
        }
    }
}