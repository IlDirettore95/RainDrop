using UnityEngine;

namespace GMDG.RainDrop.System
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject CanvasGO;

        [SerializeField] private GameObject MainMenuUIPrefab;
        [SerializeField] private GameObject GameplayUIPrefab;
        [SerializeField] private GameObject PauseUIPrefab;
        [SerializeField] private GameObject GameOverUIPrefab;

        #region Unity_Messages

        private void Awake()
        {
            Debug.Assert(CanvasGO != null, "There is not a Canvas linked to UI Manager!");
            Debug.Assert(MainMenuUIPrefab != null, "There is not a MainMenuUIPrefab linked to UI Manager!");
            Debug.Assert(GameplayUIPrefab != null, "There is not a GameplayUIPrefab linked to UI Manager!");
            Debug.Assert(PauseUIPrefab != null, "There is not a PauseUIPrefab linked to UI Manager!");
            Debug.Assert(GameOverUIPrefab != null, "There is not a GameOverUIPrefab linked to UI Manager!");

            // Subscribes
            EventManager.Instance.Subscribe(EEvent.OnGameManagerChangedState, GameManagerStateChanged);

            EventManager.Instance.Publish(EEvent.OnUIManagerLoaded);
        }

        private void OnDestroy()
        {
            // Unsubscribes
            EventManager.Instance.Unsubscribe(EEvent.OnGameManagerChangedState, GameManagerStateChanged);

            EventManager.Instance.Publish(EEvent.OnUIManagerDestroyed);
        }

        #endregion

        #region Event_Listeners

        private void GameManagerStateChanged(object[] args)
        {
            GameManager.EState oldState = (GameManager.EState)args[0];
            GameManager.EState newState = (GameManager.EState)args[1];
            
            switch (newState)
            {
                case GameManager.EState.MainMenu:
                    CreatePanel(MainMenuUIPrefab);
                    break;
                case GameManager.EState.Gameplay:
                    CreatePanel(GameplayUIPrefab);
                    break;
                case GameManager.EState.Pause:
                    CreatePanel(PauseUIPrefab);
                    break;
                case GameManager.EState.GameOver:
                    CreatePanel(GameOverUIPrefab);
                    break;
            }
        }

        #endregion

        private void CreatePanel(GameObject prefab)
        {
            DestroyAllCanvasChildren();
            Instantiate(prefab, CanvasGO.transform);
        }

        // Clear all child UI elements
        private void DestroyAllCanvasChildren()
        {
            for (int i = CanvasGO.transform.childCount - 1; i >= 0; i--) 
            {
                Destroy(CanvasGO.transform.GetChild(i).gameObject);
            }
        }
    }
}
