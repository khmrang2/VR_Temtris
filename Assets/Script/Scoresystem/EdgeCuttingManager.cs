using System.Collections.Generic;
using UnityEngine;

public class EdgeCuttingManager : MonoBehaviour
{
    [Header("절단 헬퍼")]
    public EdgeCuttingHelper edgeCutter;

    [Header("점수 관리자")]
    public ScoreManager scoreManager;

    public bool RequestCut(LineTrigger trigger)
    {
        if (trigger == null || edgeCutter == null) return false;

        List<GameObject> blocks = trigger.GetCurrentBlocks();
        if (blocks == null || blocks.Count == 0) return false;

        Vector3 upperPos = trigger.GetUpperPlanePos();
        Vector3 upperNormal = trigger.GetUpperPlaneNormal();
        Vector3 lowerPos = trigger.GetLowerPlanePos();
        Vector3 lowerNormal = trigger.GetLowerPlaneNormal();

        int cutCount = 0;
        foreach (var block in blocks)
        {
            Transform root = block.transform;
            while (root.parent != null)
                root = root.parent;

            if (root.CompareTag("Block"))
            {
                edgeCutter.Cut(root.gameObject, upperPos, upperNormal);
                edgeCutter.Cut(root.gameObject, lowerPos, lowerNormal);
                cutCount++;
            }
        }

        if (cutCount >= blocks.Count * 0.8f)
        {
            scoreManager?.AddScore(10);
            return true;
        }

        return false;
    }
}