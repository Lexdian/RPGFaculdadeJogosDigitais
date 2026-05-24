using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CombatenteData
{
    public CharacterSO fichaBase;
    public int nivelAtual = 1;
    public int xpAtual = 0;

    [Header("Status Dinâmicos")]
    public int vidaAtual;
    public int manaAtual;

    [Header("Equipamentos")]
    public EquipableItemSO cabeca;
    public EquipableItemSO corpo;
    public EquipableItemSO botas;
    public EquipableItemSO arma;
    public EquipableItemSO acessorio1;
    public EquipableItemSO acessorio2;

    public CombatenteData(CharacterSO baseData, int nivel)
    {
        this.fichaBase = baseData;
        this.nivelAtual = nivel;
        this.vidaAtual = GetMaxVidaTotal();
        this.manaAtual = GetMaxManaTotal();
        this.Skills = new List<SkillSO>(fichaBase.habilidadesIniciais);
    }

    public int GetMaxVida()       => fichaBase.baseVida          + (fichaBase.perLevelUpgradeVida          * (nivelAtual - 1));
    public int GetMaxMana()       => fichaBase.baseMana          + (fichaBase.perLevelUpgradeMana          * (nivelAtual - 1));
    public int GetForca()         => fichaBase.baseForca         + (fichaBase.perLevelUpgradeForca         * (nivelAtual - 1));
    public int GetInteligencia()  => fichaBase.baseInteligencia  + (fichaBase.perLevelUpgradeInteligencia  * (nivelAtual - 1));
    public int GetAgilidade()     => fichaBase.baseAgilidade     + (fichaBase.perLevelUpgradeAgilidade     * (nivelAtual - 1));
    public int GetResiliencia() => fichaBase.baseResiliencia + (fichaBase.perLevelUpgradeResiliencia * (nivelAtual - 1)) + SomarBonus(e => e.bonusResiliencia);
    public int GetSorte()       => fichaBase.baseSorte       + (fichaBase.perLevelUpgradeSorte       * (nivelAtual - 1)) + SomarBonus(e => e.bonusSorte); 
    
    public int GetMaxVidaTotal()       => GetMaxVida()      + SomarBonus(e => e.bonusVida);
    public int GetMaxManaTotal()       => GetMaxMana()      + SomarBonus(e => e.bonusMana);
    public int GetForcaTotal()         => GetForca()        + SomarBonus(e => e.bonusForca);
    public int GetInteligenciaTotal()  => GetInteligencia() + SomarBonus(e => e.bonusInteligencia);
    public int GetAgilidadeTotal()     => GetAgilidade()    + SomarBonus(e => e.bonusAgilidade);
    public int GetDefesaFisicaTotal() => SomarBonus(e => e.bonusDefesaFisica);
    public int GetDefesaMagicaTotal() => SomarBonus(e => e.bonusDefesaMagica);

    public List<SkillSO> Skills = new();

    public void Equipar(EquipableItemSO item)
    {
        EquipableItemSO atual = GetSlot(item.slot);

        if (atual != null)
            GameManager.Instance.inventarioGrupo.TryAdd(atual);

        SetSlot(item.slot, item);
        GameManager.Instance.inventarioGrupo.TryRemove(item);
    }

    public void Desequipar(SlotEquipamento slot)
    {
        EquipableItemSO atual = GetSlot(slot);
        if (atual == null) return;

        GameManager.Instance.inventarioGrupo.TryAdd(atual);
        SetSlot(slot, null);
    }

    private EquipableItemSO GetSlot(SlotEquipamento slot) => slot switch
    {
        SlotEquipamento.Cabeca     => cabeca,
        SlotEquipamento.Corpo      => corpo,
        SlotEquipamento.Botas      => botas,
        SlotEquipamento.Arma       => arma,
        SlotEquipamento.Acessorio1 => acessorio1,
        SlotEquipamento.Acessorio2 => acessorio2,
        _ => null
    };

    private void SetSlot(SlotEquipamento slot, EquipableItemSO item)
    {
        switch (slot)
        {
            case SlotEquipamento.Cabeca:      cabeca     = item; break;
            case SlotEquipamento.Corpo:       corpo      = item; break;
            case SlotEquipamento.Botas:       botas      = item; break;
            case SlotEquipamento.Arma:        arma       = item; break;
            case SlotEquipamento.Acessorio1:  acessorio1 = item; break;
            case SlotEquipamento.Acessorio2:  acessorio2 = item; break;
        }
    }

    private int SomarBonus(System.Func<EquipableItemSO, int> selector)
    {
        int total = 0;
        foreach (var equip in new[] { cabeca, corpo, botas, arma, acessorio1, acessorio2 })
            if (equip != null) total += selector(equip);
        return total;
    }
}