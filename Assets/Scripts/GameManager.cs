using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuração de Assets dos Heróis")]
    public CharacterSO[] heroisDisponiveis = new CharacterSO[4];

    [Header("Dados de Campanha da Equipe")]
    public List<CombatenteData> equipeAtual = new List<CombatenteData>();

    [Header("Inventário do Grupo")]
    public Inventory inventarioGrupo;

    [Header("Prefabs do Overworld")]
    public GameObject lider;
    public GameObject followers;

    [Header("Dados de Batalha temporários")]
    public EnemyGroup inimigosParaSpawnar;

    [Header("Itens de Teste")]
    public List<ItemSO> itensTeste = new List<ItemSO>();

    [Header("Recursos")]
    public int gold = 0;

    [Header("Localização")]
    public string cidadeAtual = "Kran-Tor";

    [Header("Estado do Jogo")]
    public bool emCombate = false;
    private string cenaAnterior = "SampleScene";


    private Vector2 spawnPosition;
    private bool precisaRecriarEquipe = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;

            inventarioGrupo = new Inventory(initialMaxSlots: 10);
            InicializarEquipe();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InicializarEquipe()
    {
        foreach (var heroiSO in heroisDisponiveis)
        {
            if (heroiSO != null)
                equipeAtual.Add(new CombatenteData(heroiSO, 1));
        }
    }

    void Start()
    {
        CreateTeam(Vector2.zero);
        InicializarItensTeste();
    }

    private void InicializarItensTeste()
    {
        foreach (var item in itensTeste)
        {
            if (item != null)
                inventarioGrupo.TryAdd(item, 1);
        }
    }

    private void CreateTeam(Vector2 position)
    {
        if (equipeAtual.Count == 0) return;

        GameObject liderChar = Instantiate(lider, position, Quaternion.identity);
        liderChar.GetComponent<LiderCharacter>()
                 .Setup(equipeAtual[0].fichaBase.overworldAnimator);

        for (int i = 1; i < equipeAtual.Count; i++)
        {
            GameObject newChar = Instantiate(followers, position, Quaternion.identity);

            newChar.GetComponent<Character>()
                   .Setup(equipeAtual[i].fichaBase.overworldAnimator);

            liderChar.GetComponent<LiderCharacter>()
                     .followers[i - 1] = newChar.GetComponent<Character>();
        }
    }

    public void IniciarBatalha(EnemyGroup grupo, Vector2 posicaoJogador = default)
    {
        cenaAnterior = SceneManager.GetActiveScene().name;
        inimigosParaSpawnar = grupo;
        spawnPosition = posicaoJogador;
        emCombate = true;
        SceneManager.LoadScene("BattleScene");
    }

    public void VoltarDosCombate()
    {
        emCombate = false;
        precisaRecriarEquipe = true;
        SceneManager.LoadScene(cenaAnterior);
    }

    public void ResetarEquipeEVoltar()
    {
        foreach (var combatente in equipeAtual)
        {
            if (combatente == null) continue;
            combatente.vidaAtual = combatente.GetMaxVidaTotal();
            combatente.manaAtual = combatente.GetMaxManaTotal();
        }

        cenaAnterior = "SampleScene";
        emCombate = false;
        spawnPosition = Vector2.zero;
        precisaRecriarEquipe = true;
        SceneManager.LoadScene("SampleScene");
    }

    public void MudarMapa(string nomeCena, string nomeLocal, Vector2 posicaoInicial)
    {
        cidadeAtual = nomeLocal;

        spawnPosition = posicaoInicial;
        precisaRecriarEquipe = true;

        SceneManager.LoadScene(nomeCena);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (precisaRecriarEquipe)
        {
            CreateTeam(spawnPosition);
            precisaRecriarEquipe = false;
        }
    }
}