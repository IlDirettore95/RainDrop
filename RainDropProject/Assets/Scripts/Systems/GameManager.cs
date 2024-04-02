using UnityEngine;

namespace GMDG.RainDrop.System
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public enum EState
        {
            MainMenu,
            Gameplay
        }

        private EState _currentState;
        public EState CurrentState
        {
            get
            {
                return _currentState;
            }
            private set
            {
                EState oldState = _currentState;
                _currentState = value;
                EventManager.Instance.Publish(EEvent.OnGameManagerChangedState, oldState, value);
            }
        }

        private int _points;
        public int Points
        {
            get 
            { 
                return _points;
            }
            private set
            {
                _points = value;
                EventManager.Instance.Publish(EEvent.OnGameManagerChangedPoints, _points);
            }
        }

        #region UnityMessages

        private void Awake()
        {
            // Singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else
            { 
                Destroy(this);
            }

            // Subscribes
            EventManager.Instance.Subscribe(EEvent.OnUIManagerStartGameClicked, StartGameClicked);
            EventManager.Instance.Subscribe(EEvent.OnLevelManagerDropExplosion, DropExploded);
            EventManager.Instance.Subscribe(EEvent.OnLevelManagerGoldenDropExplosion, DropExploded);

            EventManager.Instance.Publish(EEvent.OnGameManagerLoaded);
        }

        private void OnDestroy()
        {
            // Unsubscribes
            EventManager.Instance.Unsubscribe(EEvent.OnUIManagerStartGameClicked, StartGameClicked);
            EventManager.Instance.Unsubscribe(EEvent.OnLevelManagerDropExplosion, DropExploded);
            EventManager.Instance.Unsubscribe(EEvent.OnLevelManagerGoldenDropExplosion, DropExploded);

            EventManager.Instance.Publish(EEvent.OnGameManagerDestroyed);
        }

        private void Start()
        {
            ChangeState(EState.MainMenu);
        }

        private void Update()
        {
            // State tick
            switch (_currentState)
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

        private void DropExploded(object[] args)
        {
            Points += 100;
        }

        #endregion

        private void ChangeState(EState newState)
        {
            EState oldState = _currentState;

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


            CurrentState = newState;
        }
    }
}