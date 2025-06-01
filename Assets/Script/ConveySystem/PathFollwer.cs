using UnityEngine;

public class PathFollower : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 2f;
    public int openBoxAtIndex = 2;

    private int currentIndex = 0;
    private Gripper gripper;

    private Vector3 randomOpenPoint; // 상자 오픈 포인트
    public void ResetFollower()
    {
        currentIndex = 0;
    }

    void Start()
    {
        gripper = GetComponent<Gripper>();

        // 상자 오픈 포인트 계산, 초기 설정된 오픈 포인트에서부터 다음 waypoint 사이 랜덤한 지점
        Vector3 from = waypoints[openBoxAtIndex].position;
        Vector3 to = waypoints[openBoxAtIndex + 1].position;
        float t = Random.Range(0f, 1f); 
        randomOpenPoint = Vector3.Lerp(from, to, t);
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

        // 사전 설정된 오픈 포인트 도달시
        if (Vector3.Distance(transform.position, randomOpenPoint) < 0.1f)
        {
            if (gripper != null)
            {
                gripper.OpenBoxIfHolding();
            }
        }

        // 일반 waypoint 도달 처리
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentIndex++;
        }
    }
}