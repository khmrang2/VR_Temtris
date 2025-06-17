using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "XRMenu/Map Node")]

public class MapNode : MenuNode
{
    public MapType mapType;

    public void OnSelectMap()
    {
        Debug.Log("선택된 맵 변경됨 → " + mapType);
        MapSelectionManager.Instance.SetMap(mapType);
        // MenuController.Instance.OpenMenu(nextMenu); // 네비게이션 시스템에 따라 다르게 구현
    }
}
