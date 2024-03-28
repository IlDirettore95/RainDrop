using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GMDG.RainDrop.Entities;

namespace GMDG.RainDrop.System
{
    public class LevelManager : MonoBehaviour
    {
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
        public List<Drop> Drops { get {  return _drops; } }

        private IEnumerator _spawnCoroutine;
        private WaitForSeconds _spawnCooldown;

        private float ScreenHeight => Camera.main.orthographicSize * 2.0f;
        private float ScreenWidth => ScreenHeight * Camera.main.aspect;

        private float DropHalfWidth => _dropPrefabSpriteRenderer.bounds.extents.x;
        private float DropHalfHeight => _dropPrefabSpriteRenderer.bounds.extents.y;

        #region UnityMessages

        private void Awake()
        {
            // Init variables
            Debug.Assert(DropPrefab != null, "Drop prefab is not defined!");
            Debug.Assert(DropPrefab.GetComponent<Drop>() != null, "Drop prefab does not have a Drop component!");
            Debug.Assert(DropPrefab.GetComponent<SpriteRenderer>() != null, "Drop prefab does not have a SpriteRenderer component!");

            _dropPrefabSpriteRenderer = DropPrefab.GetComponent<SpriteRenderer>();

            _dropsPool = new StaticPool(DropPrefab, 16);
            _goldenDropsPool = new StaticPool(DropPrefab, 4);

            _drops = new List<Drop>();

            _spawnCoroutine = SpawnCoroutine();
            _spawnCooldown = new WaitForSeconds(2);

            // Subscribe listeners
            EventManager.Instance.Subscribe(EEvent.OnGameManagerChangedState, GameManagerStateChanged);

            EventManager.Instance.Publish(EEvent.OnGameManagerLoaded);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe listeners

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
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(ScreenWidth, ScreenHeight, 0));
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

        #endregion

        private IEnumerator SpawnCoroutine()
        {
            while(true)
            {
                yield return _spawnCooldown;
                if (_dropsPool.HasItems())
                {
                    float randomX = Random.Range(-ScreenWidth / 2 + DropHalfWidth, ScreenWidth / 2 - DropHalfWidth);
                    SpawnDrop(new Vector3(randomX, ScreenHeight / 2 + DropHalfHeight, 0));
                }
            }
        }

        private void SpawnDrop(Vector2 position)
        {
            GameObject drop = _dropsPool.Get(true, position);
            Drop dropComponent = drop.GetComponent<Drop>();
            dropComponent.Operation = new Sum(2, 3, 5);
            _drops.Add(dropComponent);

            EventManager.Instance.Publish(EEvent.OnDropSpawned);
        }

        private void DespawnDrop(int index)
        {
            Debug.Assert(index >= 0 && index < _drops.Count, "A not subscribed drop can't be despawn!");

            _dropsPool.Release(_drops[index].gameObject);
            _drops.RemoveAt(index);

            EventManager.Instance.Publish(EEvent.OnDropDespawned);
        }
    }
}
