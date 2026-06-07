using System.Collections.Generic;
using UnityEngine;

public class EnemyEntity : BattleEntity
{
    public EnemySO Data;

    private AbstractEnemyBehavior behavior;
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

        Velocidade = data.velocidade;
        Precisao = data.precisao;
        Evasao = data.evasao;

        ChanceCritico = data.chanceCritico;

        behavior = data.behavior;

        Skills = data.Skills;

        Icon = data.enemyIcon;

        AtaqueBasico = SkillSO.AtaqueBasico(data.delay, data.recuperacao, data.tipoAlvo, data.dano);
    }

    public BattleDecision GetAction(List<BattleEntity> allEntities)
    {
        return behavior.ChooseAction(allEntities, Skills, this);
    }

    public override void TakeFisicalDamage(BattleEntity dealer, int amount)
    {
        int danoFinal = Mathf.Max(1, amount - Defesa);

        CurrentHP -= danoFinal;

        PopDamage(danoFinal, DamageType.Normal);

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

        PopDamage(danoFinal, DamageType.Normal);

        Debug.Log($"{EntityName} recebeu {danoFinal} dano mágico");

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            Die();
        }
    }
}