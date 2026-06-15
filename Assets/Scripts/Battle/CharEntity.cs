using Cinemachine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class CharEntity : BattleEntity
{
    public CombatenteData Data;

    [Header("Câmera Cinemachine")]
    [SerializeField] private CinemachineVirtualCamera minhaVirtualCamera;

    public CinemachineVirtualCamera MinhaCamera => minhaVirtualCamera;

    public bool TemAcaoSelecionada { get; private set; }

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

        AtaqueBasico = SkillSO.AtaqueBasico(1, 1, TipoAlvo.Unico, TipoDano.Normal, data.fichaBase.prefabEfeitoVisualAtaqueBasico);
    }

    public void EndUpdate(int xp)
    {
        Data.vidaAtual = Mathf.Max(1, CurrentHP);
        Data.manaAtual = CurrentMP;
        Data.GanharXp(xp);
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
            Debug.LogWarning("MenuFocadoNoPlayer n�o encontrado! Defina a refer�ncia no BattleManager.");
        }
    }

    public void SelecionandoAcao()
    {
        TemAcaoSelecionada = false;
    }
    public void SelecionandoAlvo()
    {
        TemAcaoSelecionada = true;
    }
    // Fun��o que a UI vai chamar de volta para entregar a decis�o
    public void DefinirDecisao(BattleDecision decision)
    {
        decisaoSelecionada = decision;
        DecididoNoTurno = true; // Libera o BattleManager para continuar
        TemAcaoSelecionada = false; // Fecha a UI de escolha de alvo
    }

    public BattleDecision ObterDecisaoFinal()
    {
        DecididoNoTurno = false;
        return decisaoSelecionada;
    }

    public override void Die()
    {
        base.Die();

        Data.vidaAtual = 0;
        CurrentState = BattleState.Dead;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(1f, 1f, 1f, 0.25f);
    }

    public override void TakeFisicalDamage(BattleEntity dealer, int amount)
    {
        int danoFinal = Mathf.Max(1, amount - Defesa);

        CurrentHP -= danoFinal;

        PopDamage(danoFinal, DamageType.Normal);

        Debug.Log($"{EntityName} recebeu {danoFinal} dano f�sico");

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

        Debug.Log($"{EntityName} recebeu {danoFinal} dano m�gico");

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            Die();
        }
    }
    public void DefinirOpacidade(float alphaAlvo, float duracao = 0.25f)
    {
        // Se o personagem estiver morto, não mexemos na transparência dele
        if (CurrentState == BattleState.Dead) return;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Altera suavemente o Alpha do Sprite sem perder a cor original (RGB)
            sr.DOFade(alphaAlvo, duracao);
        }
    }
}