using UnityEngine;

[CreateAssetMenu(fileName = "NovoMaterial", menuName = "RPG/Itens/Material")]
public class MaterialItemSO : ItemSO
{
    [Header("Uso")]
    public bool usadoEmCraft;
    public bool usadoEmQuest;
}