using System.Collections.Generic;
using UnityEngine;
using GMDG.RainDrop.Entities;
using GMDG.RainDrop.Scriptable;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GMDG.RainDrop.System
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance;

        public List<DifficultyAsset> Difficulties = new List<DifficultyAsset>();
        [SerializeField] private GameObject DropPrefab;
        [SerializeField] private GameObject GoldenDropPrefab;

        private SpriteRenderer _dropPrefabSpriteRenderer;

        // Pool for "creation" of drops
        private StaticPool _dropsPool;
        public StaticPool DropsPool { get { return _dropsPool; } }

        // Pool for "creation" of goldenDrops
        private StaticPool _goldenDropsPool;
        public StaticPool GoldenDropsPool { get { return _goldenDropsPool; } }

        // List for iterate on drops to check for "collisions" and movement
        private List<Drop> _drops;
        public List<Drop> Drops { get { return _drops; } }

        // Dictionary to query operations result
        // Maps result (int) into list's indexes
        private Dictionary<int, List<Drop>> _dropsPerResult;

        // Difficulty settings
        private int _currentDifficultyIndex = 0;
        public int CurrentDifficultyIndex
        {
            get
            {
                return _currentDifficultyIndex;
            }
            private set
            {
                _currentDifficultyIndex = value;
                if (_currentDifficultyIndex < Difficulties.Count)
                {
                    _dropsSpeed = Difficulties[_currentDifficultyIndex].DropsSpeed;
                    EventManager.Instance.Publish(EEvent.OnLevelManagerDifficultyChanged, Difficulties[_currentDifficultyIndex]);
                }
            }
        }

        public DifficultyAsset CurrentDifficulty => Difficulties[CurrentDifficultyIndex];

        private float _dropsSpeed;

        private float ScreenHeight => Camera.main? Camera.main.orthographicSize * 2.0f : 0;
        private float ScreenWidth => ScreenHeight * (Camera.main? Camera.main.aspect : 0);

        private float DropHalfWidth => _dropPrefabSpriteRenderer.bounds.extents.x;
        private float DropHalfHeight => _dropPrefabSpriteRenderer.bounds.extents.y;

        private List<int> _indexesOfLanesAvailable;
        private Timer[] _timersPerLane;

        private Timer _spawnTimer;

        private int NumberOfLanes => (int)(ScreenWidth / (DropHalfWidth * 2));
        private float LaneWidth => ScreenWidth / NumberOfLanes;
        private float LanePositionX(int i) => -ScreenWidth / 2 + i * LaneWidth + LaneWidth / 2;


        #region UnityMessages

        private void Awake()
        {
            // Init variables
            Debug.Assert(Difficulties != null && Difficulties.Count > 0, "No difficulties defined!");
            for (int i = 0; i < Difficulties.Count; i++) 
            {
                Debug.Assert(Difficulties[i].OperationsData != null && Difficulties[i].OperationsData.Count > 0, "A difficulty contains no operations!");
            }
            Debug.Assert(DropPrefab != null, "Drop prefab is not defined!");
            Debug.Assert(DropPrefab.GetComponent<Drop>() != null, "Drop prefab does not have a Drop component!");
            Debug.Assert(DropPrefab.GetComponent<SpriteRenderer>() != null, "Drop prefab does not have a SpriteRenderer component!");

            // Singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }

            _dropPrefabSpriteRenderer = DropPrefab.GetComponent<SpriteRenderer>();

            _dropsPool = new StaticPool(DropPrefab, 16);
            _goldenDropsPool = new StaticPool(GoldenDropPrefab, 4);

            _drops = new List<Drop>();

            _dropsPerResult = new Dictionary<int, List<Drop>>();

            // Init Difficulty
            CurrentDifficultyIndex = 0;

            _indexesOfLanesAvailable = new List<int>();
            for (int i = 0; i < NumberOfLanes; i++) 
            { 
                _indexesOfLanesAvailable.Add(i);
            }
            _timersPerLane = new Timer[NumberOfLanes];
            for (int i = 0;i < NumberOfLanes; i++) 
            {
                _timersPerLane[i] = new Timer();
            }
            _spawnTimer = new Timer();

            // Subscribe listeners
            EventManager.Instance.Subscribe(EEvent.OnGameManagerChangedState, GameManagerStateChanged);
            EventManager.Instance.Subscribe(EEvent.OnUIManagerResultWasSubmitted, ResultSubmitted);
            EventManager.Instance.Subscribe(EEvent.OnGameManagerTargetPointsReached, TargetScoreReached);

            EventManager.Instance.Publish(EEvent.OnGameManagerLoaded);
        }
        
        private void OnDestroy()
        {
            for (int i = 0; i < NumberOfLanes; i++)
            {
                _timersPerLane[i].Stop();
            }

            // Unsubscribe listeners
            EventManager.Instance.Unsubscribe(EEvent.OnGameManagerChangedState, GameManagerStateChanged);
            EventManager.Instance.Unsubscribe(EEvent.OnUIManagerResultWasSubmitted, ResultSubmitted);
            EventManager.Instance.Unsubscribe(EEvent.OnGameManagerTargetPointsReached, TargetScoreReached);

            EventManager.Instance.Publish(EEvent.OnGameManagerDestroyed);
        }

        private void Update()
        {
#if DEBUG_MODE && DEBUG_LEVEL_MANAGER
            LogManager.LogLevelManager(this);
#endif

            if (GameManager.Instance.CurrentState == GameManager.EState.Pause) return;

            // Move Drops
            for (int i = 0; i < _drops.Count; i++) 
            {
                _drops[i].transform.position += Vector3.down * _dropsSpeed * Time.deltaTime;

                if (_drops[i].transform.position.y < -ScreenHeight / 2 - DropHalfHeight)
                {
                    DespawnDrop(i);
                    EventManager.Instance.Publish(EEvent.OnLevelManagerDropDespawned);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            // Draw Screen
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(ScreenWidth, ScreenHeight, 0));

            // Draw Lanes
            Gizmos.color = Color.green;
            for (int i = 0; i < NumberOfLanes; i++)
            {
                Gizmos.DrawLine(new Vector3(-ScreenWidth/2 + i * LaneWidth, ScreenHeight / 2, 0), new Vector3(-ScreenWidth / 2 + i * LaneWidth, -ScreenHeight / 2, 0));
            }

            // Draw Lanes cooldown
            GUIStyle style = new GUIStyle();

            for (int i = 0; i < NumberOfLanes; i++)
            {
                if (_indexesOfLanesAvailable.Contains(i))
                {
                    style.normal.textColor = Color.green;
                }
                else
                {
                    style.normal.textColor = Color.red;
                }

                Handles.Label(new Vector3(LanePositionX(i), ScreenHeight / 2 - DropHalfHeight, 0), _timersPerLane[i].TimeLeft.ToString(), style);
            }
        }
#endif

#endregion

        #region Event_Listeners

        private void GameManagerStateChanged(object[] args)
        {
            GameManager.EState oldState = (GameManager.EState)args[0];
            GameManager.EState newState = (GameManager.EState)args[1];

            switch (oldState)
            {
                case GameManager.EState.MainMenu:
                    switch (newState)
                    {
                        case GameManager.EState.Gameplay:
                            // From MainMenu to Gameplay
                            // Start the spawntimer based on current difficulty and spawning a random drop after it
                            _spawnTimer.Start(CurrentDifficulty.SpawnCooldown, SpawnRandomDrop);
                            break;
                    }
                    break;

                case GameManager.EState.Gameplay:
                    switch (newState)
                    {
                        case GameManager.EState.Pause:
                            // From Gameplay to Pause
                            _spawnTimer.Pause();
                            break;
                        case GameManager.EState.GameOver:
                            // From Gameplay to GameOver
                            _spawnTimer.Stop();
                            DespawnAllDrop();
                            break;
                    }
                    break;

                case GameManager.EState.Pause:
                    switch (newState) 
                    {
                        case GameManager.EState.MainMenu:
                            // From Pause to MainMenu
                            _spawnTimer.Stop();
                            DespawnAllDrop();
                            break;
                        case GameManager.EState.Gameplay:
                            // From Pause to Gameplay
                            _spawnTimer.Unpause();
                            break;
                    }
                    break;
            }
        }

        private void ResultSubmitted(object[] args)
        {
            int result = (int)args[0];

            List<Drop> dropsList = new List<Drop>();
            if (!_dropsPerResult.TryGetValue(result, out dropsList))
            {
                return;
            }

            // First Drop added
            Drop drop = dropsList[0];
            if (drop.IsGolden)
            {
                ExplodeAllDrop();
                EventManager.Instance.Publish(EEvent.OnLevelManagerGoldenDropExplosion);

            }
            else
            {
                DespawnDrop(_drops.IndexOf(drop));
                EventManager.Instance.Publish(EEvent.OnLevelManagerDropExplosion);
            }
        }

        private void TargetScoreReached(object[] args)
        {
            // Change Difficulty
            if (CurrentDifficultyIndex >= Difficulties.Count - 1)
            {
                EventManager.Instance.Publish(EEvent.OnLevelManagerLastDifficultyFinished);
                return;
            }
            CurrentDifficultyIndex++;

   
        }

        #endregion

        private void SpawnRandomDrop()
        {
            // Is Golden?
            bool isGolden = Random.Range(1, 100) < CurrentDifficulty.GoldenDropSpawnPercentage ? true : false;

            // If there are enough object in the pools change drop type or terminate the procedure
            if (isGolden && !_goldenDropsPool.HasItems()) 
            { 
                if (_dropsPool.HasItems())
                {
                    isGolden = false;
                }
                else
                {
                    return;
                }
            }
            else if (!isGolden && !_dropsPool.HasItems())
            {
                if (_goldenDropsPool.HasItems())
                {
                    isGolden = true;
                }
                else
                {
                    return;
                }
            }

            // Position
            int randomLaneIndex = _indexesOfLanesAvailable[Random.Range(0, _indexesOfLanesAvailable.Count)];
            float randomX = LanePositionX(randomLaneIndex);

            // Operation Type 
            OperationData randomOperationData = CurrentDifficulty.OperationsData[Random.Range(0, CurrentDifficulty.OperationsData.Count)];
            Operation randomOperation = null;
            int firstOperand = randomOperationData.FirstOperand;
            int secondOperand = randomOperationData.SecondOperand;
            switch (randomOperationData.OperationType)
            {
                case EOperationType.Sum:
                    randomOperation = new Sum(firstOperand, secondOperand);
                    break;

                case EOperationType.Sub:
                    randomOperation = new Sub(firstOperand, secondOperand);
                    break;

                case EOperationType.Mul:
                    randomOperation = new Mul(firstOperand, secondOperand);
                    break;

                case EOperationType.Div:
                    randomOperation = new Div(firstOperand, secondOperand);
                    break;

                case EOperationType.And:
                    randomOperation = new And(firstOperand, secondOperand);
                    break;

                case EOperationType.Or:
                    randomOperation = new Or(firstOperand, secondOperand);
                    break;
            }

            SpawnDrop(new Vector3(randomX, ScreenHeight / 2 + DropHalfHeight, 0), randomOperation, isGolden);

            // Start Lane timer
            _timersPerLane[randomLaneIndex].Start(DropHalfHeight * 2 / _dropsSpeed, () =>
            {
                // callback
                _indexesOfLanesAvailable.Add(randomLaneIndex);
            });
            _indexesOfLanesAvailable.Remove(randomLaneIndex);

            // Restart spawn
            _spawnTimer.Start(CurrentDifficulty.SpawnCooldown, SpawnRandomDrop);
        }

        private void SpawnDrop(Vector2 position, Operation operation, bool isGolden)
        {
            Debug.Assert(operation != null, "Can't spawn a drop with a null operation!");

            GameObject drop = isGolden? _goldenDropsPool.Get(true, position) : _dropsPool.Get(true, position);
            Drop dropComponent = drop.GetComponent<Drop>();
            dropComponent.Operation = operation;

            _drops.Add(dropComponent);

            if (!_dropsPerResult.ContainsKey(dropComponent.Operation.Result))
            {
                _dropsPerResult[dropComponent.Operation.Result] = new List<Drop>();
            }
            _dropsPerResult[dropComponent.Operation.Result].Add(dropComponent);

            EventManager.Instance.Publish(EEvent.OnLevelManagerDropSpawned);
        }

        private void DespawnDrop(int index)
        {
            Debug.Assert(index >= 0 && index < _drops.Count, "A not subscribed drop can't be despawn!");
            
            Drop dropToRemove = _drops[index];
            int result = dropToRemove.Operation.Result;

            if (dropToRemove.IsGolden)
            {
                _goldenDropsPool.Release(dropToRemove.gameObject);
            }
            else
            {
                _dropsPool.Release(dropToRemove.gameObject);
            }

            _dropsPerResult[result].Remove(dropToRemove);
            _drops.RemoveAt(index);

            if (_dropsPerResult[result].Count == 0)
            {
                _dropsPerResult.Remove(result);
            }
        }

        private void ExplodeAllDrop()
        {
            for (int i = _drops.Count - 1; i >= 0 && _drops.Count > 0; i--)
            {
                DespawnDrop(i);
                EventManager.Instance.Publish(EEvent.OnLevelManagerDropExplosion);
            }
        }

        private void DespawnAllDrop()
        {
            for (int i = _drops.Count - 1; i >= 0; i--)
            {
                DespawnDrop(i);
            }
        }
    }
}