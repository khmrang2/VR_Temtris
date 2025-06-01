using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "XRMenu/Scene Loader Asset")]
public class SceneLoaderSO : ScriptableObject
{
    // public ScreenFader fader; // 👈 인스펙터에서 연결할 수 있게
    public StageDataSO stageData;
    
    /// <summary>
    /// data와 Scene을 불러옴. 
    /// </summary>
    /// <param name="sceneName">불러올 build Scene 파일명</param>
    /// <param name="data">StageData -> Create/Game/StageData 로 생성.</param>
    public void LoadScene(string sceneName, StageDataSO data)
    {
        stageData = data;
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;   // 현재 선택된 오브젝트 초기화
#endif
        SceneManager.LoadScene(sceneName);
    }

    /*
    public void LoadSceneWithFade(string sceneName, StageDataSO data)
    {
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;   // 현재 선택된 오브젝트 초기화
#endif
        var fader = GameObject.FindObjectOfType<ScreenFader>();

        if (fader != null)
        {
            fader.FadeOutAndLoad(sceneName);
        }
        else
        {
            LoadScene(sceneName);
        }
    }
    */
}