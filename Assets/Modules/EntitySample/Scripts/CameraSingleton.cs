namespace Modules.EntitySample {
    // There are many ways of getting access to the main camera, but the approach using
    // a singleton (as we use here) works for any kind of MonoBehaviour.
    internal class CameraSingleton : UnityEngine.MonoBehaviour {
        public static UnityEngine.Camera Instance;

        private void Awake() {
            Instance = GetComponent<UnityEngine.Camera>();
        }
    }
}