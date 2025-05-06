using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ڽ� ������Ʈ���� ��ġ �Ǵ� Mesh�� �߽��� ��������
/// Rigidbody�� center of mass�� �ڵ����� ����ϰ� �����մϴ�.
/// ���� ������Ʈ, ���� Mesh ���� ��� ���� �����մϴ�.
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

        // �ڽ��� ���ų� ���� Mesh ������ ��� �� Mesh.bounds.center ���
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
