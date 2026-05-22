using UnityEngine;

[System.Serializable]
public class StatusEffectInstance
{
    public BattleEntity source;
    public int turnosRestantes;
    public StatusEffectSO status;

    public StatusEffectInstance(StatusEffectSO statusEffectSO, BattleEntity dealler)
    {
        status = statusEffectSO;
        source = dealler;
        turnosRestantes = statusEffectSO.duracao;
    }

    public bool Tick(BattleEntity target)
    {
        status.OnTick(target, this);

        if (status.duracaoStatus == DuracaoStatus.Combate)
        {
            turnosRestantes--;
            if (turnosRestantes <= 0)
            {
                status.OnExpire(target);
                return false;
            }
        }

        return true;
    }
}