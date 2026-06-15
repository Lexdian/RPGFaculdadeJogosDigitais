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
    private enum EstadoMenu { Principal, Skills, Itens, SelecaoAlvo }
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

    [Header("Itens")]
    public GameObject painelItens;
    public Transform containerBotoesItens;
    public GameObject prefabBotaoItem;

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

    private List<ConsumableItemSO> itensDisponiveis = new List<ConsumableItemSO>();
    private List<Button> botoesItensInstanciados = new List<Button>();
    private int indiceItemFocadoGlobal = 0;
    private int indiceItemTopoJanela = 0;
    private ConsumableItemSO itemSelecionadoParaUsar;

    private bool animando = false;
    private GameObject ultimoBotaoFocado;

    private readonly Vector2 posOriginalAtacar = new Vector2(0, 100);
    private readonly Vector2 posOriginalFugir = new Vector2(0, -100);
    private readonly Vector2 posOriginalItens = new Vector2(-150, 0);
    private readonly Vector2 posOriginalEspecial = new Vector2(150, 0);

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip navigationSound;
    public AudioClip buttonCancelSound;
    public AudioClip buttonClickSound;

    void Awake()
    {
        botaoAtacar.GetComponent<Button>().onClick.AddListener(ClicouAtacar);
        botaoEspecial.GetComponent<Button>().onClick.AddListener(ClicouEspecial);
        botaoItens.GetComponent<Button>().onClick.AddListener(ClicouItens);
        audioSource = GetComponent<AudioSource>();

        if (gerenciadorSetas != null) gerenciadorSetas.LimparSetas();
        if (painelHabilidades != null) painelHabilidades.SetActive(false);
        if (painelItens != null) painelItens.SetActive(false);
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
            else if (focado != null)
            {
                LimparPreview();
            }
        }
        UpdateMenuPosition();
    }

    void UpdateMenuPosition()
    {
        Vector3 posicaoTela = cam.WorldToScreenPoint(playerAtual.transform.position + offset);
        rectTransform.position = posicaoTela;
    }

    public void FocarNoPlayer(CharEntity novoPlayer, List<BattleEntity> entidadesDaBatalha)
    {
        playerAtual = novoPlayer;
        listaDeEntidades = entidadesDaBatalha;
        estadoAtual = EstadoMenu.Principal;

        UpdateMenuPosition();

        gameObject.SetActive(true);
        animando = true;

        if (gerenciadorSetas != null) gerenciadorSetas.LimparSetas();
        if (painelHabilidades != null) painelHabilidades.SetActive(false);
        if (painelItens != null) painelItens.SetActive(false);

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
        playerAtual.SelecionandoAlvo();
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
        if(habilidadeSelecionadaParaAtacar != null && habilidadeSelecionadaParaAtacar.prioridade == Prioridade.Inimigos)
        {
            timeAlvoAtual = AlvoTime.Inimigos;
        }
        else
            timeAlvoAtual = AlvoTime.Aliados;
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

        audioSource.PlayOneShot(buttonClickSound);
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

        if(estadoAtual == EstadoMenu.Itens)
        {
            int slotFisicoParaFocar = indiceItemFocadoGlobal - indiceItemTopoJanela;
            if (slotFisicoParaFocar >= 0 && slotFisicoParaFocar < botoesItensInstanciados.Count)
            {
                botoesItensInstanciados[slotFisicoParaFocar].onClick.Invoke();
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

        audioSource.PlayOneShot(buttonCancelSound);

        if (estadoAtual == EstadoMenu.SelecaoAlvo)
        {
            if (gerenciadorSetas != null) gerenciadorSetas.LimparSetas();

            if (habilidadeSelecionadaParaAtacar == playerAtual.AtaqueBasico)
            {
                animando = true;
                AnimarRetornoBotoesPrincipal(() =>
                {
                    estadoAtual = EstadoMenu.Principal;
                    EventSystem.current.SetSelectedGameObject(ultimoBotaoFocado);
                });
            }
            else if(itemSelecionadoParaUsar != null)
            {
                estadoAtual = EstadoMenu.Itens;
                painelItens.SetActive(true);
                itemSelecionadoParaUsar = null;
                ConstruirJanelaBotoesItem();
                FocarItemVisualmente();
            }
            else
            {
                estadoAtual = EstadoMenu.Skills;
                painelHabilidades.SetActive(true);
                habilidadeSelecionadaParaAtacar = null;
                ConstruirJanelaBotoesHabilidade();
                FocarHabilidadeVisualmente();
            }
            playerAtual.SelecionandoAcao();
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

        if(estadoAtual == EstadoMenu.Itens)
        {
            animando = true;
            painelItens.SetActive(false);
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

        audioSource.PlayOneShot(navigationSound);

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

        if (estadoAtual == EstadoMenu.Itens)
        {
            if (direcao.y < 0) NavegarItens(1);
            else if (direcao.y > 0) NavegarItens(-1);
        }
    }

    #endregion

    private void ConfirmarAtaqueNoAlvo()
    {
        BattleDecision decisao = new BattleDecision();
        if (itemSelecionadoParaUsar != null)
        {
            habilidadeSelecionadaParaAtacar = SkillSO.CriarItem(itemSelecionadoParaUsar);
            GameManager.Instance.inventarioGrupo.TryRemove(itemSelecionadoParaUsar);
        }
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

        habilidadeSelecionadaParaAtacar = null;
        itemSelecionadoParaUsar = null;
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

    public void ClicouItens()
    {
        if (playerAtual == null || animando) return;

        if (GameManager.Instance != null && GameManager.Instance.inventarioGrupo != null)
        {
            itensDisponiveis = GameManager.Instance.inventarioGrupo.Slots
                .Where(slot => slot.item is ConsumableItemSO)
                .Select(slot => slot.item as ConsumableItemSO)
                .Where(item => item.podeUsarEmBatalha)
                .ToList();
        }
        else
        {
            itensDisponiveis = new List<ConsumableItemSO>();
        }

        if (itensDisponiveis.Count == 0)
        {
            Debug.LogWarning("Nenhum item consumível disponível para uso em batalha!");
            return;
        }

        ultimoBotaoFocado = EventSystem.current.currentSelectedGameObject;
        estadoAtual = EstadoMenu.Itens;
        indiceItemFocadoGlobal = 0;
        indiceItemTopoJanela = 0;

        animando = true;
        AnimarRecuoBotoesPrincipal(() =>
        {
            animando = false;
            painelItens.SetActive(true);
            ConstruirJanelaBotoesItem();
            FocarItemVisualmente();
        });
    }

    // 3. Atualize o construtor da janela para passar o item correto para o botão:
    private void ConstruirJanelaBotoesItem()
    {
        foreach (var b in botoesItensInstanciados) if (b != null) Destroy(b.gameObject);
        botoesItensInstanciados.Clear();

        int quantidadeParaRenderizar = Mathf.Min(3, itensDisponiveis.Count);

        for (int i = 0; i < quantidadeParaRenderizar; i++)
        {
            int indexGlobalDoItem = indiceItemTopoJanela + i;
            if (indexGlobalDoItem >= itensDisponiveis.Count) break;

            ConsumableItemSO item = itensDisponiveis[indexGlobalDoItem];

            GameObject btnGO = Instantiate(prefabBotaoItem, containerBotoesItens);
            BotaoItemUI botaoUI = btnGO.GetComponent<BotaoItemUI>();

            if (botaoUI != null)
            {
                int quantidade = GameManager.Instance.inventarioGrupo.Slots
                    .Where(slot => slot.item == item)
                    .Sum(slot => slot.quantity);
                botaoUI.Setup(item, quantidade); // Passa o ConsumableItemSO e a quantidade
                Button btnComponent = botaoUI.componenteBotao;

                if (btnComponent != null)
                {
                    Navigation nav = btnComponent.navigation;
                    nav.mode = Navigation.Mode.None;
                    btnComponent.navigation = nav;

                    int slotFisico = i;
                    btnComponent.onClick.AddListener(() => ClicouNoBotaoItem(slotFisico));

                    botoesItensInstanciados.Add(btnComponent);
                }
            }
        }
    }

    private void NavegarItens(int direcao)
    {
        int totalItems = itensDisponiveis.Count;
        if (totalItems <= 1) return;

        indiceItemFocadoGlobal += direcao;

        if (indiceItemFocadoGlobal >= totalItems)
        {
            indiceItemFocadoGlobal = 0;
            indiceItemTopoJanela = 0;
            ConstruirJanelaBotoesItem();
        }
        else if (indiceItemFocadoGlobal < 0)
        {
            indiceItemFocadoGlobal = totalItems - 1;
            indiceItemTopoJanela = Mathf.Max(0, totalItems - 3);
            ConstruirJanelaBotoesItem();
        }
        else
        {
            if (indiceItemFocadoGlobal >= indiceItemTopoJanela + 3)
            {
                indiceItemTopoJanela++;
                ConstruirJanelaBotoesItem();
            }
            else if (indiceItemFocadoGlobal < indiceItemTopoJanela)
            {
                indiceItemTopoJanela--;
                ConstruirJanelaBotoesItem();
            }
        }

        FocarItemVisualmente();
    }

    private void FocarItemVisualmente()
    {
        if (botoesItensInstanciados.Count == 0) return;

        int slotFisicoParaFocar = indiceItemFocadoGlobal - indiceItemTopoJanela;
        if (slotFisicoParaFocar >= 0 && slotFisicoParaFocar < botoesItensInstanciados.Count)
        {
            botoesItensInstanciados[slotFisicoParaFocar].Select();
        }
    }

    private void ClicouNoBotaoItem(int slotFisico)
    {
        int indexGlobal = indiceItemTopoJanela + slotFisico;
        itemSelecionadoParaUsar = itensDisponiveis[indexGlobal];

        painelItens.SetActive(false);
        IniciarSelecaoDeAlvo();
    }

    public void AtualizarPreviewDoItem(ConsumableItemSO item)
    {
        if (barraProgresso == null || playerAtual == null || item == null) return;

        int turnoBase = playerAtual.ReadyTurn;
        int turnoAcao = turnoBase + 1;
        int turnoRecuperacao = turnoAcao+1;

        barraProgresso.MostrarPrevisaoTurno(playerAtual, turnoAcao, turnoRecuperacao, playerAtual.Data.fichaBase.charPortrait);
    }
}