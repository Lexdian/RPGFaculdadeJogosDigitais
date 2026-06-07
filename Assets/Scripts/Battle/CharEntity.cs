using System.Collections.Generic;
using UnityEngine;

public class CharEntity : BattleEntity
{
    public CombatenteData Data;

    public bool DecididoNoTurno { get; private set; }
    private BattleDecision decisaoSelecionada;

    public void Setup(CombatenteData data)
    {
        Data = data;

        EntityName = data.fichaBase.charName;

        MaxHP = data.GetMaxVida();
        CurrentHP = data.vidaAtual;

        MaxMP = data.GetMaxMana();
        CurrentMP = data.manaAtual;

        Ataque = data.GetAtaqueFisico();
        AtaqueMagico = data.GetAtaqueMagico();

        Defesa = data.GetDefesaFisicaTotal();
        DefesaMagica = data.GetDefesaMagicaTotal();

        Velocidade = data.GetVelocidadeTotal();
        Evasao = data.GetEvasaoTotal();
        Precisao = data.GetPrecisaoTotal();

        ChanceCritico = data.GetChanceCriticoTotal();

        Skills = data.Skills;

        Icon = data.fichaBase.charPortrait;

        AtaqueBasico = SkillSO.AtaqueBasico(1, 1, TipoAlvo.Unico, TipoDano.Normal);
    }

    public void EscoolherAcaoDoPlayer(List<BattleEntity> todasAsEntidades, MenuFocadoNoPlayer menuUI)
    {
        DecididoNoTurno = false;

        if (menuUI != null)
        {
            Debug.LogWarning("MenuFocadoNoPlayer encontrado!");
            menuUI.FocarNoPlayer(this, todasAsEntidades);
        }
        else
        {
            Debug.LogWarning("MenuFocadoNoPlayer não encontrado! Defina a referência no BattleManager.");
        }
    }

    // Função que a UI vai chamar de volta para entregar a decisão
    public void DefinirDecisao(BattleDecision decision)
    {
        decisaoSelecionada = decision;
        DecididoNoTurno = true; // Libera o BattleManager para continuar
    }

    public BattleDecision ObterDecisaoFinal()
    {
        return decisaoSelecionada;
    }

    public override void TakeFisicalDamage(BattleEntity dealer, int amount)
    {
        int danoFinal = Mathf.Max(1, amount - Defesa);

        CurrentHP -= danoFinal;

        PopDamage(danoFinal, DamageType.Normal);

        Debug.Log($"{EntityName} recebeu {danoFinal} dano físico");

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            Die();
        }
    }

    public override void TakeMagicalDamage(BattleEntity dealer, int amount)
    {
        int danoFinal = Mathf.Max(1, amount - DefesaMagica);

        CurrentHP -= danoFinal;

        PopDamage(danoFinal, DamageType.Normal);

        Debug.Log($"{EntityName} recebeu {danoFinal} dano mágico");

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            Die();
        }
    }
}