using UnityEngine;
using System.Collections;

public class PooledEffect : MonoBehaviour
{
    private ParticleSystem ps;

    void OnEnable()     
    {
        if (ps == null)
            ps = GetComponent<ParticleSystem>();

        ps.Play();  

        StartCoroutine(DisableAfterDuration(ps.main.duration));
    }

    IEnumerator DisableAfterDuration(float time)    // 이펙트 재생 이후 비활성화
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}
