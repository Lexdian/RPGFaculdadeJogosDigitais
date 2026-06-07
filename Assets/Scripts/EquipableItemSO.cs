using UnityEngine;

public enum SlotEquipamento { Cabeca, Corpo, Botas, Arma, Acessorio1, Acessorio2 }

[CreateAssetMenu(fileName = "NovoEquipavel", menuName = "RPG/Itens/Equipavel")]
public class EquipableItemSO : ItemSO
{
    [Header("Slot")]
    public SlotEquipamento slot;

    [Header("Bônus de Status")]
    public int bonusVida;
    public int bonusMana;

    [Header("Bônus de Ordem")]
    public int bonusVelocidade;

    [Header("Ataque")]
    public int bonusDanoFisico;
    public int bonusDanoMagico;
    public int bonusChanceCritico;
    public int bonusPrecisao;

    [Header("Defesa")]
    public int bonusDefesaFisica;
    public int bonusDefesaMagica;
    public int bonusEvasao;
}