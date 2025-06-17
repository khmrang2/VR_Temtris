using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 자식 오브젝트들의 위치 또는 Mesh의 중심을 기준으로
/// Rigidbody의 center of mass를 자동으로 계산하고 설정합니다.
/// 복합 오브젝트, 단일 Mesh 조각 모두 대응 가능합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CenterofMass : MonoBehaviour
{
    void Start()
    {
        SetCenterOfMass();
    }

    public void SetCenterOfMass()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.automaticCenterOfMass = false;

        Transform[] children = GetComponentsInChildren<Transform>();
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (Transform child in children)
        {
            if (child == transform) continue;
            sum += transform.InverseTransformPoint(child.position);
            count++;
        }

        if (count > 0)
        {
            Vector3 avg = sum / count;
            rb.centerOfMass = avg;
#if UNITY_EDITOR
            Debug.Log($"[COM] {gameObject.name} (multi-child): {avg}");
#endif
            return;
        }

        // 자식이 없거나 단일 Mesh 조각일 경우 → Mesh.bounds.center 사용
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Vector3 meshCenter = meshFilter.sharedMesh.bounds.center;
            rb.centerOfMass = meshCenter;
#if UNITY_EDITOR
            Debug.Log($"[COM] {gameObject.name} (single mesh): {meshCenter}");
#endif
        }
    }
}
