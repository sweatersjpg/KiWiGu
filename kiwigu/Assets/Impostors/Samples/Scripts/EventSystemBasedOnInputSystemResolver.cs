using UnityEngine;

namespace Impostors.Samples
{
    [AddComponentMenu("")]
    public class EventSystemBasedOnInputSystemResolver : MonoBehaviour
    {
        [SerializeField]
        private GameObject _inputManagerEventSystem;

        [SerializeField]
        private GameObject _inputSystemEventSystem;

        private void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            Destroy(_inputManagerEventSystem);
#else
            Destroy(_inputSystemEventSystem);
#endif
        }
    }
}