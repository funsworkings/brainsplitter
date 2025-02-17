using UnityEngine;

namespace Framework
{
    public class SceneController : MonoBehaviour
    {
        [SerializeField] private Camera _primaryCamera;
        public Camera PrimaryCamera => _primaryCamera;
    }
}