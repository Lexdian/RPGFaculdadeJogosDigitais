using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BattleManager : MonoBehaviour
{
    [Header("Prefabs e Visuais")]
    public GameObject enemyPrefab;
    public GameObject allyPrefab;
    public Sprite shadowSprite;

    [Header("Configura��es dos Inimigos (Esquerda)")]
    [SerializeField] private Vector3 centroChaoInimigos = new Vector3(-4f, -1.5f, 0);
    public float distanciaEntreColunasInimigos = 2.0f;
    public float alturaTotalColunaInimigos = 4.0f;
    public float offsetVoadorY = 1.5f;

    [Header("Configura��es dos Aliados (Direita)")]
    [SerializeField] private Vector3 centroChaoAliados = new Vector3(4f, -1.5f, 0);
    public float alturaTotalColunaAliados = 4.0f;

    private List<ActionData> actionQueue = new List<ActionData>();
    private int currentTurn = 0;

    private List<EnemyEntity> enemies = new();
    private List<CharEntity> allies = new();

    public List<EnemyEntity> Enemies => enemies;
    public List<CharEntity> Allies => allies;

    public Dictionary<BattleEntity, CharInfos> CharInfosMap = new Dictionary<BattleEntity, CharInfos>();

    [Header("Refer�ncias de UI")]
    public CanvasGroup CanvasGroup;
    public MenuFocadoNoPlayer menuUI;
    public BarraProgresso timelineUI;
    public GameObject charInfoPrefab;
    public RectTransform charInfosContainer;
    public CaixaMensagem caixaMensagem;

    public Texture2D texturaTeste; // Apenas para teste rápido de aplicação de textura no vidro, pode ser removida depois

    [Header("Tela de Resultado")]
    [SerializeField] private BattleResultUI telaResultado;

    [Header("Referência do Cubo 3D")]
    public GlassShatter cuboMaterialAlvo;

    private bool batalhaEncerrada = false;

    public AudioClip battleMusic;

    void Awake()
    {
        CanvasGroup.alpha = 0;
        CanvasGroup.interactable = false;
        if (GameManager.Instance != null)
            GameManager.Instance.emCombate = true;
        SpawnEnemies();
        SpawnAllies();
        AplicarTexturaNoCubo(GameManager.Instance != null ? GameManager.Instance.lastTexture : texturaTeste);
    }

    // Mudamos de 'void Start' para 'IEnumerator Start' 
    // O Unity entende isso nativamente e sabe como rodar como Coroutine
    IEnumerator Start()
    {
        AudioManager.Instance.PlayMusicWithFade(battleMusic, fadeDuration: 2.0f, targetVolume: 0.1f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(charInfosContainer);
        charInfosContainer.GetComponent<VerticalLayoutGroup>().enabled = false;
        if (cuboMaterialAlvo != null)
        {
            Debug.Log("Iniciando efeito de estilhaçamento do vidro...");
            yield return StartCoroutine(cuboMaterialAlvo.ShatterCoroutine(cuboMaterialAlvo.transform.position));
        }
        CanvasGroup.alpha = 1;
        CanvasGroup.interactable = true;
        // Agora sim! O loop da batalha só começa DEPOIS que o vidro quebrou e sumiu.
        StartCoroutine(BattleLoop());
    }

#if UNITY_EDITOR
    // ─── ATALHO DE TESTE TEMPORÁRIO ─────────────────────────────────────────────
    // F1 = abre a tela de Vitória / F2 = abre a tela de Derrota, sem precisar
    // terminar a batalha de verdade. Só existe em builds do Editor.
    // REMOVER depois de validar as telas de resultado.
    void Update()
    {
        if (Keyboard.current == null || batalhaEncerrada) return;

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            batalhaEncerrada = true;
            StartCoroutine(MostrarResultadoCoroutine(vitoria: true));
        }
        else if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            batalhaEncerrada = true;
            StartCoroutine(MostrarResultadoCoroutine(vitoria: false));
        }
    }
#endif

    #region SPAWN DOS INIMIGOS
    void SpawnEnemies()
    {
        if (GameManager.Instance == null) return;

        OrganizarColunaInimigos(GameManager.Instance.inimigosParaSpawnar.colunaFrente, 0);
        OrganizarColunaInimigos(GameManager.Instance.inimigosParaSpawnar.colunaMeio, 1);
        OrganizarColunaInimigos(GameManager.Instance.inimigosParaSpawnar.colunaTras, 2);
    }

    void OrganizarColunaInimigos(EnemySO[] inimigos, int indexColuna)
    {
        if (inimigos == null) return;

        var lista = inimigos.Where(e => e != null).ToList();
        int total = lista.Count;
        if (total == 0) return;

        float posX = centroChaoInimigos.x - (indexColuna * distanciaEntreColunasInimigos);

        for (int i = 0; i < total; i++)
        {
            EnemySO data = lista[i];

            float stepY = alturaTotalColunaInimigos / (total + 1);
            float posY = centroChaoInimigos.y + (alturaTotalColunaInimigos / 2) - (stepY * (i + 1));

            Vector3 posicaoFinal = new Vector3(posX, posY, 0);

            if (data.isVoador)
            {
                InstanciarSombra(posicaoFinal);
                posicaoFinal.y += offsetVoadorY;
            }

            InstanciarInimigo(data, posicaoFinal, -total + i);
        }
    }

    void InstanciarInimigo(EnemySO data, Vector3 posicao, int order)
    {
        GameObject go = Instantiate(enemyPrefab, posicao, Quaternion.identity);

        var sr = go.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.sprite = data.enemySprite;
            sr.sortingOrder = order;
        }

        EnemyEntity entity = go.AddComponent<EnemyEntity>();
        entity.Setup(data);

        enemies.Add(entity);

        go.name = data.enemyName;
    }
    #endregion

    #region SPAWN DOS ALIADOS
    void SpawnAllies()
    {
        if (GameManager.Instance == null || GameManager.Instance.equipeAtual == null) return;

        var equipe = GameManager.Instance.equipeAtual;
        int totalAliados = equipe.Count;
        if (totalAliados == 0) return;

        for (int i = 0; i < totalAliados; i++)
        {
            CombatenteData dadosAliado = equipe[i];
            if (dadosAliado == null || dadosAliado.fichaBase == null) continue;

            float stepY = alturaTotalColunaAliados / (totalAliados + 1);
            float posY = centroChaoAliados.y + (alturaTotalColunaAliados / 2) - (stepY * (i + 1));

            Vector3 posicaoFinal = new Vector3(centroChaoAliados.x, posY, 0);

            Debug.Log($"Ordem final para aliado {-totalAliados + i}");

            InstanciarAliado(dadosAliado, posicaoFinal, -totalAliados + i);
        }
    }

    void InstanciarAliado(CombatenteData dados, Vector3 posicao, int order)
    {
        GameObject go = Instantiate(allyPrefab, posicao, Quaternion.identity);

        var sr = go.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.sprite = dados.fichaBase.charBattle;
            sr.sortingOrder = order;
            sr.flipX = false;
        }

        CharEntity entity = go.GetComponent<CharEntity>();
        entity.Setup(dados);

        InstantiateCharInfoUI(dados, entity);

        allies.Add(entity);

        go.name = dados.fichaBase.charName;
    }
    #endregion

    #region Instantiate CharInfos
    private void InstantiateCharInfoUI(CombatenteData data, BattleEntity entity)
    {
        GameObject infoGO = Instantiate(charInfoPrefab, charInfosContainer);
        CharInfos charInfos = infoGO.GetComponent<CharInfos>();
        if (charInfos != null)
        {
            charInfos.Sesup(data);
            CharInfosMap.Add(entity, charInfos);
        }
    }
    #endregion
    #region Pipeline de Batalha

    IEnumerator BattleLoop()
    {
        while (!batalhaEncerrada)
        {
            yield return StartCoroutine(MainPipelineCoroutine());
        }
    }

    public IEnumerator MainPipelineCoroutine()
    {
        Debug.Log($"=========================TURNO {currentTurn}=========================");

        yield return StartCoroutine(TickStatusEfeitosCoroutine());

        CheckBattleEnd();
        if (batalhaEncerrada) yield break;

        yield return StartCoroutine(ExecuteActionsCoroutine());

        if (batalhaEncerrada) yield break;

        UpdateRecovery();

        yield return StartCoroutine(AskForActionsCoroutine());

        bool carregouSegmento = false;
        timelineUI.AtualizarProgressoTurno(() => carregouSegmento = true);
        yield return new WaitUntil(() => carregouSegmento);

        currentTurn++;
    }
    private IEnumerator TickStatusEfeitosCoroutine()
    {
        List<BattleEntity> todasEntidades = GetAllEntities();

        foreach (var entity in todasEntidades)
        {
            if (!entity.IsAlive || entity.statusAtivos.Count == 0) continue;

            string nomes = string.Join(", ", entity.statusAtivos.ConvertAll(s => s.status.effectName));
            Debug.Log(" Nomes:" + nomes);
            StartCoroutine(caixaMensagem.ExibirMensagem($"{entity.EntityName} sofre: {nomes}!"));

            entity.TickAllStatus();

            if (CharInfosMap.ContainsKey(entity))
            {
                yield return StartCoroutine(CharInfosMap[entity].UpdateInfos((CharEntity)entity));
            }
            else
            {
                yield return new WaitForSeconds(0.6f);
            }

            if (!entity.IsAlive)
            {
                StartCoroutine(caixaMensagem.ExibirMensagem($"{entity.EntityName} foi derrotado pelo efeito de status!"));
                yield return new WaitForSeconds(0.5f);
                CheckBattleEnd();
            }
        }
    }

    IEnumerator ExecuteActionsCoroutine()
    {
        List<ActionData> actionsThisTurn =
            actionQueue
            .Where(a => a.turnoExecucao == currentTurn)
            .OrderByDescending(a => a.executor.Velocidade)
            .ToList();

        foreach (var action in actionsThisTurn)
        {
            // Executa a a��o do personagem atual e aguarda os efeitos/anima��es terminarem
            yield return StartCoroutine(ExecuteActionCoroutine(action));

            foreach (BattleEntity b in action.alvo)
            {
                if (!b.IsAlive)
                {
                    EntityDied(b);
                }
            }
            if (!action.executor.IsAlive)
                EntityDied(action.executor);
            actionQueue.Remove(action);

            CheckBattleEnd();
            if (batalhaEncerrada) yield break;

        }
    }

    IEnumerator ExecuteActionCoroutine(ActionData action)
    {
        if (!action.executor.IsAlive) yield break;

        // 1. Inicia o efeito visual de piscar em branco (In�cio do Ataque)
        SpriteRenderer srExecutor = action.executor.GetComponent<SpriteRenderer>();
        Coroutine piscarCoroutine = null;
        if (srExecutor != null)
        {
            piscarCoroutine = StartCoroutine(FlashWhiteCoroutine(srExecutor, 0.4f));
            yield return new WaitForSeconds(0.2f); // Aguarda um pouco para o efeito de piscar ser perceptível antes de aplicar a lógica
        }

        if (action.executor.CurrentMP < action.habilidade.custoMana)
        {
            timelineUI.RemoverInconeAcao(action.executor);
            StartCoroutine(caixaMensagem.ExibirMensagem($"{action.executor.EntityName} tentou usar {action.habilidade.skillName} mas não tinha mana suficiente!"));
        }
        else {
            action.executor.CurrentMP -= action.habilidade.custoMana;
            BattleEntity[] alvosFinais = action.alvo;
            if (!action.habilidade.podeSerUsadaEmMortos)
            {
                alvosFinais = alvosFinais.Where(e => e.IsAlive).ToArray();
                if (alvosFinais.Length == 0)
                {
                    alvosFinais = new BattleEntity[] { GetAllEntities().Where(e => e.IsAlive).First() };
                }
            }

            StartCoroutine(caixaMensagem.ExibirMensagem($"{action.executor.EntityName} usou {action.habilidade.skillName}!"));

            yield return StartCoroutine(BattleAnimationManager.Instance.ExecutarAnimacaoMagia(action.habilidade, action.executor, alvosFinais));

            timelineUI.RemoverInconeAcao(action.executor);

            yield return new WaitForSeconds(0.2f);

            // 3. Aplica a l�gica de combate nos alvos
            foreach (var alvo in alvosFinais)
            {
                alvo.ReceiveAction(action.executor, action.habilidade);
            }

            if (CharInfosMap.ContainsKey(action.executor))
            {
                yield return StartCoroutine(CharInfosMap[action.executor].UpdateInfos((CharEntity)action.executor));
            }
            for (int i = 0; i < alvosFinais.Length; i++)
            {
                if (CharInfosMap.ContainsKey(alvosFinais[i]))
                {
                    yield return StartCoroutine(CharInfosMap[alvosFinais[i]].UpdateInfos((CharEntity)alvosFinais[i]));
                }
            }
        }
        action.executor.CurrentState = BattleState.Resting;
        action.executor.ReadyTurn = currentTurn + action.turnoRecuperacao;

        // Se a corotina de piscar ainda estiver rodando, garante que ela encerre e limpe o Material
        if (piscarCoroutine != null) StopCoroutine(piscarCoroutine);
        if (srExecutor != null) srExecutor.color = Color.white;

        yield return new WaitForSeconds(1.0f);
    }

    // MODIFICADO: Transformado em IEnumerator para conseguir dar yield na anima��o de reset da barra
    IEnumerator QueueActionCoroutine(BattleEntity executor, BattleEntity[] alvo, SkillSO habilidade)
    {
        int turnoDeExecucao = currentTurn + habilidade.turnosParaExecutar;

        ActionData action = new ActionData
        {
            executor = executor,
            alvo = alvo,
            habilidade = habilidade,
            turnoExecucao = turnoDeExecucao,
            turnoRecuperacao = habilidade.turnosRecuperacao
        };

        actionQueue.Add(action);
        executor.CurrentState = BattleState.Preparing;

        int turnoRecuperacaoFinal = turnoDeExecucao + habilidade.turnosRecuperacao;

        if (turnoRecuperacaoFinal >= timelineUI.GetPrimeiroTurno() + 7 || currentTurn+1 < timelineUI.GetPrimeiroTurno())
        {
            // Dispara o ZerarBarra e aguarda o t�rmino do Tween suave do DOTween antes de prosseguir
            var barraTween = timelineUI.ZerarBarra(currentTurn+1);
            if (barraTween != null)
            {
                yield return barraTween.WaitForCompletion();
            }
        }

        // Adiciona os �cones que agora se mover�o suavemente
        timelineUI.AdicionarOuMoverIconeDuplo(executor, turnoDeExecucao, turnoRecuperacaoFinal, executor.Icon);

        Debug.Log($"{executor.EntityName} come�ou preparar {habilidade.skillName}");
    }

    void UpdateRecovery()
    {
        foreach (var entity in GetAllEntities())
        {
            if (!entity.IsAlive)
                continue;

            if (entity.CurrentState == BattleState.Resting &&
                currentTurn >= entity.ReadyTurn)
            {
                entity.CurrentState = BattleState.WaitingAction;

                Debug.Log($"{entity.EntityName} pode agir novamente");
                timelineUI.RemoverInconeEscolha(entity);
            }
        }
    }

    IEnumerator AskForActionsCoroutine()
    {
        foreach (var entity in Enemies)
        {
            if (!entity.IsAlive || entity.CurrentState != BattleState.WaitingAction) continue;

            if (entity.HasStatusEffect<AtordoamentoStatusSO>())
            {
                StartCoroutine(caixaMensagem.ExibirMensagem($"{entity.EntityName} está atordoado e não pode agir!"));
                yield return new WaitForSeconds(0.8f);
                continue;
            }

            BattleDecision decision = ((EnemyEntity)entity).GetAction(GetAllEntities());
            if (decision.skill != null)
                // MODIFICADO: Espera o enfileiramento e poss�veis anima��es de transi��o suave terminar
                yield return StartCoroutine(QueueActionCoroutine(entity, decision.targets, decision.skill));
        }

        foreach (var entity in Allies)
        {
            if (!entity.IsAlive || entity.CurrentState != BattleState.WaitingAction) continue;

            CharEntity playerChar = (CharEntity)entity;

            playerChar.EscoolherAcaoDoPlayer(GetAllEntities(), menuUI);
            CharInfosMap[entity].MoverParaEsquerda();

            bool primeiraTransicaoFeita = false;

            while (!playerChar.DecididoNoTurno)
            {
                // ====================================================================
                // SE O JOGADOR ESTÁ NA TELA DE AÇÃO: ZOOM NO HERÓI
                // ====================================================================
                if (!playerChar.TemAcaoSelecionada)
                {
                    if (playerChar.MinhaCamera != null && playerChar.MinhaCamera.Priority != 11)
                    {
                        playerChar.MinhaCamera.Priority = 11;

                        // NOVO: Foca visualmente no herói atual e esmaece os outros
                        AplicarEfeitoTransparencia(playerChar);

                        if (!primeiraTransicaoFeita)
                        {
                            yield return new WaitForSeconds(0.8f);
                            primeiraTransicaoFeita = true;
                        }
                    }

                    yield return new WaitUntil(() => playerChar.TemAcaoSelecionada || playerChar.DecididoNoTurno);
                }

                // ====================================================================
                // SE O JOGADOR AVANÇOU PARA OS ALVOS: CÂMERA AFASTADA
                // ====================================================================
                if (playerChar.TemAcaoSelecionada && !playerChar.DecididoNoTurno)
                {
                    if (playerChar.MinhaCamera != null && playerChar.MinhaCamera.Priority != 1)
                    {
                        playerChar.MinhaCamera.Priority = 1;

                        // NOVO: Como a câmera se afastou para o campo, todos voltam a ficar 100% visíveis
                        ResetarTransparenciaDeTodos();
                    }

                    yield return new WaitUntil(() => !playerChar.TemAcaoSelecionada || playerChar.DecididoNoTurno);
                }
            }

            // ====================================================================
            // FINALIZAÇÃO DO TURNO
            // ====================================================================
            if (playerChar.MinhaCamera != null) playerChar.MinhaCamera.Priority = 1;

            // NOVO: Garante o reset de visibilidade caso ele confirme a ação direto do menu
            ResetarTransparenciaDeTodos();

            BattleDecision decision = playerChar.ObterDecisaoFinal();
            if (decision.skill != null)
            {
                yield return StartCoroutine(QueueActionCoroutine(entity, decision.targets, decision.skill));
            }

            CharInfosMap[entity].VoltarParaPosicaoInicial();
        }
    }

    public void CheckBattleEnd()
    {
        if (batalhaEncerrada) return;

        bool allEnemiesDead = enemies.All(e => !e.IsAlive);
        bool allAlliesDead = allies.All(a => !a.IsAlive);
        if (allEnemiesDead)
        {
            batalhaEncerrada = true;
            if (GameManager.Instance != null)
                GameManager.Instance.emCombate = false;
            StartCoroutine(MostrarResultadoCoroutine(vitoria: true));
        }
        else if (allAlliesDead)
        {
            batalhaEncerrada = true;
            if (GameManager.Instance != null)
                GameManager.Instance.emCombate = false;
            StartCoroutine(MostrarResultadoCoroutine(vitoria: false));
        }
    }

    private IEnumerator MostrarResultadoCoroutine(bool vitoria)
    {
        yield return new WaitForSeconds(1.5f);



        if (telaResultado == null)
        {
            Debug.LogError("[BattleManager] Referência 'Tela Resultado' não configurada no Inspector!");
            yield break;
        }

        if (vitoria)
        {
            int xpTotal = CalcularXPTotal();
            StartCoroutine(caixaMensagem.ExibirMensagem("Vitória!"));
            yield return new WaitForSeconds(0.5f);
            CombatenteData[] dadosPersonagens = Allies.Select(a => a.Data).ToArray();
            telaResultado.MostrarVitoria(xpTotal, dadosPersonagens);
            int aliadosVivos = Allies.Where(c => c.IsAlive).Count();
            int xpCada = xpTotal / aliadosVivos;
            foreach (CharEntity c in Allies)
            {
                if (c.IsAlive)
                    c.EndUpdate(xpCada);
            }
        }
        else
        {
            StartCoroutine(caixaMensagem.ExibirMensagem("Derrota..."));
            yield return new WaitForSeconds(0.5f);
            telaResultado.MostrarDerrota();
        }
    }

    private int CalcularXPTotal()
    {
        int total = 0;
        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive)
                total += enemy.Data.xpReward;
        }
        return total;
    }


    #endregion

    #region UTILIT�RIOS
    void InstanciarSombra(Vector3 posicaoChao)
    {
        GameObject sombra = new GameObject("Sombra");
        sombra.transform.position = posicaoChao;
        var sr = sombra.AddComponent<SpriteRenderer>();
        sr.sprite = shadowSprite;
        sr.color = new Color(0, 0, 0, 0.4f);
        sr.sortingOrder = 9;
    }

    public List<BattleEntity> GetAllEntities()
    {
        List<BattleEntity> all = new();

        all.AddRange(allies);
        all.AddRange(enemies);

        return all;
    }

    IEnumerator FlashWhiteCoroutine(SpriteRenderer sr, float duracaoTotal)
    {
        float tempoMapeado = 0f;
        Color corOriginal = Color.white;
        // Tom avermelhado/branco brilhante usando a propriedade de colora��o nativa do SpriteRenderer
        Color corBrancaBrilhante = new Color(5f, 5f, 5f, 1f);

        while (tempoMapeado < duracaoTotal)
        {
            tempoMapeado += Time.deltaTime;
            // Interpola a cor de forma linear criando o efeito de "pulso"
            float interpolador = Mathf.PingPong(tempoMapeado * 4f, 1f);
            sr.color = Color.Lerp(corOriginal, corBrancaBrilhante, interpolador);
            yield return null;
        }

        sr.color = corOriginal;
    }

    /// <summary>
    /// Apenas aplica a Texture2D no material do vidro sem quebrá-lo.
    /// </summary>
    public void AplicarTexturaNoCubo(Texture2D textura)
    {
        if (cuboMaterialAlvo != null)
        {
            cuboMaterialAlvo.AplicarTextura(textura);
        }
    }
    /// <summary>
    /// Dispara o efeito de estilhaçar a tela de vidro.
    /// </summary>
    public void EstourarTelaDeVidro()
    {
        if (cuboMaterialAlvo != null)
        {
            // Como o método do vidro é um IEnumerator, precisamos iniciá-lo via Coroutine aqui
            StartCoroutine(cuboMaterialAlvo.ShatterCoroutine(cuboMaterialAlvo.transform.position));
        }
    }

    private void EntityDied(BattleEntity b)
    {
        actionQueue.Remove(actionQueue.Where(a => a.executor == b).FirstOrDefault());
        timelineUI.RemoverIcone(b);
    }
    private void AplicarEfeitoTransparencia(CharEntity heroiFocado)
    {
        // Passa por absolutamente todas as entidades da batalha
        foreach (var entidade in GetAllEntities())
        {
            if (entidade is CharEntity aliado)
            {
                if (aliado == heroiFocado)
                    aliado.DefinirOpacidade(1f); // Herói ativo fica 100% visível
                else
                    aliado.DefinirOpacidade(0.3f); // Outros aliados ficam transparentes
            }
            else if (entidade is EnemyEntity inimigo)
            {
                // Opcional: Se quiser que os inimigos também fiquem um pouco transparentes
                // enquanto você escolhe a ação no menu principal:
                // inimigo.DefinirOpacidade(0.3f); 
            }
        }
    }

    private void ResetarTransparenciaDeTodos()
    {
        foreach (var entidade in GetAllEntities())
        {
            if (entidade is CharEntity aliado) aliado.DefinirOpacidade(1f);
            // if (entidade is EnemyEntity inimigo) inimigo.DefinirOpacidade(1f);
        }
    }
    #endregion

    #region MOTOR DE PREVIS�O VISUAL (GIZMOS NO EDITOR)
    private void OnDrawGizmos()
    {
        // Se o jogo j� estiver rodando, n�o precisamos desenhar a previs�o por cima dos her�is reais
        if (Application.isPlaying) return;

        // --- 1. DESENHO DOS ALIADOS (Lado Direito) ---
        Gizmos.color = Color.cyan;
        // Desenha uma linha vertical mostrando a altura do limite da coluna de aliados
        Vector3 topoAliados = centroChaoAliados + new Vector3(0, alturaTotalColunaAliados / 2, 0);
        Vector3 baseAliados = centroChaoAliados - new Vector3(0, alturaTotalColunaAliados / 2, 0);
        Gizmos.DrawLine(topoAliados, baseAliados);

        // Simula o spawn de uma equipe de at� 3 aliados para teste visual
        int testeAliadosCount = 3;
        for (int i = 0; i < testeAliadosCount; i++)
        {
            float stepY = alturaTotalColunaAliados / (testeAliadosCount + 1);
            float posY = centroChaoAliados.y + (alturaTotalColunaAliados / 2) - (stepY * (i + 1));
            Vector3 posPrevisao = new Vector3(centroChaoAliados.x, posY, 0);

            // Desenha uma esfera azul onde o aliado vai nascer
            Gizmos.DrawWireSphere(posPrevisao, 0.3f);
        }

        // --- 2. DESENHO DOS INIMIGOS (Lado Esquerdo - 3 Colunas) ---
        // Vamos testar simulando 2 inimigos por coluna para ver o espacamento
        int testeInimigosPorColuna = 2;

        for (int col = 0; col < 3; col++)
        {
            // Define uma cor para cada coluna: Frente (Verde), Meio (Amarelo), Tr�s (Vermelho)
            if (col == 0) Gizmos.color = Color.green;
            else if (col == 1) Gizmos.color = Color.yellow;
            else Gizmos.color = Color.red;

            float posX = centroChaoInimigos.x - (col * distanciaEntreColunasInimigos);

            // Linha guia da coluna atual
            Vector3 topoInimigoCol = new Vector3(posX, centroChaoInimigos.y + (alturaTotalColunaInimigos / 2), 0);
            Vector3 baseInimigoCol = new Vector3(posX, centroChaoInimigos.y - (alturaTotalColunaInimigos / 2), 0);
            Gizmos.DrawLine(topoInimigoCol, baseInimigoCol);

            for (int i = 0; i < testeInimigosPorColuna; i++)
            {
                float stepY = alturaTotalColunaInimigos / (testeInimigosPorColuna + 1);
                float posY = centroChaoInimigos.y + (alturaTotalColunaInimigos / 2) - (stepY * (i + 1));
                Vector3 posPrevisaoInimigo = new Vector3(posX, posY, 0);

                // Desenha um cubo aramado representando o inimigo no ch�o
                Gizmos.DrawWireCube(posPrevisaoInimigo, new Vector3(0.5f, 0.5f, 0));

                // Desenha uma caixinha extra flutuante para checar visualmente o "Offset Voador" na coluna de tr�s (como teste)
                if (col == 2 && i == 0)
                {
                    Gizmos.color = Color.magenta;
                    Vector3 posVoador = posPrevisaoInimigo + new Vector3(0, offsetVoadorY, 0);
                    Gizmos.DrawWireCube(posVoador, new Vector3(0.4f, 0.4f, 0));
                    Gizmos.DrawLine(posPrevisaoInimigo, posVoador); // Linha ligando a sombra ao voador
                    Gizmos.color = Color.red; // Reseta a cor da coluna
                }
            }
        }
    }
    #endregion
}
struct ActionData
{
    public BattleEntity executor;
    public BattleEntity[] alvo;
    public SkillSO habilidade;
    public int turnoExecucao;
    public int turnoRecuperacao;
}