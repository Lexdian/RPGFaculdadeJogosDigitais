using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Linq;
using System;

public class MenuFocadoNoPlayer : MonoBehaviour
{
    private enum EstadoMenu { Principal, Skills, SelecaoAlvo }
    private EstadoMenu estadoAtual = EstadoMenu.Principal;

    // NOVO: Controle de qual grupo de entidades estamos mirando no momento
    private enum AlvoTime { Inimigos, Aliados }
    private AlvoTime timeAlvoAtual = AlvoTime.Inimigos;

    [Header("Configurações de Posição")]
    public CharEntity playerAtual;
    public Vector3 offset;

    [Header("Botões (Devem ter RectTransform)")]
    public RectTransform botaoAtacar;
    public RectTransform botaoFugir;
    public RectTransform botaoItens;
    public RectTransform botaoEspecial;

    [Header("Referência do Gerenciador de Setas")]
    public GerenciadorSetasUI gerenciadorSetas;

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

    [SerializeField] private Camera cam;
    [SerializeField] private RectTransform rectTransform;

    private List<BattleEntity> listaDeEntidades;

    // VARIÁVEIS MODIFICADAS: Agora controlamos listas separadas de entidades vivas
    private List<BattleEntity> inimigosVivos = new List<BattleEntity>();
    private List<BattleEntity> aliadosVivos = new List<BattleEntity>();
    private int indiceAlvoAtual = 0;

    private List<SkillSO> habilidadesDoPlayer = new List<SkillSO>();
    private List<Button> botoesHabilidadeInstanciados = new List<Button>();
    private int indiceSkillFocadaGlobal = 0;
    private int indiceSkillTopoJanela = 0;
    private SkillSO habilidadeSelecionadaParaAtacar;

    private bool animando = false;
    private GameObject ultimoBotaoFocado;

    private readonly Vector2 posOriginalAtacar = new Vector2(0, 50);
    private readonly Vector2 posOriginalFugir = new Vector2(0, -50);
    private readonly Vector2 posOriginalItens = new Vector2(-100, 0);
    private readonly Vector2 posOriginalEspecial = new Vector2(100, 0);

    void Awake()
    {
        botaoAtacar.GetComponent<Button>().onClick.AddListener(ClicouAtacar);
        botaoEspecial.GetComponent<Button>().onClick.AddListener(ClicouEspecial);

        if (gerenciadorSetas != null) gerenciadorSetas.LimparSetas();
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

        if (gerenciadorSetas != null) gerenciadorSetas.LimparSetas();
        if (painelHabilidades != null) painelHabilidades.SetActive(false);

        LimparPreview();
        AnimarEntradaBotoes();
    }

    #region LÓGICA DO MENU PRINCIPAL

    public void ClicouAtacar()
    {
        if (playerAtual == null || animando) return;

        habilidadeSelecionadaParaAtacar = playerAtual.AtaqueBasico;
        ultimoBotaoFocado = botaoAtacar.gameObject;

        animando = true;
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
        // Separa dinamicamente quem são os inimigos e quem são os aliados (vivos)
        inimigosVivos = listaDeEntidades.Where(e => e is EnemyEntity && e.IsAlive).ToList();
        aliadosVivos = listaDeEntidades.Where(e => e is CharEntity && e.IsAlive).ToList();

        if (inimigosVivos.Count == 0 && aliadosVivos.Count == 0)
        {
            AnimarRetornoBotoesPrincipal(null);
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        estadoAtual = EstadoMenu.SelecaoAlvo;

        // MODIFICADO: Reseta para mirar nos inimigos por padrão ao abrir a seleção
        timeAlvoAtual = AlvoTime.Inimigos;
        indiceAlvoAtual = 0;

        AtualizarSetasDeAlvo();

        if (habilidadeSelecionadaParaAtacar != null)
        {
            AtualizarPreviewDaSkill(habilidadeSelecionadaParaAtacar);
        }
    }

    private void AtualizarSetasDeAlvo()
    {
        if (gerenciadorSetas == null) return;

        // Pega a lista correta baseado no time que estamos focando atualmente
        List<BattleEntity> listaFocada = (timeAlvoAtual == AlvoTime.Inimigos) ? inimigosVivos : aliadosVivos;

        if (listaFocada.Count == 0) return;

        // Garante que o índice não estoure caso a lista de aliados seja menor que a de inimigos ao trocar
        indiceAlvoAtual = Mathf.Clamp(indiceAlvoAtual, 0, listaFocada.Count - 1);

        if (habilidadeSelecionadaParaAtacar != null && habilidadeSelecionadaParaAtacar.alvo == TipoAlvo.Grupo)
        {
            // Se for em área, coloca a seta em todos do time selecionado
            gerenciadorSetas.MostrarSetasNosAlvos(listaFocada);
        }
        else
        {
            // Se for alvo único, coloca apenas no índice atual do time selecionado
            List<BattleEntity> alvoUnico = new List<BattleEntity> { listaFocada[indiceAlvoAtual] };
            gerenciadorSetas.MostrarSetasNosAlvos(alvoUnico);
        }
    }

    #endregion

    #region PAGINAÇÃO DE HABILIDADES

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

    #region INTERFACE DE COMUNICAÇÃO COM BOTÕES

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

    // NOVO MÉTODO: Vinculado à sua nova Action "Trocar" no Input System
    public void OnInputTrocarAlvoTime(InputAction.CallbackContext context)
    {
        // Só executa se o botão for pressionado e estivermos escolhendo alvos
        if (!context.performed || estadoAtual != EstadoMenu.SelecaoAlvo || animando) return;

        // Inverte o time focado
        if (timeAlvoAtual == AlvoTime.Inimigos)
        {
            // Só troca para aliados se houver algum aliado vivo na lista
            if (aliadosVivos.Count > 0) timeAlvoAtual = AlvoTime.Aliados;
        }
        else
        {
            if (inimigosVivos.Count > 0) timeAlvoAtual = AlvoTime.Inimigos;
        }

        // Reseta o índice para o primeiro membro do novo grupo e atualiza o visual
        indiceAlvoAtual = 0;
        AtualizarSetasDeAlvo();
    }

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

        if (estadoAtual == EstadoMenu.SelecaoAlvo)
        {
            if (gerenciadorSetas != null) gerenciadorSetas.LimparSetas();

            if (habilidadeSelecionadaParaAtacar != playerAtual.AtaqueBasico)
            {
                estadoAtual = EstadoMenu.Skills;
                painelHabilidades.SetActive(true);
                ConstruirJanelaBotoesHabilidade();
                FocarHabilidadeVisualmente();
            }
            else
            {
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
            if (habilidadeSelecionadaParaAtacar != null && habilidadeSelecionadaParaAtacar.alvo == TipoAlvo.Grupo)
                return;

            // MODIFICADO: A navegação agora descobre o tamanho da lista focada no momento
            List<BattleEntity> listaFocada = (timeAlvoAtual == AlvoTime.Inimigos) ? inimigosVivos : aliadosVivos;
            if (listaFocada.Count <= 1) return;

            if (direcao.y < 0 || direcao.x > 0)
            {
                indiceAlvoAtual = (indiceAlvoAtual + 1) % listaFocada.Count;
                AtualizarSetasDeAlvo();
            }
            else if (direcao.y > 0 || direcao.x < 0)
            {
                indiceAlvoAtual = (indiceAlvoAtual - 1 + listaFocada.Count) % listaFocada.Count;
                AtualizarSetasDeAlvo();
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
        BattleDecision decisao = new BattleDecision();
        decisao.skill = habilidadeSelecionadaParaAtacar;

        // MODIFICADO: Pega o grupo ativo no momento da confirmação
        List<BattleEntity> listaFocada = (timeAlvoAtual == AlvoTime.Inimigos) ? inimigosVivos : aliadosVivos;

        if (habilidadeSelecionadaParaAtacar != null && habilidadeSelecionadaParaAtacar.alvo == TipoAlvo.Grupo)
        {
            // Retorna o grupo inteiro focado (todos os inimigos OU todos os aliados)
            decisao.targets = listaFocada.ToArray();
        }
        else
        {
            // Retorna apenas o indivíduo selecionado dentro do grupo atual
            BattleEntity alvoEscolhido = listaFocada[indiceAlvoAtual];
            decisao.targets = new BattleEntity[] { alvoEscolhido };
        }

        if (gerenciadorSetas != null) gerenciadorSetas.LimparSetas();

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
        if (gerenciadorSetas != null) gerenciadorSetas.LimparSetas();
        if (painelHabilidades != null) painelHabilidades.SetActive(false);
        gameObject.SetActive(false);
    }
}