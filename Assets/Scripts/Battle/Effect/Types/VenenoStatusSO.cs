using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(menuName = "RPG/StatusEffect/VenenoStatusSO")]
public class VenenoStatusSO : StatusEffectSO
{
    public override void OnApply(BattleEntity target, BattleEntity source)
    {
        Debug.Log($"{target} foi envenenado por {source}!");
    }

    public override void OnExpire(BattleEntity target)
    {
        Debug.Log($"Veneno expirou em {target.EntityName}");
    }

    public override void OnTick(BattleEntity target, StatusEffectInstance instance)
    {
        target.TakeFisicalDamage(instance.source, valorDano);
        Debug.Log($"{target} tomou: {valorDano} de dano de veneno!");
    }
}
