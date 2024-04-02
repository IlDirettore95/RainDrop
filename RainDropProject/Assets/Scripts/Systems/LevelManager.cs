using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GMDG.RainDrop.Entities;
using GMDG.RainDrop.Scriptable;

namespace GMDG.RainDrop.System
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private List<DifficultyAsset> Difficulties = new List<DifficultyAsset>();
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

        private IEnumerator _spawnCoroutine;
        private WaitForSeconds _spawnCooldown;

        private float ScreenHeight => Camera.main.orthographicSize * 2.0f;
        private float ScreenWidth => ScreenHeight * Camera.main.aspect;

        private float DropHalfWidth => _dropPrefabSpriteRenderer.bounds.extents.x;
        private float DropHalfHeight => _dropPrefabSpriteRenderer.bounds.extents.y;

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

            _dropPrefabSpriteRenderer = DropPrefab.GetComponent<SpriteRenderer>();

            _dropsPool = new StaticPool(DropPrefab, 16);
            _goldenDropsPool = new StaticPool(GoldenDropPrefab, 4);

            _drops = new List<Drop>();

            _dropsPerResult = new Dictionary<int, List<Drop>>();

            _spawnCoroutine = SpawnCoroutine();
            _spawnCooldown = new WaitForSeconds(2);

            // Subscribe listeners
            EventManager.Instance.Subscribe(EEvent.OnGameManagerChangedState, GameManagerStateChanged);
            EventManager.Instance.Subscribe(EEvent.OnUIManagerResultWasSubmitted, ResultSubmitted);

            EventManager.Instance.Publish(EEvent.OnGameManagerLoaded);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe listeners
            EventManager.Instance.Unsubscribe(EEvent.OnGameManagerChangedState, GameManagerStateChanged);
            EventManager.Instance.Unsubscribe(EEvent.OnUIManagerResultWasSubmitted, ResultSubmitted);

            EventManager.Instance.Publish(EEvent.OnGameManagerDestroyed);
        }

        private void Update()
        {
            LogManager.LogLevelManager(this);

            for (int i = 0; i < _drops.Count; i++) 
            {
                _drops[i].transform.position += Vector3.down * 1.0f * Time.deltaTime;

                if (_drops[i].transform.position.y < -ScreenHeight / 2 - DropHalfHeight)
                {
                    DespawnDrop(i);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw Screen
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(ScreenWidth, ScreenHeight, 0));

            // Draw Lanes
            Gizmos.color = Color.green;
            for (int i = 0; i < NumberOfLanes; i++)
            {
                Gizmos.DrawLine(new Vector3(-ScreenWidth/2 + i * LaneWidth, ScreenHeight / 2, 0), new Vector3(-ScreenWidth / 2 + i * LaneWidth, -ScreenHeight / 2, 0));
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

            for (int i = dropsList.Count - 1; i >= 0; i--)
            {
                Drop drop = dropsList[i];
                if (drop.IsGolden)
                {
                    ExplodeAllDrop();
                    EventManager.Instance.Publish(EEvent.OnLevelManagerGoldenDropExplosion);
                    break;
                }
                else
                {
                    DespawnDrop(_drops.IndexOf(dropsList[i]));
                    EventManager.Instance.Publish(EEvent.OnLevelManagerDropExplosion);
                }
            }
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
                    int randomLaneIndex = Random.Range(0, NumberOfLanes);
                    float randomX = LanePositionX(randomLaneIndex);

                    // Operation Type 
                    DifficultyAsset difficulty = Difficulties[0];
                    OperationData randomOperationData = difficulty.OperationsData[Random.Range(0, difficulty.OperationsData.Count)];
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
                    }

                    // Is Golden?
                    bool isGolden = Random.Range(0, 99) < difficulty.GoldenDropSpawnPercentage - 1 ? true : false;

                    SpawnDrop(new Vector3(randomX, ScreenHeight / 2 + DropHalfHeight, 0), randomOperation, isGolden);
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

            EventManager.Instance.Publish(EEvent.OnLevelManagerDropDespawned);
        }

        private void ExplodeAllDrop()
        {
            for (int i = _drops.Count - 1; i >= 0; i--)
            {
                DespawnDrop(i);
                EventManager.Instance.Publish(EEvent.OnLevelManagerDropExplosion);
            }
        }
    }
}
