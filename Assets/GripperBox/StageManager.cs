using UnityEngine;
using System.Collections;

public class StageManager : MonoBehaviour
{
    public Transform spawnPoint;      // �ϳ��� ��� ����
    public int spawnCount = 10;       // �� �� �� ��������, ���߿� �Ķ���ͽ����� ���� �ʿ�
    public float spawnInterval = 1.5f; // �� �� �������� ��������

    void Start()
    {
        StartCoroutine(SpawnGrippersWithInterval());
    }

    IEnumerator SpawnGrippersWithInterval()
    {
        ObjectPoolManager.Instance.ResetAllObjects();

        for (int i = 0; i < spawnCount; i++)
        {
            // Gripper�� Box ����
            GameObject gripper = ObjectPoolManager.Instance.SpawnFromPool("Gripper", spawnPoint.position, Quaternion.identity);
            if (gripper == null)
            {
                Debug.LogWarning("Gripper ���� ����. �ڽ��� �������� ����.");
                continue;
            }
            GameObject box = ObjectPoolManager.Instance.SpawnFromPool("Box", Vector3.zero, Quaternion.identity);
            if (box == null)
            {
                Debug.LogWarning("Box ���� ����.");
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
