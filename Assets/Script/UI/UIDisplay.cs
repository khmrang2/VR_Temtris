using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text descText;
    [SerializeField] Image iconImage;

    public void Setup(ItemInfo info)
    {
        nameText.text = info.itemName;
        descText.text = info.description;
        iconImage.sprite = info.icon;
    }
}