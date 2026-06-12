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
    public int GetResiliencia() => fichaBase.baseResiliencia + (fichaBase.perLevelUpgradeResiliencia * (nivelAtual - 1));
    public int GetSorte() => fichaBase.baseSorte + (fichaBase.perLevelUpgradeSorte * (nivelAtual - 1));
    
    public int GetMaxVidaTotal()       => GetMaxVida()      + SomarBonus(e => e.bonusVida);
    public int GetMaxManaTotal()       => GetMaxMana()      + SomarBonus(e => e.bonusMana);
    public int GetAtaqueFisico() => GetForca() + SomarBonus(e => e.bonusDanoFisico);
    public int GetAtaqueMagico() => GetInteligencia() + SomarBonus(e => e.bonusDanoMagico);
    public int GetDefesaFisicaTotal() => GetResiliencia() + SomarBonus(e => e.bonusDefesaFisica);
    public int GetDefesaMagicaTotal() => GetInteligencia() + SomarBonus(e => e.bonusDefesaMagica);
    public int GetEvasaoTotal() => (GetAgilidade()/2) + (GetSorte()/10) + SomarBonus(e => e.bonusEvasao);
    public int GetChanceCriticoTotal() => (GetAgilidade()/10) + (GetSorte()/5) + SomarBonus(e => e.bonusChanceCritico);
    public int GetVelocidadeTotal() => GetAgilidade() + SomarBonus(e => e.bonusVelocidade);
    public int GetPrecisaoTotal() => GetAgilidade()+ SomarBonus(e => e.bonusPrecisao);

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

    public void GanharXp(int xp)
    {
        // O XP atual só aumenta, nunca diminui!
        this.xpAtual += xp;

        // Pega a meta total de XP acumulada necessária para o próximo nível
        int metaTotalProximoNivel = GetXpTotalNecessariaParaNivel(this.nivelAtual + 1);

        // Enquanto o XP atual for maior ou igual à meta total do próximo nível, ele upa
        while (this.xpAtual >= metaTotalProximoNivel)
        {
            this.nivelAtual++;
            Debug.Log($"{fichaBase.charName} subiu para o nível {this.nivelAtual}!");

            // APRENDIZADO DE SKILLS: Checa se o novo nível libera alguma habilidade
            ChecarNovasHabilidades();

            // CURA COMPLETA (Opcional): Restaura os status dinâmicos
            this.vidaAtual = GetMaxVidaTotal();
            this.manaAtual = GetMaxManaTotal();

            // Atualiza a meta para o próximo nível após o upgrade (ex: se era o 2, agora checa a meta do 3)
            metaTotalProximoNivel = GetXpTotalNecessariaParaNivel(this.nivelAtual + 1);
        }
    }

    /// <summary>
    /// Calcula quanta XP CUMULATIVA TOTAL o herói precisa ter desde o nível 1 para atingir o nível alvo.
    /// </summary>
    public int GetXpTotalNecessariaParaNivel(int nivelAlvo)
    {
        if (nivelAlvo <= 1) return 0;

        int xpAcumuladaTotal = 0;

        // Soma os requisitos de todos os níveis anteriores para descobrir o valor total cumulativo
        for (int i = 1; i < nivelAlvo; i++)
        {
            // Requisito de "degrau" entre o nível i e o nível i+1
            float degrauXP = fichaBase.xpBaseNecessario * Mathf.Pow(fichaBase.curvaXPMultiplicador, i - 1);
            xpAcumuladaTotal += Mathf.RoundToInt(degrauXP);
        }

        return xpAcumuladaTotal;
    }

    /// <summary>
    /// Varre a lista de habilidades por nível do CharacterSO e adiciona à ficha se atingir o requisito.
    /// </summary>
    private void ChecarNovasHabilidades()
    {
        if (fichaBase.habilidadesPorNivel == null) return;

        foreach (var vinculo in fichaBase.habilidadesPorNivel)
        {
            if (this.nivelAtual >= vinculo.nivelNecessario && !Skills.Contains(vinculo.habilidade))
            {
                Skills.Add(vinculo.habilidade);
                Debug.Log($"{fichaBase.charName} aprendeu a habilidade: {vinculo.habilidade.skillName}!");
            }
        }
    }
}