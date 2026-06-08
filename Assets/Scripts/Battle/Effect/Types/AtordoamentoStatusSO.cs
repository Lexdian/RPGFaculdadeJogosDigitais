using UnityEngine;

[CreateAssetMenu(menuName = "RPG/StatusEffect/AtordoamentoStatusSO")]
public class AtordoamentoStatusSO : StatusEffectSO
{
    public override void OnApply(BattleEntity target, BattleEntity source)
    {
        target.PopDamage(0, DamageType.Atordoado);
        Debug.Log($"{target.EntityName} foi atordoado por {source.EntityName} por {duracao} turnos!");
    }

    public override void OnTick(BattleEntity target, StatusEffectInstance instance)
    {
        target.PopDamage(0, DamageType.Atordoado);
    }

    public override void OnExpire(BattleEntity target)
    {
        Debug.Log($"Atordoamento de {target.EntityName} expirou — pode agir novamente.");
    }
}