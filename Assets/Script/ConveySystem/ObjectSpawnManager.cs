using UnityEngine;
using System.Collections;

public class ObjectSpawnManager : MonoBehaviour
{
    public Transform spawnPoint;      // 출발 지점(Gripper의 첫번째 Waypoint와 동일)
    public int spawnCount = 10;       // 총 몇 개 생성할지, 나중에 파라미터식으로 수정 필요
    public float spawnInterval = 1.5f; // 몇 초 간격으로 생성할지

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

            gripper.GetComponent<Gripper>()?.ResetGripper();
            box.GetComponent<BoxOpen>()?.ResetBox();

            gripper.GetComponent<PathFollower>()?.ResetFollower();
            gripper.GetComponent<Gripper>()?.Grab(box);

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
