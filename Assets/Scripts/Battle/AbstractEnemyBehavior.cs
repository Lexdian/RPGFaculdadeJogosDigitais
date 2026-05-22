using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractEnemyBehavior
{
    public abstract BattleDecision ChooseAction(List<BattleEntity> allEntities, List<SkillSO> skills, EnemyEntity self);
}
