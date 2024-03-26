using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMDG.RainDrop.System
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private GameObject DropPrefab;

        private StaticPool _dropsPool;
        public StaticPool DropsPool { get { return _dropsPool; } }

        private List<Drop> _drops;
        public List<Drop> Drops { get {  return _drops; } }

        private IEnumerator _spawnCoroutine;
        private WaitForSeconds _spawnCooldown;

        #region UnityMessages

        private void Awake()
        {
            Debug.Assert(DropPrefab != null, "Drop prefab is not defined!");
            Debug.Assert(DropPrefab.GetComponent<Drop>() != null, "Drop prefab does not have a Drop component!");
            Debug.Assert(DropPrefab.GetComponent<SpriteRenderer>() != null, "Drop prefab does not have a SpriteRenderer component!");

            _dropsPool = new StaticPool(DropPrefab, 16);
            _drops = new List<Drop>();
            _spawnCoroutine = SpawnCoroutine();
            _spawnCooldown = new WaitForSeconds(2);
            StartCoroutine(_spawnCoroutine);

            EventManager.Instance.Publish(Event.OnGameManagerLoaded);
        }
        
        private void OnDestroy()
        {
            EventManager.Instance.Publish(Event.OnGameManagerDestroyed);
        }

        private void Update()
        {
            LogManager.LogLevelManager(this);

            for (int i = 0; i < _drops.Count; i++) 
            {
                _drops[i].transform.position += Vector3.down * 2.0f * Time.deltaTime;

                if (_drops[i].transform.position.y < -Camera.main.orthographicSize * Camera.main.aspect)
                {
                    DespawnDrop(i);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            float verticalHeightSeen = Camera.main.orthographicSize * 2.0f;
            float verticalWidthSeen = verticalHeightSeen * Camera.main.aspect;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(verticalWidthSeen, verticalHeightSeen, 0));
        }
#endif

        #endregion

        private IEnumerator SpawnCoroutine()
        {
            while(true)
            {
                yield return _spawnCooldown;
                if (_dropsPool.HasItems())
                {
                    SpawnDrop(new Vector3(0, Camera.main.orthographicSize, 0));
                }
            }
        }

        private void SpawnDrop(Vector2 position)
        {
            GameObject drop = _dropsPool.Get(true, position);
            _drops.Add(drop.GetComponent<Drop>());
        }

        private void DespawnDrop(int index)
        {
            Debug.Assert(index >= 0 && index < _drops.Count, "A not subscribed drop can't be despawn!");

            _dropsPool.Release(_drops[index].gameObject);
            _drops.RemoveAt(index);
        }
    }
}
