using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Prefabs e Visuais")]
    public GameObject enemyPrefab;
    public GameObject allyPrefab;
    public Sprite shadowSprite;

    [Header("Configurações dos Inimigos (Esquerda)")]
    [SerializeField] private Vector3 centroChaoInimigos = new Vector3(-4f, -1.5f, 0);
    public float distanciaEntreColunasInimigos = 2.0f;
    public float alturaTotalColunaInimigos = 4.0f;
    public float offsetVoadorY = 1.5f;

    [Header("Configurações dos Aliados (Direita)")]
    [SerializeField] private Vector3 centroChaoAliados = new Vector3(4f, -1.5f, 0);
    public float alturaTotalColunaAliados = 4.0f;

    private List<ActionData> actionQueue = new List<ActionData>();
    private int currentTurn = 0;

    private List<EnemyEntity> enemies = new();
    private List<CharEntity> allies = new();

    public List<EnemyEntity> Enemies => enemies;
    public List<CharEntity> Allies => allies;

    [Header("Referências de UI")]
    public MenuFocadoNoPlayer menuUI;
    public BarraProgresso timelineUI;

    void Awake()
    {
        SpawnEnemies();
        SpawnAllies();
    }

    void Start()
    {
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

        var animador = go.GetComponent<Animator>();

        if (animador != null)
        {
            animador.runtimeAnimatorController = dados.fichaBase.battleAnimator;
        }

        var sr = go.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.sortingOrder = 10 + order;
            sr.flipX = true;
        }

        CharEntity entity = go.AddComponent<CharEntity>();
        entity.Setup(dados);

        allies.Add(entity);

        go.name = dados.fichaBase.charName;
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

        ExecuteActions();

        UpdateRecovery();

        yield return StartCoroutine(AskForActionsCoroutine());

        CheckBattleEnd();

        bool carregouSegmento = false;
        timelineUI.AtualizarProgressoTurno(() => carregouSegmento = true);
        yield return new WaitUntil(() => carregouSegmento);

        currentTurn++;
    }

    void ExecuteActions()
    {
        List<ActionData> actionsThisTurn =
            actionQueue
            .Where(a => a.turnoExecucao == currentTurn)
            .OrderByDescending(a => a.executor.Agilidade)
            .ToList();

        bool acted = actionsThisTurn.Count > 0;
        foreach (var action in actionsThisTurn)
        {
            ExecuteAction(action);

            actionQueue.Remove(action);
        }

        if (acted)
            timelineUI.ZerarBarra(currentTurn + 1);
    }

    void ExecuteAction(ActionData action)
    {
        if (!action.executor.IsAlive) return;

        timelineUI.RemoverIcone(action.executor);

        foreach (var alvo in action.alvo)
        {
            if (!alvo.IsAlive) continue;
            alvo.ReceiveAction(action.executor, action.habilidade);
        }

        Debug.Log($"{action.executor.EntityName} usou {action.habilidade.skillName}");
        action.executor.CurrentState = BattleState.Resting;
        action.executor.ReadyTurn = currentTurn + action.turnoRecuperacao;
    }

    void QueueAction(BattleEntity executor, BattleEntity[] alvo, SkillSO habilidade)
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

        int turnoRecuperacao = turnoDeExecucao + habilidade.turnosRecuperacao;

        timelineUI.AdicionarOuMoverIconeDuplo(executor, turnoDeExecucao, turnoRecuperacao, executor.Icon);

        Debug.Log($"{executor.EntityName} começou preparar {habilidade.skillName}");
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
                QueueAction(entity, decision.targets, decision.skill);
        }

        foreach (var entity in Allies)
        {
            if (!entity.IsAlive || entity.CurrentState != BattleState.WaitingAction) { Debug.LogWarning("Não atua"); continue; }

            ((CharEntity)entity).EscoolherAcaoDoPlayer(GetAllEntities(), menuUI);

            yield return new WaitUntil(() => ((CharEntity)entity).DecididoNoTurno == true);

            BattleDecision decision = ((CharEntity)entity).ObterDecisaoFinal();
            if (decision.skill != null)
            {
                QueueAction(entity, decision.targets, decision.skill);
            }
        }
    }

    public void CheckBattleEnd()
    {
        bool allEnemiesDead = enemies.All(e => !e.IsAlive);
        bool allAlliesDead = allies.All(a => !a.IsAlive);
        if (allEnemiesDead)
        {
            Debug.Log("Vitória!");
        }
        else if (allAlliesDead)
        {
            Debug.Log("Derrota...");
        }
    }

    #endregion

    #region UTILITÁRIOS
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
    #endregion

    #region MOTOR DE PREVISÃO VISUAL (GIZMOS NO EDITOR)
    private void OnDrawGizmos()
    {
        // Se o jogo já estiver rodando, não precisamos desenhar a previsão por cima dos heróis reais
        if (Application.isPlaying) return;

        // --- 1. DESENHO DOS ALIADOS (Lado Direito) ---
        Gizmos.color = Color.cyan;
        // Desenha uma linha vertical mostrando a altura do limite da coluna de aliados
        Vector3 topoAliados = centroChaoAliados + new Vector3(0, alturaTotalColunaAliados / 2, 0);
        Vector3 baseAliados = centroChaoAliados - new Vector3(0, alturaTotalColunaAliados / 2, 0);
        Gizmos.DrawLine(topoAliados, baseAliados);

        // Simula o spawn de uma equipe de até 3 aliados para teste visual
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
            // Define uma cor para cada coluna: Frente (Verde), Meio (Amarelo), Trás (Vermelho)
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

                // Desenha um cubo aramado representando o inimigo no chão
                Gizmos.DrawWireCube(posPrevisaoInimigo, new Vector3(0.5f, 0.5f, 0));

                // Desenha uma caixinha extra flutuante para checar visualmente o "Offset Voador" na coluna de trás (como teste)
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