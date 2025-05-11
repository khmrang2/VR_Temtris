using UnityEngine;

public class PathFollower : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 2f;
    public int openBoxAtIndex = 2;

    private int currentIndex = 0;
    private Gripper gripper;

    public void ResetFollower()
    {
        currentIndex = 0;
    }

    void Start()
    {
        gripper = GetComponent<Gripper>();
    }

    void Update()
    {
        if (currentIndex >= waypoints.Length)   // ��� ���� �������� �� Gripper ��Ȱ��ȭ
        {
            if (gripper != null)
            {
                gripper.ResetGripper();
                gameObject.SetActive(false);
            }

            return;
        }

        Transform target = waypoints[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);      // ��η� �̵�

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            if (currentIndex == openBoxAtIndex && gripper != null)      // ������ �ڽ� ���� ��ġ�� ���� �� �ڽ� ����
            {
                gripper.OpenBoxIfHolding();
            }

            currentIndex++;     // ���� ��η�
        }
    }
}