using System.Collections.Generic;
using UnityEngine;

public class EnemyEntity : BattleEntity
{
    public EnemySO Data;

    public void Setup(EnemySO data)
    {
        Data = data;

        EntityName = data.enemyName;

        MaxHP = data.vida;
        CurrentHP = data.vida;

        MaxMP = data.mana;
        CurrentMP = data.mana;

        Ataque = data.ataqueFisico;
        AtaqueMagico = data.ataqueMagico;

        Defesa = data.defesaFisica;
        DefesaMagica = data.defesaMagica;

        Evasao = data.evasao;
        Agilidade = data.agilidade;
    }

    public override BattleDecision GetAction(List<BattleEntity> allEntities)
    {
        // IA futura
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