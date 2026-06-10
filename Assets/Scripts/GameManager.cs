using System.Collections.Generic;
using System.IO; // Adicionado para manipulação de arquivos físicos
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

    // Mudado para 'public set' para que possamos manipular ou ler externamente se necessário
    public Texture2D lastTexture { get; private set; }

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

        // 1. Captura a imagem normalmente para uso em tempo de execução
        lastTexture = CapturarImagemDaCamera(Camera.main);

        // 2. Salva a cópia física em formato PNG dentro do seu projeto para análise pós-jogo
        SalvarTexturaEmDisco(lastTexture);

        SceneManager.LoadScene("BattleScene");
    }

    /// <summary>
    /// Converte a textura capturada em um arquivo PNG real dentro da pasta Assets do projeto.
    /// </summary>
    private void SalvarTexturaEmDisco(Texture2D textura)
    {
        if (textura == null) return;

        try
        {
            // Transforma os pixels da textura em uma array de bytes formatada em PNG
            byte[] bytesPng = textura.EncodeToPNG();

            // Define o caminho para a raiz da pasta Assets do seu projeto Unity
            string caminhoArquivo = Path.Combine(Application.dataPath, "UltimaCapturaBatalha.png");

            // Grava o arquivo fisicamente no SSD/HD
            File.WriteAllBytes(caminhoArquivo, bytesPng);

            Debug.Log($"[GameManager] Textura de teste salva com sucesso em: {caminhoArquivo}");

#if UNITY_EDITOR
            // Força o editor da Unity a atualizar as pastas para o arquivo aparecer nos Assets imediatamente
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Erro ao tentar gravar o PNG em disco: {e.Message}");
        }
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

    public Texture2D CapturarImagemDaCamera(Camera cameraAlvo, int largura = 1920, int altura = 1080)
    {
        if (cameraAlvo == null)
        {
            Debug.LogError("Nenhuma câmera foi passada para a captura!");
            return null;
        }

        RenderTexture rt = new RenderTexture(largura, altura, 24);
        RenderTexture anterior = cameraAlvo.targetTexture;

        cameraAlvo.targetTexture = rt;
        cameraAlvo.Render();

        RenderTexture.active = rt;

        Texture2D texturaFinal = new Texture2D(largura, altura, TextureFormat.RGB24, false);
        texturaFinal.ReadPixels(new Rect(0, 0, largura, altura), 0, 0);
        texturaFinal.Apply();

        cameraAlvo.targetTexture = anterior;
        RenderTexture.active = null;
        Destroy(rt);

        return texturaFinal;
    }
}