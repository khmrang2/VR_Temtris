using UnityEngine;

public enum MapType { Desert, Water }

public class MapSelectionManager : MonoBehaviour
{
    public static MapSelectionManager Instance;

    public MapType SelectedMap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 넘어가도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMap(MapType map)
    {
        SelectedMap = map;
        Debug.Log("선택된 맵: " + map);
    }
}
