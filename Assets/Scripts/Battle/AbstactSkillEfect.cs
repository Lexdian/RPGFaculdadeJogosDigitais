using System;
using UnityEngine;

[Serializable]
public abstract class AbstactSkillEfect
{
    public abstract void ApplyEffect(BattleEntity target, BattleEntity dealler, SkillSO skill);
}
