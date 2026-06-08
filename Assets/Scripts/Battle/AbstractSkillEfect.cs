using System;
using UnityEngine;

[Serializable]
public abstract class AbstractSkillEfect 
{
    public abstract void ApplyEffect(BattleEntity target, BattleEntity dealler, SkillSO skill);
}
