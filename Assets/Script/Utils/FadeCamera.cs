using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public delegate void Delegate(string argument);

public class FadeCamera : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 2f; // fade할 시간.
    [SerializeField] private Color _fadeColor = Color.black;

    [SerializeField] private bool _fadeOnStart = true;
    
    private Renderer _rend;
    // Start is called before the first frame update
    void Start()
    {
        _rend = GetComponent<Renderer>();
        if (_fadeOnStart)
        {
            fadeIn();
        }
    }


    /// <summary>
    /// fade-in 코루틴을 호출할 메소드
    /// </summary>
    public void fadeIn()
    {
        Debug.Log("페이드 인 시작");
        StartCoroutine(FadeRoutine(1, 0));
    }

    /// <summary>
    /// fade out 메소드
    /// </summary>
    /// <param name="functionToCall"></param>
    /// <param name="strParameter"></param>
    public void fadeOut(Delegate functionToCall, string strParameter)
    {
        Debug.Log("페이드 아웃 시작");
        StartCoroutine(FadeRoutineWithFinalFunctionCall(functionToCall, strParameter, 0, 1));
    }

    /// <summary>
    /// fade in 코루틴
    /// </summary>
    /// <param name="alphaIn"></param>
    /// <param name="alphaOut"></param>
    /// <returns></returns>
    public IEnumerator FadeRoutine(float alphaIn, float alphaOut)
    {
        float timer = 0;
        while (timer <= _fadeDuration)
        {
            Color newColor = _fadeColor;
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / _fadeDuration);
            _rend.material.SetColor("_Color", newColor);
            timer += Time.deltaTime;
            yield return null;
        }
        Color newColor2 = _fadeColor;
        newColor2.a = alphaOut;
        _rend.material.SetColor("_Color", newColor2);
        Debug.Log("꺼졌냐?");
        _rend.gameObject.SetActive(false);
        Debug.Log("꺼졌냐??");
    }
    /// <summary>
    /// fade out 코루틴
    /// </summary>
    /// <param name="functionToCall"></param>
    /// <param name="strParameter"></param>
    /// <param name="alphaIn"></param>
    /// <param name="alphaOut"></param>
    /// <returns></returns>
    public IEnumerator FadeRoutineWithFinalFunctionCall(Delegate functionToCall, string strParameter, float alphaIn, float alphaOut)
    {
        float timer = 0;
        while (timer <= _fadeDuration)
        {
            Color newColor = _fadeColor;
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / _fadeDuration);
            _rend.material.SetColor("_Color", newColor);
            timer += Time.deltaTime;
            yield return null;
        }
        Color newColor2 = _fadeColor;
        newColor2.a = alphaOut;
        _rend.material.SetColor("_Color", newColor2);
        functionToCall(strParameter);
    }
}
