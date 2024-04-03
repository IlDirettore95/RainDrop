using GMDG.RainDrop.System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace GMDG.RainDrop.UI
{
    public class GameplayPanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField InputField;
        [SerializeField] private TMP_Text PointsText;
        [SerializeField] private TMP_Text LivesText;

        private IEnumerator _forceInputFieldFocusCoroutine;

        #region Unity_Messages

        private void Awake()
        {
            Debug.Assert(InputField != null, "InputField is not serialized!");
            Debug.Assert(PointsText != null, "Points is not serialized!");

            // Auto focus Input Field
            InputField.ActivateInputField();
            _forceInputFieldFocusCoroutine = ForceInputFieldFocusCoroutine(InputField);
            StartCoroutine(_forceInputFieldFocusCoroutine);

            // Handle cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnEnable()
        {
            // Subscribes
            InputField.onSubmit.AddListener(InputFieldStringSubmitted);

            EventManager.Instance.Subscribe(EEvent.OnGameManagerPointsChanged, ScoreChanged);
            EventManager.Instance.Subscribe(EEvent.OnGameManagerLivesChanged, LivesChanged);
        }

        private void Start()
        {
            PointsText.text = GameManager.Instance.Points.ToString();
            LivesText.text = GameManager.Instance.Lives.ToString();
        }

        private void OnDisable()
        {
            // Unsubscribes
            InputField.onSubmit.RemoveListener(InputFieldStringSubmitted);

            EventManager.Instance.Unsubscribe(EEvent.OnGameManagerPointsChanged, ScoreChanged);
            EventManager.Instance.Unsubscribe(EEvent.OnGameManagerLivesChanged, LivesChanged);
        }

        #endregion

        #region Events_Listeners
        private void ScoreChanged(object[] args)
        {
            int score = (int)args[0];
            PointsText.text = score.ToString();
        }

        private void LivesChanged(object[] args)
        {
            int lives = (int)args[0];
            LivesText.text = lives.ToString();
        }

        #endregion

        #region UI_Events_Listeners
        private void InputFieldStringSubmitted(string text)
        {
            ClearInputField();

            if (!int.TryParse(text, out int result))
            {
                return;
            }

            EventManager.Instance.Publish(EEvent.OnUIManagerResultWasSubmitted, result);
        }
        #endregion

        // This coroutine is active to ensure that the input field is always focused
        private IEnumerator ForceInputFieldFocusCoroutine(TMP_InputField inputField)
        {
            while (true)
            {
                if (!inputField.isFocused)
                {
                    inputField.Select();
                    inputField.ActivateInputField();
                }
                yield return null;
            }
        }

        // Clear InputField
        private void ClearInputField()
        {
            InputField.Select();
            InputField.text = string.Empty;
        }
    }
}
