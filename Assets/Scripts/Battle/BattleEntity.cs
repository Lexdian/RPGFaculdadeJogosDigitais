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
    public int Evasao;
    public int Agilidade;

    public List<SkillSO> Skills = new();

    [Header("Afinidades")]
    public List<TipoDano> Resistencias = new();
    public List<TipoDano> Fraquezas = new();
    public List<TipoDano> Imunidades = new();

    public bool IsAlive => CurrentHP > 0;

    [Header("Battle State")]
    public BattleState CurrentState = BattleState.WaitingAction;

    public int ReadyTurn = 0;

    public SkillSO AtaqueBasico;

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
            Debug.Log($"{EntityName} é imune a {tipo}");
            return;
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
            efeito.ApplyEffect(target, dealer, skill);
        }
    }

    public virtual void Heal(int amount)
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);

        Debug.Log($"{EntityName} curou {amount} HP");
    }

    protected virtual void Die()
    {
        Debug.Log($"{EntityName} morreu!");
    }

    public abstract void TakeFisicalDamage(BattleEntity dealer, int amount);

    public abstract void TakeMagicalDamage(BattleEntity dealer, int amount);
}