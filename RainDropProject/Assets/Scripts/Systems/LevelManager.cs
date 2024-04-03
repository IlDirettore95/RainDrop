using System.Collections;
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
                    _spawnCooldown = new WaitForSeconds(Difficulties[_currentDifficultyIndex].SpawnCooldown);
                    EventManager.Instance.Publish(EEvent.OnLevelManagerDifficultyChanged, Difficulties[_currentDifficultyIndex]);
                }
            }
        }

        public DifficultyAsset CurrentDifficulty => Difficulties[CurrentDifficultyIndex];

        private float _dropsSpeed;

        private IEnumerator _spawnCoroutine;
        private WaitForSeconds _spawnCooldown;

        private float ScreenHeight => Camera.main.orthographicSize * 2.0f;
        private float ScreenWidth => ScreenHeight * Camera.main.aspect;

        private float DropHalfWidth => _dropPrefabSpriteRenderer.bounds.extents.x;
        private float DropHalfHeight => _dropPrefabSpriteRenderer.bounds.extents.y;

        private List<int> _indexesOfLanesAvailable;
        private float[] _cooldownsPerLane;

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

            _spawnCoroutine = SpawnCoroutine();

            _indexesOfLanesAvailable = new List<int>();
            for (int i = 0; i < NumberOfLanes; i++) 
            { 
                _indexesOfLanesAvailable.Add(i);
            }
            _cooldownsPerLane = new float[NumberOfLanes];
            for (int i = 0;i < NumberOfLanes; i++) 
            {
                _cooldownsPerLane[i] = -1.0f;
            }

            // Subscribe listeners
            EventManager.Instance.Subscribe(EEvent.OnGameManagerChangedState, GameManagerStateChanged);
            EventManager.Instance.Subscribe(EEvent.OnUIManagerResultWasSubmitted, ResultSubmitted);
            EventManager.Instance.Subscribe(EEvent.OnGameManagerTargetPointsReached, TargetScoreReached);

            EventManager.Instance.Publish(EEvent.OnGameManagerLoaded);
        }
        
        private void OnDestroy()
        {
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

            // Update cooldowns
            for (int i = 0; i < _cooldownsPerLane.Length; i++)
            {
                if (_cooldownsPerLane[i] == -1)
                {
                    continue;
                }

                _cooldownsPerLane[i] -= Time.deltaTime;

                if (_cooldownsPerLane[i] <= 0)
                {
                    _cooldownsPerLane[i] = -1;
                    _indexesOfLanesAvailable.Add(i);
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

                Handles.Label(new Vector3(LanePositionX(i), ScreenHeight / 2 - DropHalfHeight, 0), _cooldownsPerLane[i].ToString(), style);
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
                case GameManager.EState.Gameplay:
                    StopCoroutine(_spawnCoroutine);
                    DespawnAllDrop();
                    break;
            }

            switch (newState)
            {
                case GameManager.EState.Gameplay:
                    StartCoroutine(_spawnCoroutine);
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

        private IEnumerator SpawnCoroutine()
        {
            while(true)
            {
                yield return _spawnCooldown;
                if (_dropsPool.HasItems())
                {
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

                    // Is Golden?
                    bool isGolden = Random.Range(1, 100) < CurrentDifficulty.GoldenDropSpawnPercentage ? true : false;

                    SpawnDrop(new Vector3(randomX, ScreenHeight / 2 + DropHalfHeight, 0), randomOperation, isGolden);

                    // Set Lane cooldown
                    _cooldownsPerLane[randomLaneIndex] = DropHalfHeight * 2 / _dropsSpeed;
                    _indexesOfLanesAvailable.Remove(randomLaneIndex);
                }
            }
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
