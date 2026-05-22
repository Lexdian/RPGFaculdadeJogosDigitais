using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Prefabs e Visuais")]
    public GameObject enemyPrefab;
    public GameObject allyPrefab; // Prefab simples contendo SpriteRenderer e Animator para a batalha
    public Sprite shadowSprite;    // Sprite de sombra circular

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

    void Start()
    {
        SpawnEnemies();
        SpawnAllies();
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

        // X da coluna: Frente fica mais perto do centro (centroChao.x). Meio e Trás recuam para a esquerda (-).
        float posX = centroChaoInimigos.x - (indexColuna * distanciaEntreColunasInimigos);

        for (int i = 0; i < total; i++)
        {
            EnemySO data = lista[i];

            // Cálculo de Y para centralizar verticalmente na tela
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

            // Cálculo de Y para balancear e centralizar a equipe dinamicamente no lado direito
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

    public void MainPipeline()
    {
        Debug.Log($"TURNO {currentTurn}");

        ExecuteActions();

        UpdateRecovery();

        AskForActions();

        CheckBattleEnd();

        currentTurn++;
    }

    void ExecuteActions()
    {
        List<ActionData> actionsThisTurn =
            actionQueue
            .Where(a => a.turnoExecucao == currentTurn)
            .OrderByDescending(a => a.executor.Agilidade)
            .ToList();

        foreach (var action in actionsThisTurn)
        {
            ExecuteAction(action);

            actionQueue.Remove(action);
        }
    }

    void ExecuteAction(ActionData action)
    {
        if (!action.executor.IsAlive)
            return;

        foreach (var alvo in action.alvo)
        {
            if (!alvo.IsAlive)
                continue;

            alvo.ReceiveAction(action.executor, action.habilidade);
        }

        Debug.Log($"{action.executor.EntityName} usou {action.habilidade.skillName}");

        action.executor.CurrentState = BattleState.Resting;

        action.executor.ReadyTurn = currentTurn + action.turnoRecuperacao;
    }

    void QueueAction(BattleEntity executor, BattleEntity[] alvo, SkillSO habilidade)
    {
        ActionData action = new ActionData
        {
            executor = executor,
            alvo = alvo,
            habilidade = habilidade,

            turnoExecucao = currentTurn + habilidade.turnosParaExecutar,

            turnoRecuperacao = habilidade.turnosRecuperacao
        };

        actionQueue.Add(action);

        executor.CurrentState = BattleState.Preparing;

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

    void AskForActions()
    {
        foreach (var entity in GetAllEntities())
        {
            if (!entity.IsAlive)
                continue;

            if (entity.CurrentState != BattleState.WaitingAction)
                continue;

            BattleDecision decision = entity.GetAction(GetAllEntities());

            if (decision.skill == null)
                continue;

            QueueAction(entity, decision.targets, decision.skill);
        }
    }

    public void CheckBattleEnd()
    {
        bool allEnemiesDead = enemies.All(e => !e.IsAlive);
        bool allAlliesDead = allies.All(a => !a.IsAlive);
        if (allEnemiesDead)
        {
            Debug.Log("Vitória!");
            // Aqui você pode chamar a tela de vitória, recompensas, etc.
        }
        else if (allAlliesDead)
        {
            Debug.Log("Derrota...");
            // Aqui você pode chamar a tela de derrota, opções de retry, etc.
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
}
struct ActionData
{
    public BattleEntity executor;
    public BattleEntity[] alvo;
    public SkillSO habilidade;
    public int turnoExecucao;
    public int turnoRecuperacao;
}