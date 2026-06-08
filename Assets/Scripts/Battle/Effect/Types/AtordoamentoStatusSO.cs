using UnityEngine;

/// <summary>
/// Status de Atordoamento: impede a entidade de agir durante 'duracao' turnos.
/// O BattleManager verifica HasStatusEffect&lt;AtordoamentoStatusSO&gt;() em
/// AskForActionsCoroutine para pular a vez do afetado.
/// </summary>
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
        // Nenhum dano — a lógica de pular a vez está no BattleManager.
        // O popup a cada tick reforça o estado visualmente.
        target.PopDamage(0, DamageType.Atordoado);
    }

    public override void OnExpire(BattleEntity target)
    {
        Debug.Log($"Atordoamento de {target.EntityName} expirou — pode agir novamente.");
    }
}