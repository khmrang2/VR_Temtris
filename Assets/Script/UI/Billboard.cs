using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Transform cam;
    void Awake() => cam = Camera.main.transform;
    void LateUpdate()
    {
        Vector3 fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
        transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
    }
}