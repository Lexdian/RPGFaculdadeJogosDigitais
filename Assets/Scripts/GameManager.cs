using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            inventarioGrupo = new Inventory(initialMaxSlots: 10);
            InicializarEquipe();
        }
        else
        {
            Destroy(gameObject);
        }
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
        CreateTeam();
    }

    private void CreateTeam()
    {
        if (equipeAtual.Count == 0) return;

        GameObject liderChar = Instantiate(lider, new Vector2(0, 0), Quaternion.identity);
        liderChar.GetComponent<LiderCharacter>().Setup(equipeAtual[0].fichaBase.overworldAnimator);

        for (int i = 1; i < equipeAtual.Count; i++)
        {
            GameObject newChar = Instantiate(followers, new Vector2(0, 0), Quaternion.identity);
            newChar.GetComponent<Character>().Setup(equipeAtual[i].fichaBase.overworldAnimator);
            liderChar.GetComponent<LiderCharacter>().followers[i - 1] = newChar.GetComponent<Character>();
        }
    }
}