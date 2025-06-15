using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public int attachCount; // 붙일 횟수
    public LayerMask blockLayer; // layer를 block으로 설정해야함

    private void OnCollisionEnter(Collision collision)
    {
        GameObject block = collision.gameObject;
        if (((1 << block.layer) & blockLayer) == 0 || block == gameObject) return;

        var temp = block.GetComponent<AttachableOnCollision>();
        if (!temp.canAttach)
        {
            temp.canAttach = true;
            attachCount--;
            Debug.Log($"블럭 {block.name} 붙일 준비 완료.");
        }
        else return;

        if (attachCount < 1)
        {
            Destroy(gameObject);
            Debug.Log($"아이템 '풀' {gameObject.name} 삭제 완료.");
            return;
        }

        Debug.Log($"블럭 붙일 횟수 {attachCount} 남음.");

    }
}
