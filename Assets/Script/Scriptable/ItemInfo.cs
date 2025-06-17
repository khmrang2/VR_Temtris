using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "XRDemo/Item info")]
public class ItemInfo : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public float weight;
}
