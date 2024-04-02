using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GMDG.RainDrop.System
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject CanvasGO;

        [SerializeField] private GameObject MainMenuUIPrefab;
        [SerializeField] private GameObject GameplayUIPrefab;

        #region Unity_Messages

        private void Awake()
        {
            Debug.Assert(CanvasGO != null, "There is not a Canvas linked to UI Manager!");
            Debug.Assert(MainMenuUIPrefab != null, "There is not a MainMenuUIPrefab linked to UI Manager!");
            Debug.Assert(GameplayUIPrefab != null, "There is not a GameplayUIPrefab linked to UI Manager!");

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
                    StartMainMenu();
                    break;
                case GameManager.EState.Gameplay:
                    StartGameplay();
                    break;
            }
        }

        #endregion

        private void StartMainMenu()
        {
            DestroyAllCanvasChildren();
            Instantiate(MainMenuUIPrefab, CanvasGO.transform);
        }

        private void StartGameplay()
        {
            DestroyAllCanvasChildren();
            Instantiate(GameplayUIPrefab, CanvasGO.transform);
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
