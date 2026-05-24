using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class AbstractEnemyBehavior
{
    public abstract BattleDecision ChooseAction(List<BattleEntity> allEntities, List<SkillSO> skills, EnemyEntity self);
}
