using UnityEngine;

namespace GMDG.RainDrop.System
{
    public class GameManager : MonoBehaviour
    {
        private State currentState;

        #region UnityMessages

        private void Awake()
        {
            EventManager.Instance.Publish(Event.OnGameManagerLoaded);
        }

        private void OnDestroy()
        {
            EventManager.Instance.Publish(Event.OnGameManagerDestroyed);
        }

        private void Start()
        {
            ChangeState(State.MainMenu);
        }

        private void Update()
        {
            // State tick
            switch (currentState)
            {
                case State.MainMenu:
                    break;

                case State.Gameplay:
                    break;
            }
        }

        #endregion

        private void ChangeState(State newState)
        {
            State oldState = currentState;
            currentState = newState;

            // Code on exit
            switch(oldState)
            {
                case State.MainMenu:
                    break;

                case State.Gameplay:
                    break;
            }

            // Code on enter
            switch (newState)
            {
                case State.MainMenu:
                    break;

                case State.Gameplay:
                    break;
            }

            // Code for specific transitions
            // ...


            EventManager.Instance.Publish(Event.OnGameManagerChangedState, oldState, newState);
        }

        public enum State
        {
            MainMenu,
            Gameplay
        }
    }
}