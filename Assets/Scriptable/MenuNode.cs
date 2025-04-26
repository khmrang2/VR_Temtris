using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "XRMenu/Menu Node")]
public class MenuNode : ScriptableObject
{
    public string label;
    public Sprite icon;
    public UnityEvent onSelect;          // 최하위 항목일 때 실행
    public List<MenuNode> children;      // 서브 메뉴
}
