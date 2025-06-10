using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class BoardScreen : MonoBehaviour
{
    public Camera chatCam;        // 렌더링하는 카메라

    void Start()
    {
        var rt = chatCam.targetTexture;
        float aspect = (float)rt.width / rt.height;


        // 기본 Plane은 10×10 유닛이므로, 1→10으로 치환해서 곱해 줍니다.
        var s = transform.localScale;
        transform.localScale = new Vector3(s.x * aspect, s.y, s.z);
    }
}