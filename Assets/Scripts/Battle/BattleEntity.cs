using System.Collections.Generic;
using UnityEngine;

public abstract class BattleEntity : MonoBehaviour
{
    public string EntityName;
    [Header("Veda & MP")]
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

    [Header("Afinidade Elemental")]
    public List<TipoDano> Resistencias; // Lista de tipos de dano que a entidade щ resistente
    public List<TipoDano> Fraquezas; // Lista de tipos de dano que a entidade щ fraca
    public List<TipoDano> Imunidades; // Lista de tipos de dano que a entidade щ imune

    public bool IsAlive => CurrentHP > 0;
    public bool IsActiong = false; // Flag para indicar se a entidade estс realizando uma aчуo

    public int AttackColdown = 0; // Turnos para recarregar o ataque

    public abstract SkillSO GetAction(); // Sprite para a batalha

    

    private void ApplyExtraEffects(BattleEntity target, BattleEntity dealler, SkillSO skill)
    {
        if (skill.efeitosExtras != null)
        {
            foreach (var efeito in skill.efeitosExtras)
            {
                efeito.ApplyEffect(target, dealler, skill);
            }
        }
    }

    public void ReceiveAction(BattleEntity dealler, SkillSO skill)
    {
        if (skill.categoria == CategoriaHabilidade.Cura)
        {
            RecebeuCura(skill.poderBase);
            ApplyExtraEffects(this, dealler, skill);
        }
        if (skill.categoria == CategoriaHabilidade.Suporte)
        {
            ApplyExtraEffects(this, dealler, skill);
            Debug.Log($"{EntityName} recebeu um efeito de suporte: {skill.skillName}.");
        }
        else
        {
            TakeDamage(dealler, skill.poderBase, skill.tipoDano, skill.categoria);
            ApplyExtraEffects(this, dealler, skill);
        }
    }
    private void TakeDamage(BattleEntity dealler, int amount, TipoDano td, CategoriaHabilidade ch)
    {
        if (Imunidades.Contains(td))
        {
            Debug.Log($"{EntityName} щ imune a {td}!");
            return;
        }
        if (Resistencias.Contains(td))
        {
            amount = Mathf.Max(0, amount - 10); // Reduz o dano em 10 pontos
            Debug.Log($"{EntityName} щ resistente a {td}! Dano reduzido para {amount}.");
        }
        else if (Fraquezas.Contains(td))
        {
            amount += 10; // Aumenta o dano em 10 pontos
            Debug.Log($"{EntityName} щ fraco a {td}! Dano aumentado para {amount}.");
        }
        if (CategoriaHabilidade.Fisico == ch)
        {
            TakeFisicalDamage(dealler, amount);
        }
        else
        {
            TakeMagicalDamage(dealler, amount);
        }
    }

    public abstract void TakeFisicalDamage(BattleEntity dealler, int amount);
    public abstract void TakeMagicalDamage(BattleEntity dealler, int amount);

    public virtual void RecebeuCura(int amount)
    {
        Heal(amount);
    }
    private void Heal(int amount)
    {
        this.CurrentHP = Mathf.Min(this.CurrentHP + amount, this.MaxHP);
    }
}
