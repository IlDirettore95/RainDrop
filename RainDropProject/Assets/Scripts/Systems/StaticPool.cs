using System;
using UnityEngine;

namespace GMDG.RainDrop.System
{
    /*
     * This class represent a pool of objects with a fixed capacity
     * If an item is asked but none is available, it will throw an error
     */
    public class StaticPool
    {
        public int Capacity { get; private set; }
        
        private GameObject[] pool;
        private int nextFreeItemIndex;

        public StaticPool(GameObject prefab, int capacity)
        {
            Debug.Assert(capacity > 0, "Pool capacity must be greater than 0!");

            pool = new GameObject[capacity];
            for (int i = 0; i < capacity; i++) 
            {
                pool[i] = GameObject.Instantiate(prefab);
                pool[i].SetActive(false);
            }

            Capacity = capacity;
            nextFreeItemIndex = 0;
        }
        public bool HasItems()
        {
            return nextFreeItemIndex < Capacity;
        }

        public GameObject Get(bool setActive, Vector3 position)
        {
            Debug.Assert(nextFreeItemIndex < Capacity, "Can't get an item if the pool is empty!");

            GameObject item = pool[nextFreeItemIndex];
            nextFreeItemIndex++;

            item.SetActive(setActive);
            item.transform.position = position;
            return item;
        }

        public void Release(GameObject item)
        {
            Debug.Assert(item != null, "Can't return a null item!");

            int releasedItemIndex = -1;
            for (int i = 0; i < nextFreeItemIndex; i++)
            {
                if (pool[i] == item) 
                { 
                    releasedItemIndex = i;
                    break;
                }
            }

            Debug.Assert(releasedItemIndex != -1, "Can't release an item that does not belong to the pool!");

            pool[releasedItemIndex].SetActive(false);

            // Swap item in [releasedItemIndex] with [nextFreeItemIndex - 1]
            GameObject tempItem = pool[releasedItemIndex];
            pool[releasedItemIndex] = pool[nextFreeItemIndex - 1];
            pool[nextFreeItemIndex - 1] = tempItem;
            
            nextFreeItemIndex--;
        }

        public override string ToString()
        {
            string text = string.Empty;

            text += "Pool business: " + nextFreeItemIndex + "/" + Capacity + Environment.NewLine;

            return text;
        }
    }
}
