using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Linq;

public class MenuFocadoNoPlayer : MonoBehaviour
{
    private enum EstadoMenu { Principal, Skills, SelecaoAlvo }
    private EstadoMenu estadoAtual = EstadoMenu.Principal;

    [Header("Configurações de Posição")]
    public CharEntity playerAtual;
    public Vector3 offset;

    [Header("Botões (Devem ter RectTransform)")]
    public RectTransform botaoAtacar;
    public RectTransform botaoFugir;
    public RectTransform botaoItens;
    public RectTransform botaoEspecial;

    [Header("Referência da Seta de Alvo")]
    public SetaAlvoUI setaIndicadora;

    [Header("Habilidades")]
    public GameObject painelHabilidades;
    public Transform containerBotoesHabilidades;
    public GameObject prefabBotaoHabilidade;

    [Header("Configurações da Animação")]
    public float duracaoAnimacao = 0.25f;
    public Ease tipoTransicao = Ease.OutBack;
    public Ease tipoRecuo = Ease.OutQuad;

    [Header("Referências do Sistema")]
    public BattleManager battleManager;
    public BarraProgresso barraProgresso;

    private Camera cam;
    private RectTransform rectTransform;

    private List<BattleEntity> listaDeEntidades;
    private List<BattleEntity> inimigosVivos = new List<BattleEntity>();
    private int indiceAlvoAtual = 0;

    private List<SkillSO> habilidadesDoPlayer = new List<SkillSO>();
    private List<Button> botoesHabilidadeInstanciados = new List<Button>();
    private int indiceSkillFocadaGlobal = 0;
    private int indiceSkillTopoJanela = 0;
    private SkillSO habilidadeSelecionadaParaAtacar;

    private bool animando = false;
    private GameObject ultimoBotaoFocado;

    // Guardas de posições originais do menu aberto para o recuo perfeito
    private readonly Vector2 posOriginalAtacar = new Vector2(0, 50);
    private readonly Vector2 posOriginalFugir = new Vector2(0, -50);
    private readonly Vector2 posOriginalItens = new Vector2(-100, 0);
    private readonly Vector2 posOriginalEspecial = new Vector2(100, 0);

    void Awake()
    {
        cam = Camera.main;
        rectTransform = GetComponent<RectTransform>();

        botaoAtacar.GetComponent<Button>().onClick.AddListener(ClicouAtacar);
        botaoEspecial.GetComponent<Button>().onClick.AddListener(ClicouEspecial);

        if (setaIndicadora != null) setaIndicadora.Esconder();
        if (painelHabilidades != null) painelHabilidades.SetActive(false);
        EsconderMenu();
    }

    void Update()
    {
        if (estadoAtual == EstadoMenu.Principal && !animando)
        {
            GameObject focado = EventSystem.current.currentSelectedGameObject;
            if (focado != null && focado == botaoAtacar.gameObject && playerAtual != null && playerAtual.AtaqueBasico != null)
            {
                AtualizarPreviewDaSkill(playerAtual.AtaqueBasico);
            }
            else if (focado != null && focado != botaoEspecial.gameObject)
            {
                // Limpa o preview se estiver focado em Fugir ou Itens por exemplo
                LimparPreview();
            }
        }
    }

    public void FocarNoPlayer(CharEntity novoPlayer, List<BattleEntity> entidadesDaBatalha)
    {
        playerAtual = novoPlayer;
        listaDeEntidades = entidadesDaBatalha;
        estadoAtual = EstadoMenu.Principal;

        Vector3 posicaoTela = cam.WorldToScreenPoint(playerAtual.transform.position + offset);
        rectTransform.position = posicaoTela;

        gameObject.SetActive(true);
        animando = true;

        if (setaIndicadora != null) setaIndicadora.Esconder();
        if (painelHabilidades != null) painelHabilidades.SetActive(false);

        LimparPreview();
        AnimarEntradaBotoes();
    }

    #region LÓGICA DO MENU PRINCIPAL

    public void ClicouAtacar()
    {
        if (playerAtual == null || animando) return;

        habilidadeSelecionadaParaAtacar = playerAtual.AtaqueBasico;
        ultimoBotaoFocado = botaoAtacar.gameObject; // Salva que viemos do botão atacar

        animando = true;
        // AJUSTE 1: Contrair os botões também ao escolher o ataque básico
        AnimarRecuoBotoesPrincipal(() =>
        {
            animando = false;
            IniciarSelecaoDeAlvo();
        });
    }

    public void ClicouEspecial()
    {
        if (playerAtual == null || animando) return;

        habilidadesDoPlayer = playerAtual.Skills.ToList();

        if (habilidadesDoPlayer.Count == 0)
        {
            Debug.LogWarning("Este personagem não tem habilidades especiais!");
            return;
        }

        ultimoBotaoFocado = EventSystem.current.currentSelectedGameObject;
        estadoAtual = EstadoMenu.Skills;
        indiceSkillFocadaGlobal = 0;
        indiceSkillTopoJanela = 0;

        animando = true;
        AnimarRecuoBotoesPrincipal(() =>
        {
            animando = false;
            painelHabilidades.SetActive(true);
            ConstruirJanelaBotoesHabilidade();
            FocarHabilidadeVisualmente();
        });
    }

    private void IniciarSelecaoDeAlvo()
    {
        inimigosVivos = listaDeEntidades.Where(e => e is EnemyEntity && e.IsAlive).ToList();

        if (inimigosVivos.Count == 0)
        {
            // Se não houver inimigos, desfaz o recuo para não travar o menu
            AnimarRetornoBotoesPrincipal(null);
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);

        estadoAtual = EstadoMenu.SelecaoAlvo;
        indiceAlvoAtual = 0;
        setaIndicadora.MoverParaAlvo(inimigosVivos[indiceAlvoAtual]);

        if (habilidadeSelecionadaParaAtacar != null)
        {
            AtualizarPreviewDaSkill(habilidadeSelecionadaParaAtacar);
        }
    }

    #endregion

    #region PAGINAÇÃO DE HABILIDADES (JANELA DESLIZANTE)

    private void ConstruirJanelaBotoesHabilidade()
    {
        foreach (var b in botoesHabilidadeInstanciados) if (b != null) Destroy(b.gameObject);
        botoesHabilidadeInstanciados.Clear();

        int quantidadeParaRenderizar = Mathf.Min(3, habilidadesDoPlayer.Count);

        for (int i = 0; i < quantidadeParaRenderizar; i++)
        {
            int indexGlobalDaSkill = indiceSkillTopoJanela + i;
            if (indexGlobalDaSkill >= habilidadesDoPlayer.Count) break;

            SkillSO skill = habilidadesDoPlayer[indexGlobalDaSkill];

            GameObject btnGO = Instantiate(prefabBotaoHabilidade, containerBotoesHabilidades);
            BotaoHabilidadeUI botaoUI = btnGO.GetComponent<BotaoHabilidadeUI>();

            if (botaoUI != null)
            {
                botaoUI.Setup(skill);
                Button btnComponent = botaoUI.componenteBotao;

                if (btnComponent != null)
                {
                    Navigation nav = btnComponent.navigation;
                    nav.mode = Navigation.Mode.None;
                    btnComponent.navigation = nav;

                    int slotFisico = i;
                    btnComponent.onClick.AddListener(() => ClicouNoBotaoHabilidade(slotFisico));

                    botoesHabilidadeInstanciados.Add(btnComponent);
                }
            }
            else
            {
                Debug.LogError($"O prefabBotaoHabilidade não possui o script 'BotaoHabilidadeUI' anexado!");
            }
        }
    }

    private void FocarHabilidadeVisualmente()
    {
        if (botoesHabilidadeInstanciados.Count == 0) return;

        int slotFisicoParaFocar = indiceSkillFocadaGlobal - indiceSkillTopoJanela;

        if (slotFisicoParaFocar >= 0 && slotFisicoParaFocar < botoesHabilidadeInstanciados.Count)
        {
            botoesHabilidadeInstanciados[slotFisicoParaFocar].Select();
        }
    }

    private void NavegarHabilidades(int direcao)
    {
        int totalSkills = habilidadesDoPlayer.Count;
        if (totalSkills <= 1) return;

        indiceSkillFocadaGlobal += direcao;

        if (indiceSkillFocadaGlobal >= totalSkills)
        {
            indiceSkillFocadaGlobal = 0;
            indiceSkillTopoJanela = 0;
            ConstruirJanelaBotoesHabilidade();
        }
        else if (indiceSkillFocadaGlobal < 0)
        {
            indiceSkillFocadaGlobal = totalSkills - 1;
            indiceSkillTopoJanela = Mathf.Max(0, totalSkills - 3);
            ConstruirJanelaBotoesHabilidade();
        }
        else
        {
            if (indiceSkillFocadaGlobal >= indiceSkillTopoJanela + 3)
            {
                indiceSkillTopoJanela++;
                ConstruirJanelaBotoesHabilidade();
            }
            else if (indiceSkillFocadaGlobal < indiceSkillTopoJanela)
            {
                indiceSkillTopoJanela--;
                ConstruirJanelaBotoesHabilidade();
            }
        }

        FocarHabilidadeVisualmente();
    }

    private void ClicouNoBotaoHabilidade(int slotFisico)
    {
        int indexGlobal = indiceSkillTopoJanela + slotFisico;
        habilidadeSelecionadaParaAtacar = habilidadesDoPlayer[indexGlobal];

        painelHabilidades.SetActive(false);
        IniciarSelecaoDeAlvo();
    }

    #endregion

    #region INTERFACE DE COMUNICAÇÃO COM BOTÕES (PREVIEWS)

    public void ModernizarBotaoFocado(GameObject botao)
    {
        if (estadoAtual == EstadoMenu.Principal && botao != botaoAtacar.gameObject)
        {
            LimparPreview();
        }
    }

    public void AtualizarPreviewDaSkill(SkillSO skill)
    {
        if (barraProgresso == null || playerAtual == null || skill == null) return;

        int turnoBase = playerAtual.ReadyTurn;
        int turnoAcao = turnoBase + skill.turnosParaExecutar;
        int turnoRecuperacao = turnoAcao + skill.turnosRecuperacao;

        barraProgresso.MostrarPrevisaoTurno(playerAtual, turnoAcao, turnoRecuperacao, playerAtual.Icon);
    }

    public void LimparPreview()
    {
        if (barraProgresso != null)
        {
            barraProgresso.LimparPrevisaoTurno();
        }
    }

    #endregion

    #region EVENTOS DO NOVO INPUT SYSTEM

    public void OnInputConfirmar(InputAction.CallbackContext context)
    {
        if (animando || !context.performed) return;

        if (estadoAtual == EstadoMenu.SelecaoAlvo)
        {
            ConfirmarAtaqueNoAlvo();
            return;
        }

        if (estadoAtual == EstadoMenu.Skills)
        {
            int slotFisicoParaFocar = indiceSkillFocadaGlobal - indiceSkillTopoJanela;
            if (slotFisicoParaFocar >= 0 && slotFisicoParaFocar < botoesHabilidadeInstanciados.Count)
            {
                botoesHabilidadeInstanciados[slotFisicoParaFocar].onClick.Invoke();
            }
            return;
        }

        GameObject objetoFocado = EventSystem.current.currentSelectedGameObject;
        if (objetoFocado != null)
        {
            Button btn = objetoFocado.GetComponent<Button>();
            if (btn != null && btn.interactable) btn.onClick.Invoke();
        }
    }

    public void OnInputCancelar(InputAction.CallbackContext context)
    {
        if (animando || !context.performed) return;

        // AJUSTE 2: Tratamento correto ao cancelar a seleção de alvos
        if (estadoAtual == EstadoMenu.SelecaoAlvo)
        {
            setaIndicadora.Esconder();

            // Se a habilidade selecionada NÃO for o ataque básico, significa que viemos do menu de Especial
            if (habilidadeSelecionadaParaAtacar != playerAtual.AtaqueBasico)
            {
                estadoAtual = EstadoMenu.Skills;
                painelHabilidades.SetActive(true);

                // Reconstrói a janela para garantir que os botões existam fisicamente na UI
                ConstruirJanelaBotoesHabilidade();
                FocarHabilidadeVisualmente(); // Devolve o foco do ponteiro/EventSystem para a skill antiga
            }
            else
            {
                // Se era o ataque básico, expande o menu principal de volta e foca no botão Atacar
                animando = true;
                AnimarRetornoBotoesPrincipal(() =>
                {
                    estadoAtual = EstadoMenu.Principal;
                    EventSystem.current.SetSelectedGameObject(ultimoBotaoFocado);
                });
            }
            return;
        }

        if (estadoAtual == EstadoMenu.Skills)
        {
            animando = true;
            painelHabilidades.SetActive(false);
            LimparPreview();

            AnimarRetornoBotoesPrincipal(() =>
            {
                estadoAtual = EstadoMenu.Principal;
                EventSystem.current.SetSelectedGameObject(ultimoBotaoFocado);
            });
        }
    }

    public void OnInputNavegacao(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Vector2 direcao = context.ReadValue<Vector2>();

        if (estadoAtual == EstadoMenu.SelecaoAlvo)
        {
            if (direcao.y > 0 || direcao.x > 0)
            {
                indiceAlvoAtual = (indiceAlvoAtual + 1) % inimigosVivos.Count;
                setaIndicadora.MoverParaAlvo(inimigosVivos[indiceAlvoAtual]);
            }
            else if (direcao.y < 0 || direcao.x < 0)
            {
                indiceAlvoAtual = (indiceAlvoAtual - 1 + inimigosVivos.Count) % inimigosVivos.Count;
                setaIndicadora.MoverParaAlvo(inimigosVivos[indiceAlvoAtual]);
            }
            return;
        }

        if (estadoAtual == EstadoMenu.Skills)
        {
            if (direcao.y < 0) NavegarHabilidades(1);
            else if (direcao.y > 0) NavegarHabilidades(-1);
        }
    }

    #endregion

    private void ConfirmarAtaqueNoAlvo()
    {
        BattleEntity alvoEscolhido = inimigosVivos[indiceAlvoAtual];

        BattleDecision decisao = new BattleDecision();
        decisao.skill = habilidadeSelecionadaParaAtacar;
        decisao.targets = new BattleEntity[] { alvoEscolhido };

        setaIndicadora.Esconder();
        playerAtual.DefinirDecisao(decisao);
        EsconderMenu();
    }

    #region ANIMAÇÕES DE TRANSIÇÃO COM DOTWEEN

    private void AnimarEntradaBotoes()
    {
        botaoAtacar.anchoredPosition = Vector2.zero;
        botaoFugir.anchoredPosition = Vector2.zero;
        botaoItens.anchoredPosition = Vector2.zero;
        botaoEspecial.anchoredPosition = Vector2.zero;

        KillAllMenuTweens();

        Sequence seq = DOTween.Sequence();
        seq.Append(botaoAtacar.DOAnchorPos(posOriginalAtacar, duracaoAnimacao).SetEase(tipoTransicao));
        seq.Join(botaoFugir.DOAnchorPos(posOriginalFugir, duracaoAnimacao).SetEase(tipoTransicao));
        seq.Join(botaoItens.DOAnchorPos(posOriginalItens, duracaoAnimacao).SetEase(tipoTransicao));
        seq.Join(botaoEspecial.DOAnchorPos(posOriginalEspecial, duracaoAnimacao).SetEase(tipoTransicao));

        seq.OnComplete(() => {
            animando = false;
            Button btnAtacar = botaoAtacar.GetComponent<Button>();
            if (btnAtacar != null) btnAtacar.Select();
        });
    }

    private void AnimarRecuoBotoesPrincipal(System.Action onCompleteCallback)
    {
        KillAllMenuTweens();

        Sequence seq = DOTween.Sequence();
        seq.Append(botaoAtacar.DOAnchorPos(posOriginalAtacar * 0.5f, duracaoAnimacao).SetEase(tipoRecuo));
        seq.Join(botaoFugir.DOAnchorPos(posOriginalFugir * 0.5f, duracaoAnimacao).SetEase(tipoRecuo));
        seq.Join(botaoItens.DOAnchorPos(posOriginalItens * 0.5f, duracaoAnimacao).SetEase(tipoRecuo));
        seq.Join(botaoEspecial.DOAnchorPos(posOriginalEspecial * 0.5f, duracaoAnimacao).SetEase(tipoRecuo));

        seq.OnComplete(() => {
            onCompleteCallback?.Invoke();
        });
    }

    private void AnimarRetornoBotoesPrincipal(System.Action onCompleteCallback)
    {
        KillAllMenuTweens();

        Sequence seq = DOTween.Sequence();
        seq.Append(botaoAtacar.DOAnchorPos(posOriginalAtacar, duracaoAnimacao).SetEase(tipoTransicao));
        seq.Join(botaoFugir.DOAnchorPos(posOriginalFugir, duracaoAnimacao).SetEase(tipoTransicao));
        seq.Join(botaoItens.DOAnchorPos(posOriginalItens, duracaoAnimacao).SetEase(tipoTransicao));
        seq.Join(botaoEspecial.DOAnchorPos(posOriginalEspecial, duracaoAnimacao).SetEase(tipoTransicao));

        seq.OnComplete(() => {
            animando = false;
            onCompleteCallback?.Invoke();
        });
    }

    private void KillAllMenuTweens()
    {
        botaoAtacar.DOKill();
        botaoFugir.DOKill();
        botaoItens.DOKill();
        botaoEspecial.DOKill();
    }

    #endregion

    public void EsconderMenu()
    {
        animando = false;
        LimparPreview();
        if (painelHabilidades != null) painelHabilidades.SetActive(false);
        gameObject.SetActive(false);
    }
}