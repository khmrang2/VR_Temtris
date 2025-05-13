using UnityEngine;

public class Gripper : MonoBehaviour
{
    public Transform holdPoint;     // �ڽ��� ������ ��ġ
    private GameObject heldBox;     // ���� ����� �ִ� �ڽ�

    public void Grab(GameObject box)    // �ڽ� �����
    {
        Debug.Log($"[Gripper.cs] : ������ ���� : {this.gameObject.transform.position}");
        heldBox = box;
        box.transform.SetParent(holdPoint);     // �ڽ��� Gripper�� �ڽ����� ����
        box.transform.localPosition = Vector3.zero;

        var rb = box.GetComponent<Rigidbody>();     // �ڽ��� RigidBox ��Ȱ��ȭ
        if (rb != null) rb.isKinematic = true;

        var interactable = box.GetComponent<BoxOpen>();
        if (interactable != null)
        {
            interactable.SetHeldByGripper(true, this); // �ڱ� �ڽ� ����
        }
    }

    public void Release()   // ��� �ִ� �ڽ� ���ֱ�
    {
        if (heldBox == null) return;

        heldBox.transform.SetParent(null);      // �ڽİ��� ����

        var rb = heldBox.GetComponent<Rigidbody>();     // �ڽ��� RigidBox �ٽ� Ȱ��ȭ
        if (rb != null) rb.isKinematic = false;

        var interactable = heldBox.GetComponent<BoxOpen>();     // Gripper ���� ����
        if (interactable != null)
        {
            interactable.SetHeldByGripper(false, null); 
        }

        heldBox = null;     // ���� �ʱ�ȭ
    }

    public void OpenBoxIfHolding()      // Gripper�� ���� ��ġ���� �������� ��, Release �� �ڵ����� �ڽ��� Open
    {
        if (heldBox != null)
        {
            var boxScript = heldBox.GetComponent<BoxOpen>();
            if (boxScript != null)
            {
                Release();
                boxScript.TryOpen();
            }
        }
    }

    public void ResetGripper()      // Gripper�� Pool�� ���ư� �� �ʱ�ȭ
    {
        if (heldBox != null)
        {
            Release();
        }
        // -> �̰� ������ Waypoint1�� ��ġ�� �ƴ� (0,0,0) ���� ������ �Ǿ���. ���������ذ�.
        //transform.position = Vector3.zero;
        //transform.rotation = Quaternion.identity;
    }
}