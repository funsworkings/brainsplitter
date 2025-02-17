using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
    public class SceneLoader : MonoBehaviour, IManager
    {
        #region Internal

        [Serializable]
        public struct SceneObject
        {
            public string sceneTitle;
            public string sceneId;
        }
        
        #endregion

        [SerializeField] private SceneObject[] scenes;

        private TransitionManager _transitionManager;

        private int _activeSceneIndex = -1;
        private int sceneCount;
        
        // Scene properties

        public SceneController SceneController { get; private set; } = null;
            public Camera SceneCamera => SceneController?.PrimaryCamera;

        private void Start()
        {
            GameManager.Instance.Add(this);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                Debug.LogWarning("fuck");
                StartCoroutine(GoToNext());
            }
        }

        private void OnDestroy()
        {
            GameManager.Instance.Remove<SceneLoader>();
        }

        public IEnumerator Initialize()
        {
            sceneCount = scenes.Length;

            _transitionManager = GameManager.Instance.Get<TransitionManager>();
            
            onActiveSceneUpdated();
            
            yield break;
        }
        
        public void Dispose(){}

        private bool load = false;

        IEnumerator GoTo(SceneObject sceneObject) => GoTo(sceneObject.sceneId);
        IEnumerator GoTo(string sceneId)
        {
            if (load) yield break;

            load = true;
            yield return GoToScene(sceneId);
        }

        // Loads scenes asynchronously
        IEnumerator GoToScene(string sceneId)
        {
            bool waitCacheState = true;
            _transitionManager.CacheSceneState(() => waitCacheState = false);
            while (waitCacheState) yield return null;
            
            var loadOp = SceneManager.LoadSceneAsync(sceneId, LoadSceneMode.Additive);
            while (!loadOp.isDone) yield return null;
            
            _transitionManager.Transition();

            var unloadOp = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            while (!unloadOp.isDone) yield return null;
            
            onActiveSceneUpdated();
            load = false;
        }

        int GetActiveSceneIndex()
        {
            var sc = SceneManager.GetActiveScene().name;

            int index = 0;
            foreach (var s in scenes)
            {
                if (string.Equals(s.sceneId, sc)) return index;
                ++index;
            }

            return -1;
        }
        
        #region Ops

        public IEnumerator GoToStart() => GoTo(scenes[0]);

        public IEnumerator GoToNext()
        {
            var tSceneIndex = _activeSceneIndex + 1;
            if (tSceneIndex >= sceneCount) tSceneIndex = 0;
            
            Debug.LogWarning($"Try go to: {scenes[tSceneIndex].sceneId}");
            yield return GoTo(scenes[tSceneIndex]);
        }

        #endregion
        
        #region Helpers

        SceneController ScrapeSceneController(Scene sc)
        {
            var rootGameObjects = sc.GetRootGameObjects();
            foreach (var o in rootGameObjects)
            {
                var sceneController = o.GetComponentInChildren<SceneController>();
                if (sceneController != null) return sceneController;
            }
            
            return null;
        }
        
        #endregion
        
        #region Callbacks

        void onActiveSceneUpdated()
        {
            _activeSceneIndex = GetActiveSceneIndex();
            onSceneUpdated(SceneManager.GetActiveScene());  
        } 

        void onSceneUpdated(Scene scene)
        {
            SceneController = ScrapeSceneController(scene);
        }
        
        #endregion
    }
}