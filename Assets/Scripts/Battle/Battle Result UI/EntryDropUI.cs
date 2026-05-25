using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntryDropUI : MonoBehaviour
{
    public Image imgItem;
    public TMP_Text txtNome;
    public TMP_Text txtQtd;

    public void Setup(ItemSO item, int quantidade)
    {
        if (item == null) return;

        if (imgItem != null && item.icon != null)
            imgItem.sprite = item.icon;

        if (txtNome != null)
            txtNome.text = item.itemName;

        if (txtQtd != null)
            txtQtd.text = quantidade > 1 ? $"x{quantidade}" : "";
    }
}