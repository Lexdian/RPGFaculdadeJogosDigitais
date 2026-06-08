using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    WaitingAction,
    Preparing,
    Resting,
    Dead
}

public struct BattleDecision
{
    public SkillSO skill;
    public BattleEntity[] targets;
}


public abstract class BattleEntity : MonoBehaviour
{
    [Header("Info")]
    public string EntityName;
    public Sprite Icon;

    [Header("Vida & Mana")]
    public int MaxHP;
    public int CurrentHP;

    public int MaxMP;
    public int CurrentMP;

    [Header("Atributos")]
    public int Ataque;
    public int AtaqueMagico;
    public int Defesa;
    public int DefesaMagica;
    public int Velocidade;
    public int Precisao;
    public int Evasao;
    public int ChanceCritico;

    public List<SkillSO> Skills = new();

    [Header("Afinidades")]
    public List<TipoDano> Resistencias = new();
    public List<TipoDano> Fraquezas = new();
    public List<TipoDano> Imunidades = new();

    public bool IsAlive => CurrentHP > 0;

    [Header("Battle State")]
    public BattleState CurrentState = BattleState.WaitingAction;
    public List<StatusEffectInstance> statusAtivos = new();


    public int ReadyTurn = 0;
    public SkillSO AtaqueBasico;

    public void ApplyStatusEffect(StatusEffectSO status, BattleEntity dealler)
    {
        if (!status.IsStackable)
        {
            var statusExistente = statusAtivos.Find(e => e.status == status);
            if (statusExistente != null)
            {
                statusExistente.turnosRestantes = status.duracao;
                return;
            }

        }

        var statusInstance = new StatusEffectInstance(status, dealler);
        status.OnApply(this, dealler);
        statusAtivos.Add(statusInstance);
    }
    public void TickAllStatus()
    {
        statusAtivos.RemoveAll(e => !e.Tick(this));
        if (CurrentHP <= 0 && CurrentState != BattleState.Dead)
            TriggerDeath();
    }


    public void TakeStatusDamage(int amount, DamageType popupType = DamageType.Normal)
    {
        int dano = Mathf.Max(1, amount);
        CurrentHP = Mathf.Max(0, CurrentHP - dano);
        PopDamage(dano, popupType);
        if (CurrentHP <= 0 && CurrentState != BattleState.Dead)
            TriggerDeath();
    }

    public bool HasStatusEffect<T>() where T : StatusEffectSO
    {
        return statusAtivos.Exists(e => e.status is T);
    }

    public void TriggerDeath()
    {
        CurrentHP = 0;
        CurrentState = BattleState.Dead;
        Die();
    }

    public void RemoveStatusEffect(StatusEffectSO status)
    {
        var statusInstance = statusAtivos.Find(e => e.status == status);
        if (statusInstance != null)
        {
            status.OnExpire(this);
            statusAtivos.Remove(statusInstance);
        }
    }

    public virtual void ReceiveAction(BattleEntity dealer, SkillSO skill)
    {
        switch (skill.categoria)
        {
            case CategoriaHabilidade.Cura:
                Heal(skill.poderBase);
                break;

            case CategoriaHabilidade.Suporte:
                ApplyExtraEffects(this, dealer, skill);
                break;

            default:
                TakeDamage(dealer, skill.poderBase, skill.tipoDano, skill.categoria);
                ApplyExtraEffects(this, dealer, skill);
                break;
        }
    }

    private void TakeDamage(BattleEntity dealer, int amount, TipoDano tipo, CategoriaHabilidade categoria)
    {
        if (Imunidades.Contains(tipo))
        {
            PopDamage(0, DamageType.Imune);
            return;
        }
        if (Random.Range(0, 100) < 25 + dealer.Precisao - Evasao)
        {
            PopDamage(0, DamageType.Errou);
            return;
        }
        if (Random.Range(0, 100) < ChanceCritico)
        {
            amount = Mathf.CeilToInt(amount * 1.5f);
        }

        if (Resistencias.Contains(tipo))
            amount -= 10;

        if (Fraquezas.Contains(tipo))
            amount += 10;

        amount = Mathf.Max(0, amount);

        if (categoria == CategoriaHabilidade.Fisico)
            TakeFisicalDamage(dealer, amount);
        else
            TakeMagicalDamage(dealer, amount);
    }

    protected virtual void ApplyExtraEffects(BattleEntity target, BattleEntity dealer, SkillSO skill)
    {
        if (skill.efeitosExtras == null) return;

        foreach (var efeito in skill.efeitosExtras)
        {
            this.ApplyStatusEffect(efeito, dealer);
        }
    }

    public virtual void Heal(int amount)
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
        PopDamage(amount, DamageType.Heal);

        Debug.Log($"{EntityName} curou {amount} HP");
    }

    public virtual void Die()
    {
        Debug.Log($"{EntityName} morreu!");
    }

    private int GetAltura()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        return spriteRenderer != null ? (int)spriteRenderer.bounds.size.y : 1;
    }

    public void PopDamage(int danoFinal, DamageType damageType)
    {
        PopupDamage.Create(new Vector3(transform.position.x, transform.position.y + GetAltura(), transform.position.z), danoFinal, damageType);
    }

    public abstract void TakeFisicalDamage(BattleEntity dealer, int amount);

    public abstract void TakeMagicalDamage(BattleEntity dealer, int amount);
}