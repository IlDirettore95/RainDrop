using GMDG.RainDrop.System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GMDG.RainDrop.UI
{
    public class MainMenuPanel : MonoBehaviour
    {
        [SerializeField] private Button StartButton;
        [SerializeField] private Button QuitButton;
        [SerializeField] private TMP_Text BestScoreText;

        #region Unity_Messages

        private void Awake()
        {
            Debug.Assert(StartButton != null, "StartButton is not serialized!");
            Debug.Assert(QuitButton != null, "QuitButton is not serialized!");
            Debug.Assert(BestScoreText != null, "BestScoreText is not serialized!");

            // Handle cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Start()
        {
            BestScoreText.text = GameManager.Instance.BestScore.ToString();
        }

        private void OnEnable()
        {
            StartButton.onClick.AddListener(StartGameButtonClicked);
            QuitButton.onClick.AddListener(QuitButtonClicked);
        }

        private void OnDisable()
        {
            StartButton.onClick.RemoveListener(StartGameButtonClicked);
            QuitButton.onClick.RemoveListener(QuitButtonClicked);
        }

        #endregion

        #region UI_Events_Listeners
        private void StartGameButtonClicked()
        {
            EventManager.Instance.Publish(EEvent.OnUIManagerStartGameClicked);
        }

        private void QuitButtonClicked()
        {
            EventManager.Instance.Publish(EEvent.OnUIManagerQuitButtonClicked);
        }
        #endregion
    }
}