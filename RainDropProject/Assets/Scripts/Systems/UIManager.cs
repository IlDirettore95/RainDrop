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

        private IEnumerator _forceInputFieldFocusCoroutine;

        #region Unity_Messages

        private void Awake()
        {
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

            switch (oldState)
            {
                case GameManager.EState.Gameplay:
                    StopCoroutine(_forceInputFieldFocusCoroutine);
                    break;
            }
            
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

        // This coroutine is active to ensure that the input field is always focused
        private IEnumerator ForceInputFieldFocusCoroutine(TMP_InputField inputField)
        {
            while(true)
            {
                if (!inputField.isFocused)
                {
                    inputField.ActivateInputField();
                }
                yield return null;
            }
        }

        private void StartMainMenu()
        {
            DestroyAllCanvasChildren();
            GameObject mainMenuGO = Instantiate(MainMenuUIPrefab, CanvasGO.transform);
            mainMenuGO.GetComponentInChildren<Button>().onClick.AddListener(StartGameButtonClicked);

            // Handle cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void StartGameplay()
        {
            DestroyAllCanvasChildren();
            GameObject gameplayGO = Instantiate(GameplayUIPrefab, CanvasGO.transform);
            TMP_InputField inputField = gameplayGO.GetComponentInChildren<TMP_InputField>();

            // Auto focus Input Field
            inputField.ActivateInputField();
            _forceInputFieldFocusCoroutine = ForceInputFieldFocusCoroutine(inputField);
            StartCoroutine(_forceInputFieldFocusCoroutine);

            // Handle cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void StartGameButtonClicked()
        {
            EventManager.Instance.Publish(EEvent.OnStartGameClicked);
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
