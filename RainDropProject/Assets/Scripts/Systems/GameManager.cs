using UnityEngine;

namespace GMDG.RainDrop.System
{
    public class GameManager : MonoBehaviour
    {
        public enum EState
        {
            MainMenu,
            Gameplay
        }

        private EState currentState;

        #region UnityMessages

        private void Awake()
        {
            // Subscribes
            EventManager.Instance.Subscribe(EEvent.OnStartGameClicked, StartGameClicked);

            EventManager.Instance.Publish(EEvent.OnGameManagerLoaded);
        }

        private void OnDestroy()
        {
            // Unsubscribes
            EventManager.Instance.Unsubscribe(EEvent.OnStartGameClicked, StartGameClicked);

            EventManager.Instance.Publish(EEvent.OnGameManagerDestroyed);
        }

        private void Start()
        {
            ChangeState(EState.MainMenu);
        }

        private void Update()
        {
            // State tick
            switch (currentState)
            {
                case EState.MainMenu:
                    break;

                case EState.Gameplay:
                    break;
            }
        }

        #endregion

        #region Event_Listeners

        private void StartGameClicked(object[] args)
        {
            ChangeState(EState.Gameplay);
        }

        #endregion

        private void ChangeState(EState newState)
        {
            EState oldState = currentState;
            currentState = newState;

            // Code on exit
            switch(oldState)
            {
                case EState.MainMenu:
                    break;

                case EState.Gameplay:
                    break;
            }

            // Code on enter
            switch (newState)
            {
                case EState.MainMenu:
                    break;

                case EState.Gameplay:
                    break;
            }

            // Code for specific transitions
            // ...


            EventManager.Instance.Publish(EEvent.OnGameManagerChangedState, oldState, newState);
        }


    }
}