using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(menuName = "XRMenu/Stage Node")]
public class Stagenode : MenuNode
{
    // 난이도와 맵을 분리시켜 놓았기 때문에,
    // 어떤 맵이든 가능함.
    // 추후 맵을 선택하는 기능(options - grahpics 에서 선택)
    [Header("불러올 스테이지의 이름 - BuildSettings에 등록된 SceneName.")]
    public string sceneName;

    [Header("스테이지 데이터 (Inspector에 할당)")]
    public StageDataSO data;

    [Header("씬 전환을 담당할 SceneLoaderSO (Inspector에 할당)")]
    public SceneLoaderSO sceneLoader;

    /// <summary>
    /// Inspector의 On Select() 슬롯에 연결할 메서드.
    /// 이 메서드를 호출하면 내부에서 sceneLoader.LoadSceneWithFade(data) 가 실행됩니다.
    /// </summary>
    public void OnSelectStage()
    {
        if (sceneLoader == null)
        {
            Debug.LogError($"[StageNode:{label}] sceneLoader가 할당되지 않았습니다.");
            return;
        }
        if (data == null)
        {
            Debug.LogError($"[StageNode:{label}] StageDataSO(data)가 할당되지 않았습니다.");
            return;
        }
        // 실제로 currentStageData에 데이터를 기록하고 씬 전환
        sceneLoader.LoadScene(sceneName, data);

        // 만약 추가로 다른 UnityEvent(onSelect)가 걸려 있으면 실행
        // onSelect?.Invoke();
    }
}
