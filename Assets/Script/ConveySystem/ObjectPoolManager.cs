using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    [Header("Optional")]
    [SerializeField] private Transform poolRootParent;  // Hierarchy 정리용 부모

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;        // 생성할 오브젝트 수
    }

    public List<Pool> pools;    // 에디터에서 설정한 풀 목록, 박스, Gripper, 이펙트
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;

        // poolRootParent가 지정되지 않았으면 자동 생성
        if (poolRootParent == null)
        {
            GameObject tempParent = new GameObject("tempGameObjects");
            poolRootParent = tempParent.transform;
        }

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools)     // 각 풀에 초기 오브젝트 생성
        {
            if (pool.prefab == null)
            {
                Debug.LogError($"[POOL INIT ERROR] Prefab for tag {pool.tag} is NULL!");
                continue;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, poolRootParent);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
            Debug.Log($"Pool registered: {pool.tag} with {pool.size} objects.");
        }
    }

    /// <summary>
    /// 풀에서 비활성화된 오브젝트 하나를 꺼내 소환(오브젝트 반환)
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))       // 찾으려는 태그가 없으면 null
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        Queue<GameObject> pool = poolDictionary[tag];

        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)     // 아직 사용되지 않은 오브젝트를 찾음
            {
                obj.transform.SetParent(poolRootParent);  // 부모로 재지정 혹시나..
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);        // 오브젝트 활성화
                return obj;
            }
        }

        // 모든 오브젝트가 사용 중이라면 경고
        Debug.LogWarning($"[POOL] No available inactive object in pool '{tag}'. Consider increasing size.");
        return null;
    }

    // 모든 풀의 오브젝트를 비활성화시킴
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
