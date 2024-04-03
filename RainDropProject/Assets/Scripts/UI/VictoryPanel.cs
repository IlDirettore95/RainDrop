using GMDG.RainDrop.System;
using UnityEngine;
using UnityEngine.UI;

namespace GMDG.RainDrop.UI
{
    public class VictoryPanel : MonoBehaviour
    {
        [SerializeField] private Button GoBackToMenu;

        #region Unity_Messages

        private void Awake()
        {
            Debug.Assert(GoBackToMenu != null, "GoBackToMenu is not serialized!");

            // Handle cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnEnable()
        {
            GoBackToMenu.onClick.AddListener(GoBackToMenuButtonClicked);
        }

        private void OnDisable()
        {
            GoBackToMenu.onClick.RemoveListener(GoBackToMenuButtonClicked);
        }

        #endregion

        #region UI_Events_Listeners
        private void GoBackToMenuButtonClicked()
        {
            EventManager.Instance.Publish(EEvent.OnUIManagerGoBackToMenuClicked);
        }
        #endregion
    }
}
