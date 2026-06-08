using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(menuName = "RPG/StatusEffect/VenenoStatusSO")]
public class VenenoStatusSO : StatusEffectSO
{
    public override void OnApply(BattleEntity target, BattleEntity source)
    {
        Debug.Log($"{target.EntityName} foi envenenado por {source.name}!");
    }

    public override void OnExpire(BattleEntity target)
    {
        Debug.Log($"Veneno expirou em {target.EntityName}");
    }

    public override void OnTick(BattleEntity target, StatusEffectInstance instance)
    {
        target.TakeFisicalDamage(instance.source, valorDano);
        Debug.Log($"{target.EntityName} tomou: {valorDano} de dano de veneno!");
    }
}
