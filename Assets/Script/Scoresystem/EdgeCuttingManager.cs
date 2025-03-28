using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EdgeCuttingManager : MonoBehaviour
{
    [Header("절단 기능")]
    public EdgeCuttingHelper edgeCutter;

    [Header("점수 시스템 (선택)")]
    public ScoreManager scoreManager;

    /// <summary>
    /// 특정 LineTrigger에서 절단 요청이 왔을 때 호출
    /// </summary>
    public bool RequestCut(LineTrigger trigger)
    {
        Debug.Log("절단 요청 발생!");

        if (trigger == null || edgeCutter == null) return false;

        List<GameObject> blocks = trigger.GetCurrentBlocks();
        BoxCollider box = trigger.GetComponent<BoxCollider>();
        if (box == null) return false;

        Vector3 center = box.transform.position + box.center;
        Vector3 size = Vector3.Scale(box.size, box.transform.lossyScale);

        Vector3 planeNormal = Vector3.right;
        Vector3 planePos = center + new Vector3(size.x * 0.5f, 0f, 0f); // X축 상단

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