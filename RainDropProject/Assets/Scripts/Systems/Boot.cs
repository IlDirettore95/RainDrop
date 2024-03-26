using UnityEngine;
using UnityEngine.SceneManagement;

namespace GMDG.RainDrop.System
{
    /*
     * Boots the ground systems for the game, so that other entities can assume they are initialized
     */
    public class Boot : MonoBehaviour
    {
        private void Awake()
        {
            // Load all systems
            SceneManager.LoadScene("Systems", LoadSceneMode.Additive);
#if UNITY_EDITOR && DEBUG_MODE
            SceneManager.LoadScene("DebugSystem", LoadSceneMode.Additive);
#endif

            EventManager.Instance.Publish(Event.OnSystemsLoaded);
        }

        /*
         * The Start method is necessary to being sure to capture the nasty [Debug Updater] GO
         */
        private void Start()
        {
            Destroy(GameObject.Find("[Debug Updater]"));
            SceneManager.UnloadSceneAsync("Boot");

            EventManager.Instance.Publish(Event.OnBootFinished);
        }
    }
}