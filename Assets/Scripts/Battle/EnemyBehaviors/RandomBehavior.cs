using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RandomBehavior : AbstractEnemyBehavior
{
    public override BattleDecision ChooseAction(List<BattleEntity> allEntities, List<SkillSO> skills, EnemyEntity self)
    {
        BattleEntity target = allEntities[UnityEngine.Random.Range(0, allEntities.Count)];
        SkillSO skill = skills[UnityEngine.Random.Range(0, skills.Count)];

        if(self.CurrentMP < skill.custoMana)
        {
            Debug.Log($"{self.EntityName} escolheu usar Ataque Básico em {target.EntityName}");
            return new BattleDecision { skill = self.AtaqueBasico, targets = new BattleEntity[] { target } };
        }
        Debug.Log($"{self.EntityName} escolheu usar {skill.skillName} em {target.EntityName}");
        return new BattleDecision { skill = skill, targets = new BattleEntity[] { target } };
    }
}
