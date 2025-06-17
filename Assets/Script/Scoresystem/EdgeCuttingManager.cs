using System.Collections.Generic;
using UnityEngine;

public class EdgeCuttingManager : MonoBehaviour
{
    [Header("절단 헬퍼")]
    public EdgeCuttingHelper edgeCutter;

    [Header("점수 관리자")]
    public ScoreManager scoreManager;

    [Header("설정")]
    public float threshold = 0.8f;
    public float motionCheckDelay = 0.5f;

    /// <summary>
    /// 절단 또는 파괴 요청을 처리한다. 
    /// 조건에 따라 parent 관계를 해제하고 절단 또는 삭제한다.
    /// </summary>
    /// <param name="trigger">트리거 객체</param>
    /// <returns>절단 성공 여부</returns>
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

            if (!root.CompareTag("Block")) continue;

            // ✅ 절단 전 연결 해제 로직
            if (root.childCount > 0 || root.parent != null)
            {
                var attach = root.GetComponent<AttachableOnCollision>();
                if (attach != null)
                {
                    attach.DetachFromParent();
                    attach.canAttach = false;
                    attach.enabled = false;
                }
            }

            // ✅ 완전히 들어온 경우 삭제 처리
            float ratio = trigger.GetBlockFillRatio(root.gameObject);
            if (ratio >= 0.98f)
            {
                Debug.Log("[CuttingManager] 블럭의 하위 collider 기준 98% 포함됨 → 삭제 처리");
                Destroy(root.gameObject);
                cutCount++;
                continue;
            }

            // ✅ 그 외에는 절단 시도
            edgeCutter.Cut(root.gameObject, upperPos, upperNormal);
            cutCount++;
        }

        if (cutCount >= blocks.Count * 0.8f)
        {
            scoreManager?.AddScore(10);
            return true;
        }

        return false;
    }

    public float getThreshold() { return this.threshold; }
    public float getMotionCheckDelay() { return this.motionCheckDelay; }
}
