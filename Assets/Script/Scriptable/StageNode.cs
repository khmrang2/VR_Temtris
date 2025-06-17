using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(menuName = "XRMenu/Stage Node")]

public class StageSelectorNode : MenuNode
{
    [Header("사막맵용 씬")]
    public string desertSceneName;

    [Header("워터맵용 씬")]
    public string waterSceneName;

    [Header("공통 스테이지 데이터")]
    public StageDataSO stageData;

    [Header("씬 전환 담당 SceneLoader SO")]
    public SceneLoaderSO sceneLoader;

    [ContextMenu("디버그용 수동 실행")]
    public void OnSelectStage()
    {
        if (MapSelectionManager.Instance == null)
        {
            Debug.LogError("MapSelectionManager가 존재하지 않음!");
            return;
        }

        string targetScene = MapSelectionManager.Instance.SelectedMap == MapType.Desert
            ? desertSceneName
            : waterSceneName;

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("타겟 씬이 설정되어 있지 않음!");
            return;
        }

        if (stageData == null)
        {
            Debug.LogWarning("StageData가 연결되어 있지 않음!");
        }
        Debug.Log(targetScene + " Scence 이름");
        sceneLoader.LoadScene(targetScene, stageData);
    }
}

