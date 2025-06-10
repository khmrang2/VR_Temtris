using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCam : MonoBehaviour
{
    private Camera cam;         // ChatCam
    public Renderer target;    // 노란 박스를 포함하는 Renderer (Box에 붙여두세요)
    public float padding = 1f; // 여백

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }
    void Start()
    {
        var bounds = target.bounds;
        cam.transform.position = bounds.center - cam.transform.forward * 10f;
        // 위 10f는 초기 카메라 거리, 대략 맞춰두고 아래 FOV 계산에만 씁니다

        float dist = Vector3.Distance(cam.transform.position, bounds.center);
        float h = bounds.size.y * 0.5f + padding;
        float w = bounds.size.x * 0.5f + padding;

        float fovV = 2f * Mathf.Atan(h / dist) * Mathf.Rad2Deg;
        float fovH = 2f * Mathf.Atan(w / dist) * Mathf.Rad2Deg / cam.aspect;

        cam.fieldOfView = Mathf.Max(fovV, fovH);
    }
}
