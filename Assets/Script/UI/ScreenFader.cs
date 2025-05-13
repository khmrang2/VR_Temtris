using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage; // UI용
    public float fadeDuration = 1f;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded; // 씬 전환 후 알파 초기화
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Image newFadeImage = GameObject.Find("FadeImage")?.GetComponent<Image>();
        if (newFadeImage != null)
        {
            fadeImage = newFadeImage;
            StartCoroutine(Fade(1f, 0f));
        }
    }


    public void FadeOutAndLoad(string sceneName)    
    {
        StartCoroutine(FadeAndSwitchScene(sceneName));
    }

    private IEnumerator FadeAndSwitchScene(string sceneName)
    {
        // 페이드 아웃
        yield return StartCoroutine(Fade(0f, 1f));
        SceneManager.LoadScene(sceneName);
        yield return null; // 씬 로드 후 한 프레임 대기
        // 페이드 인
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        Material mat = fadeImage.material;
        Color c = mat.color;
        float t = 0f;
        Debug.Log(c.ToString());
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);
            mat.color = new Color(c.r, c.g, c.b, a);
            Debug.Log($"Alpha: {a}"); // 👈 알파 변화 로그 찍기
            yield return null;
        }
    }
}
