using UnityEngine;

using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    /* 씬 이름을 받아 바로 로드 */
    public void LoadScene(string sceneName)
    {
        Debug.Log($"LoadScene called: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
