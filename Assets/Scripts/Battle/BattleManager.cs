using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
    public MenuFocadoNoPlayer menuUI;
    public BarraProgresso timelineUI;
    public GameObject charInfoPrefab;
    public RectTransform charInfosContainer;
    public CaixaMensagem caixaMensagem;

    void Awake()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.emCombate = true;
        SpawnEnemies();
        SpawnAllies();
    }

    void Start()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(charInfosContainer);
        charInfosContainer.GetComponent<VerticalLayoutGroup>().enabled = false;
        StartCoroutine(BattleLoop());
    }

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

            InstanciarInimigo(data, posicaoFinal, i + (indexColuna * 3));
        }
    }

    void InstanciarInimigo(EnemySO data, Vector3 posicao, int order)
    {
        GameObject go = Instantiate(enemyPrefab, posicao, Quaternion.identity);

        var sr = go.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.sprite = data.enemySprite;
            sr.sortingOrder = 10 + order;
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

            InstanciarAliado(dadosAliado, posicaoFinal, i);
        }
    }

    void InstanciarAliado(CombatenteData dados, Vector3 posicao, int order)
    {
        GameObject go = Instantiate(allyPrefab, posicao, Quaternion.identity);

        var sr = go.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.sprite = dados.fichaBase.charBattle;
            sr.sortingOrder = 10 + order;
            sr.flipX = true;
        }

        CharEntity entity = go.AddComponent<CharEntity>();
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
        while (currentTurn < 10)
        {
            yield return StartCoroutine(MainPipelineCoroutine());
        }
    }

    public IEnumerator MainPipelineCoroutine()
    {
        Debug.Log($"=========================TURNO {currentTurn}=========================");

        // MODIFICADO: Agora espera a execu��o passo a passo das a��es com anima��o e delay
        yield return StartCoroutine(ExecuteActionsCoroutine());

        UpdateRecovery();

        yield return StartCoroutine(AskForActionsCoroutine());

        CheckBattleEnd();

        bool carregouSegmento = false;
        timelineUI.AtualizarProgressoTurno(() => carregouSegmento = true);
        yield return new WaitUntil(() => carregouSegmento);

        currentTurn++;
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

            actionQueue.Remove(action);
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

            BattleDecision decision = ((EnemyEntity)entity).GetAction(GetAllEntities());
            if (decision.skill != null)
                // MODIFICADO: Espera o enfileiramento e poss�veis anima��es de transi��o suave terminar
                yield return StartCoroutine(QueueActionCoroutine(entity, decision.targets, decision.skill));
        }

        foreach (var entity in Allies)
        {
            if (!entity.IsAlive || entity.CurrentState != BattleState.WaitingAction) { Debug.LogWarning("N�o atua"); continue; }

            ((CharEntity)entity).EscoolherAcaoDoPlayer(GetAllEntities(), menuUI);

            CharInfosMap[entity].MoverParaEsquerda();

            yield return new WaitUntil(() => ((CharEntity)entity).DecididoNoTurno == true);

            BattleDecision decision = ((CharEntity)entity).ObterDecisaoFinal();
            if (decision.skill != null)
            {
                // MODIFICADO: Espera o enfileiramento e poss�veis anima��es de transi��o suave terminar
                yield return StartCoroutine(QueueActionCoroutine(entity, decision.targets, decision.skill));
            }

            CharInfosMap[entity].VoltarParaPosicaoInicial();
        }
    }

    public void CheckBattleEnd()
    {
        bool allEnemiesDead = enemies.All(e => !e.IsAlive);
        bool allAlliesDead = allies.All(a => !a.IsAlive);
        if (allEnemiesDead)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.emCombate = false;
            Debug.Log("Vit�ria!");
        }
        else if (allAlliesDead)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.emCombate = false;
            Debug.Log("Derrota...");
        }
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