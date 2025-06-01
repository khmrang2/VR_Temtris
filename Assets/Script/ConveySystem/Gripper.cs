using UnityEngine;

public class Gripper : MonoBehaviour
{
    public Transform holdPoint;     // 박스를 붙잡을 위치
    private GameObject heldBox;     // 현재 붙잡고 있는 박스

    public void Grab(GameObject box)    // 박스 붙잡기
    {
        Debug.Log($"[Gripper.cs] : 스폰된 지점 : {this.gameObject.transform.position}");
        heldBox = box;
        box.transform.SetParent(holdPoint);     // 박스를 Gripper의 자식으로 설정
        box.transform.localPosition = Vector3.zero;

        var rb = box.GetComponent<Rigidbody>();     // 박스의 RigidBox 비활성화
        if (rb != null) rb.isKinematic = true;

        var interactable = box.GetComponent<BoxOpen>();
        if (interactable != null)
        {
            interactable.SetHeldByGripper(true, this); // 자기 자신 전달
        }
    }

    public void Release()   // 잡고 있던 박스 놔주기
    {
        if (heldBox == null) return;

        heldBox.transform.SetParent(null);      // 자식관계 해제

        var rb = heldBox.GetComponent<Rigidbody>();     // 박스의 RigidBox 다시 활성화
        if (rb != null) rb.isKinematic = false;

        var interactable = heldBox.GetComponent<BoxOpen>();     // Gripper 정보 제거
        if (interactable != null)
        {
            interactable.SetHeldByGripper(false, null); 
        }

        heldBox = null;     // 참조 초기화
    }

    public void OpenBoxIfHolding()      // Gripper가 최종 위치까지 도달했을 때, Release 후 자동으로 박스를 Open
    {
        if (heldBox != null)
        {
            var boxScript = heldBox.GetComponent<BoxOpen>();
            if (boxScript != null)
            {
                Release();
                boxScript.ForcedOpen();
            }
        }
    }

    public void ResetGripper()      // Gripper가 Pool로 돌아갈 때 초기화
    {
        if (heldBox != null)
        {
            Release();
        }
        // -> 이거 때문에 Waypoint1의 위치가 아닌 (0,0,0) 에서 스폰이 되었음. 스폰문제해결.
        //transform.position = Vector3.zero;
        //transform.rotation = Quaternion.identity;
    }
}