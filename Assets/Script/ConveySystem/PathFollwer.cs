using UnityEngine;
using System.Collections.Generic;

public class PathFollower : MonoBehaviour
{
    private Transform[] waypoints;
    public float speed = 2f;

    private int currentIndex = 0;
    private Gripper gripper;
    private Vector3 _randomOpenPoint;

    public void SetSpeed(float spe)
    {
        this.hasDropped = false;
        this.speed = spe;
    } 

    public void SetRandomOpenPoint(List<Transform> points, Transform f, Transform t)
    {
        waypoints = points.ToArray();

        //  랜덤한 부분에서 박스가 랜덤으로 떨어짐.
        // -> 이거는 게임 보드에서 가져오는 방식으로 변경. 
        // inspector에서 할당된 GameBoard.cs->의 지점에서 가져오는 것으로 변경. 
        Vector3 from = f.position;
        Vector3 to = t.position;
        float random = Random.Range(0f, 1f);
        _randomOpenPoint = Vector3.Lerp(from, to, random);
    }

    public void ResetFollower()
    {
        currentIndex = 0;
    }

    private void Start()
    {
        gripper = GetComponent<Gripper>();
    }

    private bool hasDropped = false;

    private void Update()
    {
        if (waypoints == null || currentIndex >= waypoints.Length)
        {
            gripper?.ResetGripper();
            gameObject.SetActive(false);
            return;
        }

        Transform target = waypoints[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // y값이 달라져서 이상하게 되던것을 수정. xz평면상으로만 결정함.
        // 어떤 컨베이어 벨트를 쓰든 따라가지만, 판정은 xz평면으로 판정.
        Vector2 currentXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetXZ = new Vector2(_randomOpenPoint.x, _randomOpenPoint.z);

        if (!hasDropped && Vector2.Distance(currentXZ, targetXZ) < 0.1f)
        {
            gripper?.OpenBoxIfHolding();
            hasDropped = true;
        }

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentIndex++;
        }
    }

}
