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

        // Aplica o Animator específico de batalha configurado no ScriptableObject
        var animador = go.GetComponent<Animator>();
        if (animador != null)
        {
            animador.runtimeAnimatorController = dados.fichaBase.battleAnimator;
        }

        // Configura o visual básico e espelha o sprite para olhar em direção aos inimigos
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 10 + order;
            sr.flipX = true;
        }

        go.name = dados.fichaBase.charName;
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
    #endregion
}