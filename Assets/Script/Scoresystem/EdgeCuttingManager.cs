using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EdgeCuttingManager : MonoBehaviour
{
    [Header("���� ���")]
    public EdgeCuttingHelper edgeCutter;

    [Header("���� �ý��� (����)")]
    public ScoreManager scoreManager;

    /// <summary>
    /// Ư�� LineTrigger���� ���� ��û�� ���� �� ȣ��
    /// </summary>
    public bool RequestCut(LineTrigger trigger)
    {
        Debug.Log("���� ��û �߻�!");

        if (trigger == null || edgeCutter == null) return false;

        List<GameObject> blocks = trigger.GetCurrentBlocks();
        BoxCollider box = trigger.GetComponent<BoxCollider>();
        if (box == null) return false;

        Vector3 center = box.transform.position + box.center;
        Vector3 size = Vector3.Scale(box.size, box.transform.lossyScale);

        Vector3 planeNormal = Vector3.right;
        Vector3 planePos = center + new Vector3(size.x * 0.5f, 0f, 0f); // X�� ���

        int cutCount = 0;
        foreach (var block in blocks)
        {
            if (block == null) continue;

            edgeCutter.Cut(block, planePos, planeNormal);
            cutCount++;
        }

        if (cutCount > 0 && scoreManager != null)
        {
            scoreManager.AddScore(cutCount * 10);
            return true;
        }

        return false;
    }
}