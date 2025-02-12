using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance = null;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame(); // Slight delay for other manager classes to init themselves
            
            var cacheManagers = _managerLookup.Values.ToArray();
            foreach (var manager in cacheManagers)
            {
                yield return manager.Initialize(); // Load manager class
            }

            yield return Get<SceneLoader>().GoToStart(); // Load into right scene
        }

        #region Manager impl

        private Dictionary<Type, IManager> _managerLookup = new Dictionary<Type, IManager>();

        public T Get<T>() where T:IManager
        {
            if (_managerLookup.TryGetValue(typeof(T), out var manager)) return (T)manager;
            return default;
        }

        public void Add<T>(T instance) where T : IManager
        {
            if (!_managerLookup.ContainsKey(typeof(T)))
            {
                _managerLookup.Add(typeof(T), instance);
            }
        }

        public void Remove<T>() where T : IManager
        {
            if (_managerLookup.ContainsKey(typeof(T)))
            {
                _managerLookup.Remove(typeof(T));
            }
        }

        #endregion
    }
}