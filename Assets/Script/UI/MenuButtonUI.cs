using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtonUI : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] Image    icon;

    void Awake()                 // ★ 자동 할당
    {
        if (!label) label = GetComponentInChildren<TMP_Text>(true);
    }

    public void Set(string text, Sprite spr)
    {
        if (label) label.text = text;

    }
}
