using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools)
        {
            if (pool.prefab == null)
            {
                Debug.LogError($"[POOL INIT ERROR] Prefab for tag {pool.tag} is NULL!");
                continue;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
            Debug.Log($"Pool registered: {pool.tag} with {pool.size} objects.");
        }
    }


    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        Queue<GameObject> pool = poolDictionary[tag];

        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                return obj;
            }
        }

        Debug.LogWarning($"[POOL] No available inactive object in pool '{tag}'. Consider increasing size.");
        return null;
    }


    public void ResetAllObjects()
    {
        foreach (var queue in poolDictionary.Values)
        {
            foreach (var obj in queue)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }
}
