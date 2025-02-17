using System;
using System.Collections;
using Framework;
using UnityEngine;

namespace Utils
{
    public class ScreenCapture : MonoBehaviour
    {
        private Action onStart, onEnd;
        [SerializeField] Texture2D screen;

        bool capturing; 

        private void OnDestroy()
        {
            // Dispose screenshot, free memory
            if (screen != null)
            {
                Destroy(screen);
                screen = null;
            }
        }

        public void Capture(System.Action<Texture2D> callback = null)
        {
            if (capturing)
            {
                return; // Ignore request to capture when in-progress
            }
            
            capturing = true;
            StartCoroutine (Capturing(callback));
        }

        IEnumerator Capturing(System.Action<Texture2D> callback)
        {
            yield return new WaitForEndOfFrame();

            var w = Screen.width;
            var h = Screen.height;

            if (screen != null)
            {
                Destroy(screen);
                screen = null;
            }

            var screenshot = new Texture2D(w, h, TextureFormat.RGB24, false);

            var rt = new RenderTexture(w, h, 24);

            var sceneLoader = GameManager.Instance.Get<SceneLoader>();
            var camera = sceneLoader.SceneCamera;

            camera.targetTexture = rt;
            camera.Render();

            var prt = RenderTexture.active;
            RenderTexture.active = rt;

            screenshot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            screenshot.Apply();

            camera.targetTexture = null;
            RenderTexture.active = prt;

            Destroy(rt);

            onStart?.Invoke();
            yield return new WaitForEndOfFrame();
            onEnd?.Invoke();

            capturing = false;
            callback?.Invoke(screenshot);

            screen = screenshot;
        }
    }
}