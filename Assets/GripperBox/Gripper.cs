using UnityEngine;

public class Gripper : MonoBehaviour
{
    public Transform holdPoint;
    private GameObject heldBox;

    public void Grab(GameObject box)
    {
        heldBox = box;
        box.transform.SetParent(holdPoint);
        box.transform.localPosition = Vector3.zero;

        var rb = box.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        var interactable = box.GetComponent<BoxOpen>();
        if (interactable != null)
        {
            interactable.SetHeldByGripper(true, this); // 자기 자신 전달
        }
    }

    public void Release()
    {
        if (heldBox == null) return;

        heldBox.transform.SetParent(null);

        var rb = heldBox.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        var interactable = heldBox.GetComponent<BoxOpen>();
        if (interactable != null)
        {
            interactable.SetHeldByGripper(false, null); // 참조 제거
        }

        heldBox = null;
    }

    public void OpenBoxIfHolding()
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

    public void ResetGripper()
    {
        if (heldBox != null)
        {
            Release();
        }

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
}

//using UnityEngine;

//public class Gripper : MonoBehaviour
//{
//    public Transform holdPoint;
//    private GameObject heldBox;

//    public void Grab(GameObject box)
//    {
//        heldBox = box;
//        box.transform.SetParent(holdPoint);
//        box.transform.localPosition = Vector3.zero;

//        var rb = box.GetComponent<Rigidbody>();
//        if (rb != null) rb.isKinematic = true;

//        var interactable = box.GetComponent<BoxOpen>();
//        if (interactable != null)
//        {
//            interactable.SetHeldByGripper(true);
//        }
//    }

//    public void Release()
//    {
//        if (heldBox == null) return;

//        heldBox.transform.SetParent(null);

//        var rb = heldBox.GetComponent<Rigidbody>();
//        if (rb != null) rb.isKinematic = false;

//        var interactable = heldBox.GetComponent<BoxOpen>();
//        if (interactable != null)
//        {
//            interactable.SetHeldByGripper(false);
//        }

//        heldBox = null;
//    }

//    public void OpenBoxIfHolding()
//    {
//        if (heldBox != null)
//        {
//            var boxScript = heldBox.GetComponent<BoxOpen>();
//            if (boxScript != null)
//            {
//                Release();
//                boxScript.TryOpen();
//            }
//        }
//    }

//    public void ResetGripper()
//    {
//        if (heldBox != null)
//        {
//            Release();
//        }

//        transform.position = Vector3.zero;
//        transform.rotation = Quaternion.identity;
//    }
//}