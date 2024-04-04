using GMDG.RainDrop.Scriptable;
using System;
using UnityEngine;

namespace GMDG.RainDrop.System
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public enum EState
        {
            None,
            MainMenu,
            Gameplay,
            Pause,
            GameOver,
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
                int delta = value - _points;
                _points = value;
                EventManager.Instance.Publish(EEvent.OnGameManagerPointsChanged, _points, delta);
            }
        }

        private int _pointsLeftToChangeDifficulty;
        public int PointsLeftToChangeDifficulty
        {
            get 
            { 
                return _pointsLeftToChangeDifficulty; 
            }
            private set
            {
                int delta = value - _pointsLeftToChangeDifficulty;
                _pointsLeftToChangeDifficulty = value;

                if (_pointsLeftToChangeDifficulty <= 0)
                {
                    EventManager.Instance.Publish(EEvent.OnGameManagerTargetPointsReached);
                }
                else
                {
                    EventManager.Instance.Publish(EEvent.OnGameManagerPointsLeftToChangeDifficultyChanged, _pointsLeftToChangeDifficulty, delta);
                }

            }
        }

        private int _lives;
        public int Lives
        {
            get
            {
                return _lives;
            }
            private set
            {
                int delta = value - _lives; 
                _lives = value;
                EventManager.Instance.Publish(EEvent.OnGameManagerLivesChanged, _lives, delta);
            }
        }

        public int BestScore { get; private set; }

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

            _lives = 3;
            Points = 0;
            BestScore = DataManager.Instance.LoadInt("Best_Score", 0);

            // Subscribes
            EventManager.Instance.Subscribe(EEvent.OnUIManagerStartGameClicked, StartGameClicked);
            EventManager.Instance.Subscribe(EEvent.OnUIManagerQuitButtonClicked, QuitGameClicked);
            EventManager.Instance.Subscribe(EEvent.OnUIManagerGoBackToMenuClicked, GoBackToMenuClicked);
            EventManager.Instance.Subscribe(EEvent.OnUIManagerResumeClicked, ResumeClicked);
            EventManager.Instance.Subscribe(EEvent.OnLevelManagerDropExplosion, DropExploded);
            EventManager.Instance.Subscribe(EEvent.OnLevelManagerGoldenDropExplosion, DropExploded);
            EventManager.Instance.Subscribe(EEvent.OnLevelManagerDropDespawned, DropDespawned);
            EventManager.Instance.Subscribe(EEvent.OnLevelManagerDifficultyChanged, DifficultyChanged);

            EventManager.Instance.Publish(EEvent.OnGameManagerLoaded);
        }

        private void Start()
        {
            _pointsLeftToChangeDifficulty = LevelManager.Instance.CurrentDifficulty.ScoreToReach;
            
            CurrentState = EState.None;
            ChangeState(EState.MainMenu);
        }

        private void OnDestroy()
        {
            // Unsubscribes
            EventManager.Instance.Unsubscribe(EEvent.OnUIManagerStartGameClicked, StartGameClicked);
            EventManager.Instance.Unsubscribe(EEvent.OnUIManagerQuitButtonClicked, QuitGameClicked);
            EventManager.Instance.Unsubscribe(EEvent.OnUIManagerGoBackToMenuClicked, GoBackToMenuClicked);
            EventManager.Instance.Unsubscribe(EEvent.OnUIManagerResumeClicked, ResumeClicked);
            EventManager.Instance.Unsubscribe(EEvent.OnLevelManagerDropExplosion, DropExploded);
            EventManager.Instance.Unsubscribe(EEvent.OnLevelManagerGoldenDropExplosion, DropExploded);
            EventManager.Instance.Unsubscribe(EEvent.OnLevelManagerDropDespawned, DropDespawned);
            EventManager.Instance.Unsubscribe(EEvent.OnLevelManagerDifficultyChanged, DifficultyChanged);

            EventManager.Instance.Publish(EEvent.OnGameManagerDestroyed);
        }

        private void Update()
        {
#if DEBUG_MODE && DEBUG_GAME_MANAGER
            LogManager.LogGameManager(this);
#endif

            // State tick
            switch (_currentState)
            {
                case EState.MainMenu:
                    break;

                case EState.Gameplay:
                    GameplayUpdate();
                    break;

                case EState.Pause:
                    PauseUpdate();
                    break;

                case EState.GameOver:
                    break;
            }
        }

        #endregion

        #region Event_Listeners

        private void StartGameClicked(object[] args)
        {
            ChangeState(EState.Gameplay);
        }


        private void QuitGameClicked(object[] obj)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void GoBackToMenuClicked(object[] args)
        {
            ChangeState(EState.MainMenu);
        }

        private void ResumeClicked(object[] args)
        {
            ChangeState(EState.Gameplay);
        }

        private void DropExploded(object[] args)
        {
            Points += 100;
            PointsLeftToChangeDifficulty -= 100;
        }

        private void DropDespawned(object[] args)
        {
            Lives--;

            if (Lives <= 0)
            {
                ChangeState (EState.GameOver);
            }
        }

        private void DifficultyChanged(object[] args)
        {
            DifficultyAsset difficulty = (DifficultyAsset)args[0];

            PointsLeftToChangeDifficulty += difficulty.ScoreToReach;
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
                    EnterMainMenu();
                    break;

                case EState.Gameplay:
                    break;

                case EState.GameOver:
                    EnterGameOver();
                    break;
            }

            // Code for specific transitions
            // ...

            CurrentState = newState;
        }

        private void EnterMainMenu()
        {
            Points = 0;
            Lives = 3;
        }

        private void GameplayUpdate()
        {
            if (Input.GetKeyDown(KeyCode.P)) 
            { 
                ChangeState(EState.Pause);
            }
        }

        private void PauseUpdate()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                ChangeState(EState.Gameplay);
            }
        }

        private void EnterGameOver()
        {
            if (Points > BestScore)
            {
                BestScore = Points;
                DataManager.Instance.SaveInt("Best_Score", Points);
            }

            Points = 0;
            Lives = 3;
        }
    }
}