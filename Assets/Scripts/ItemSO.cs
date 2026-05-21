using UnityEngine;

public enum ItemCategory { Consumivel, Equipavel, Material }

[CreateAssetMenu(fileName = "NovoItem", menuName = "RPG/Item")]
public class ItemSO : ScriptableObject
{
    [Header("Identidade")]
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Classificação")]
    public ItemCategory category;
    public bool stackable = true;
    public int maxStack = 99;
}