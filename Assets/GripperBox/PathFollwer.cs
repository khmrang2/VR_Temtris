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
        if (currentIndex >= waypoints.Length)   // 경로 끝에 도달했을 때 Gripper 비활성화
        {
            if (gripper != null)
            {
                gripper.ResetGripper();
                gameObject.SetActive(false);
            }

            return;
        }

        Transform target = waypoints[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);      // 경로로 이동

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            if (currentIndex == openBoxAtIndex && gripper != null)      // 설정된 박스 오픈 위치에 도달 시 박스 오픈
            {
                gripper.OpenBoxIfHolding();
            }

            currentIndex++;     // 다음 경로로
        }
    }
}