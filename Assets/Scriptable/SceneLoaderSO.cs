using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "XRMenu/Scene Loader Asset")]
public class SceneLoaderSO : ScriptableObject
{
    public void LoadScene(string sceneName)
    {
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;   // 현재 선택된 오브젝트 초기화
#endif
        SceneManager.LoadScene(sceneName);
    }
}