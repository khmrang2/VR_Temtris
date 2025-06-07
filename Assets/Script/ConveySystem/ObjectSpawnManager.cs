using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectSpawnManager : MonoBehaviour
{
    public Transform spawnPoint;      // 출발 지점(Gripper의 첫번째 Waypoint와 동일)
    public int spawnCount = 4444;       // 총 몇 개 생성할지, 나중에 파라미터식으로 수정 필요
    public float spawnInterval = 1.5f; // 몇 초 간격으로 생성할지
    public int blockVariantCount = 7; // itemPrefabs.Length 와 동일하게 7로 설정

    // 7-Bag 용 리스트와 인덱스
    private List<int> itemBag = new List<int>();
    private int bagIndex = 0;

    // Random.
    private System.Random rng;
    public int randomSeed = 410;

    private void Awake()
    {
        // System.Random 생성기 초기화 (공통 시드 사용)
        rng = new System.Random(randomSeed);

        // 7-Bag 리스트 생성
        CreateNewBag();
    }

    void Start()
    {
        StartCoroutine(SpawnGrippersWithInterval());
    }

    IEnumerator SpawnGrippersWithInterval()
    {
        ObjectPoolManager.Instance.ResetAllObjects();

        for (int i = 0; i < spawnCount; i++)
        {
            // Gripper와 Box 생성
            GameObject gripper = ObjectPoolManager.Instance.SpawnFromPool("Gripper", spawnPoint.position, Quaternion.identity);
            if (gripper == null)
            {
                Debug.LogWarning("Gripper 생성 실패. 박스도 생성하지 않음.");
                continue;
            }
            GameObject box = ObjectPoolManager.Instance.SpawnFromPool("Box", Vector3.zero, Quaternion.identity);
            if (box == null)
            {
                Debug.LogWarning("Box 생성 실패.");
                continue;
            }

            if (bagIndex >= itemBag.Count)
            {
                // Bag이 비었으면 다시 새로 생성
                CreateNewBag();
            }

            gripper.GetComponent<Gripper>()?.ResetGripper();
            int chosenIndex = itemBag[bagIndex++];
            box.GetComponent<BoxOpen>()?.ResetBox(chosenIndex);

            gripper.GetComponent<PathFollower>()?.ResetFollower();
            gripper.GetComponent<Gripper>()?.Grab(box);

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// 7-Bag 리스트를 생성하고 섞기
    /// </summary>
    private void CreateNewBag()
    {
        itemBag.Clear();
        for (int i = 0; i < blockVariantCount; i++)
        {
            itemBag.Add(i);
        }

        // Fisher–Yates Shuffle
        for (int i = itemBag.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1); // 0 <= j <= i
            int tmp = itemBag[i];
            itemBag[i] = itemBag[j];
            itemBag[j] = tmp;
        }

        bagIndex = 0;
    }
}
