using GMDG.RainDrop.System;
using UnityEngine;
using UnityEngine.UI;

namespace GMDG.RainDrop.UI
{
    public class PausePanel : MonoBehaviour
    {
        [SerializeField] private Button ResumeButton;
        [SerializeField] private Button BackToMenuButton;

        #region Unity_Messages

        private void Awake()
        {
            Debug.Assert(ResumeButton != null, "StartButton is not serialized!");
            Debug.Assert(BackToMenuButton != null, "QuitButton is not serialized!");

            // Handle cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnEnable()
        {
            ResumeButton.onClick.AddListener(ResumeButtonClicked);
            BackToMenuButton.onClick.AddListener(BackToMenuButtonClicked);
        }

        private void OnDisable()
        {
            ResumeButton.onClick.RemoveListener(ResumeButtonClicked);
            BackToMenuButton.onClick.RemoveListener(BackToMenuButtonClicked);
        }

        #endregion

        #region UI_Events_Listeners
        private void ResumeButtonClicked()
        {
            EventManager.Instance.Publish(EEvent.OnUIManagerResumeClicked);
        }

        private void BackToMenuButtonClicked()
        {
            EventManager.Instance.Publish(EEvent.OnUIManagerGoBackToMenuClicked);
        }
        #endregion
    }
}


