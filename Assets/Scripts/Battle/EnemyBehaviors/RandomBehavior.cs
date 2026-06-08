using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RandomBehavior : AbstractEnemyBehavior
{
    public override BattleDecision ChooseAction(List<BattleEntity> allEntities, List<SkillSO> skills, EnemyEntity self)
    {
        // 1. FILTRAGEM: Encontra apenas os heróis que estão vivos na arena
        // (Ajuste o Tipo ou Tag se a sua classe de herói tiver outro nome, ex: 'CharEntity')
        List<BattleEntity> heroisVivos = allEntities
            .Where(e => e.IsAlive && e.GetType() != typeof(EnemyEntity))
            .ToList();

        // Escolhe um herói alvo aleatório dentre os que estão vivos
        BattleEntity target = heroisVivos[UnityEngine.Random.Range(0, heroisVivos.Count)];

        // 2. SELEÇÃO DE SKILL: Filtra apenas as magias que o inimigo TEM MANA para usar
        List<SkillSO> skillsDisponiveis = skills
            .Where(s => self.CurrentMP >= s.custoMana)
            .ToList();

        // 3. TOMADA DE DECISÃO
        if (skillsDisponiveis.Count > 0)
        {
            // Se ele tiver mana, escolhe uma das habilidades válidas da lista
            SkillSO skillEscolhida = skillsDisponiveis[UnityEngine.Random.Range(0, skillsDisponiveis.Count)];

            Debug.Log($"{self.EntityName} escolheu usar {skillEscolhida.skillName} em {target.EntityName}");
            return new BattleDecision { skill = skillEscolhida, targets = new BattleEntity[] { target } };
        }
        else
        {
            // Se não tiver mana para NENHUMA habilidade da lista, usa o Ataque Básico de segurança
            Debug.Log($"{self.EntityName} está sem mana e escolheu usar Ataque Básico em {target.EntityName}");
            return new BattleDecision { skill = self.AtaqueBasico, targets = new BattleEntity[] { target } };
        }
    }
}