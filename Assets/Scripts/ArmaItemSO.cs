using UnityEngine;

public enum SlotEquipamento { Cabeca, Corpo, Botas, Arma, Acessorio1, Acessorio2 }
public enum TipoArma { Espada, Cajado, Arco, Adaga, Machado }

[CreateAssetMenu(fileName = "NovoEquipavel", menuName = "RPG/Itens/Equipavel")]
public class EquipableItemSO : ItemSO
{
    [Header("Slot")]
    public SlotEquipamento slot;
    public TipoArma tipoArma; 

    [Header("Bônus de Status")]
    public int bonusVida;
    public int bonusMana;
    public int bonusForca;
    public int bonusInteligencia;
    public int bonusAgilidade;
    public int bonusResiliencia;
    public int bonusSorte;
}