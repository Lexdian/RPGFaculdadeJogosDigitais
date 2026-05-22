using System.Collections.Generic;
using UnityEngine;

public class CharEntity : BattleEntity
{
    public CombatenteData Data;

    public void Setup(CombatenteData data)
    {
        Data = data;

        EntityName = data.fichaBase.charName;

        MaxHP = data.GetMaxVida();
        CurrentHP = data.vidaAtual;

        MaxMP = data.GetMaxMana();
        CurrentMP = data.manaAtual;

        Ataque = data.GetForca();
        AtaqueMagico = data.GetInteligencia();

        Defesa = data.GetResiliencia();
        DefesaMagica = data.GetInteligencia();

        Evasao = data.GetAgilidade()/2;
        Agilidade = data.GetAgilidade();
    }

    public override BattleDecision GetAction(List<BattleEntity> allEntities)
    {
        // Depois tu troca pela UI do jogador
        return new BattleDecision();
    }

    public override void TakeFisicalDamage(BattleEntity dealer, int amount)
    {
        int danoFinal = Mathf.Max(1, amount - Defesa);

        CurrentHP -= danoFinal;

        Debug.Log($"{EntityName} recebeu {danoFinal} dano físico");

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            Die();
        }
    }

    public override void TakeMagicalDamage(BattleEntity dealer, int amount)
    {
        int danoFinal = Mathf.Max(1, amount - DefesaMagica);

        CurrentHP -= danoFinal;

        Debug.Log($"{EntityName} recebeu {danoFinal} dano mágico");

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            Die();
        }
    }
}