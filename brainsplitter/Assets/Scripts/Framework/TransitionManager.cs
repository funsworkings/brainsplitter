using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ScreenCapture = Utils.ScreenCapture;

namespace Framework
{
    public class TransitionManager : MonoBehaviour, IManager
    {
        [SerializeField] private Canvas _transitionCanvas;
        [SerializeField] private CanvasGroup _transitionCanvasGroup;

        private ScreenCapture _screenCapture;
        
        private Texture2D _screenshot;
        [SerializeField] private RawImage _screenCaptureImage;

        [SerializeField] private float _defaultTransitionDuration = 2f;

        private void Start()
        {
            GameManager.Instance.Add(this);
        }

        #region IManager
        
        public IEnumerator Initialize()
        {
            _screenCapture = FindObjectOfType<ScreenCapture>();

            SetAlpha(0f);
            
            yield break;
        }

        public void Dispose()
        {
            
        }
        
        #endregion
        
        #region Ops

        public void CacheSceneState(System.Action onReady = null)
        {
            _screenCapture.Capture((result) =>
            {
                _screenCaptureImage.texture = result;
                onReady?.Invoke();
            });
        }

        public Coroutine Transition(float duration = 0f)
        {
            if (duration <= 0f) duration = _defaultTransitionDuration;

            //_screenshot = null;
            return StartCoroutine(TransitionRoutine(duration));
        }

        IEnumerator TransitionRoutine(float duration)
        {
            /*
            _screenCapture.Capture();
            while (_screenshot == null) yield return null;
            _screenCaptureImage.texture = _screenshot;
            SetAlpha(1f);*/
            
            yield return LerpAlphaRoutine(1f, 0f, duration);
        }
        
        #endregion

        #region Vis

        void SetAlpha(float alpha)
        {
            //alpha = Mathf.Clamp01(alpha); Safeguard on safe val for alpha 0-1
            _transitionCanvasGroup.alpha = alpha;
            _transitionCanvasGroup.interactable = alpha > 0f;
            _transitionCanvasGroup.blocksRaycasts = alpha > 0f;
        }

        IEnumerator LerpAlphaRoutine(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                SetAlpha(to);
                yield break;
            }
            
            SetAlpha(from); // Initial assignment of alpha val

            float i = 0f, t = 0f;
            while (i < 1f)
            {
                SetAlpha(Mathf.Lerp(from, to, i));

                t += Time.deltaTime;
                i = Mathf.Clamp01(t / duration);
                
                yield return null;
            }
            SetAlpha(to);
        }
        
        #endregion
    }
}