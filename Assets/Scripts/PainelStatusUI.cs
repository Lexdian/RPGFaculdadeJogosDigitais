using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PainelStatusUI : MonoBehaviour
{
    [SerializeField] private GameObject btnPersonagemPrefab;
    [SerializeField] private Transform painelAbasPersonagens;

    [SerializeField] private Image imgPortrait;
    [SerializeField] private TextMeshProUGUI txtNome;
    [SerializeField] private TextMeshProUGUI txtNivel;
    [SerializeField] private TextMeshProUGUI txtXP;

    [SerializeField] private TextMeshProUGUI txtHP;
    [SerializeField] private TextMeshProUGUI txtMP;
    [SerializeField] private TextMeshProUGUI txtForca;
    [SerializeField] private TextMeshProUGUI txtInteligencia;
    [SerializeField] private TextMeshProUGUI txtAgilidade;
    [SerializeField] private TextMeshProUGUI txtResiliencia;
    [SerializeField] private TextMeshProUGUI txtSorte;

    private void Start()
    {
        CriarAbasPersonagens();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            CriarAbasPersonagens();
            MostrarStatus(GameManager.Instance.equipeAtual[0]);
        }
    }

    private void CriarAbasPersonagens()
    {
        foreach (Transform filho in painelAbasPersonagens)
            Destroy(filho.gameObject);

        foreach (var combatente in GameManager.Instance.equipeAtual)
        {
            GameObject obj = Instantiate(btnPersonagemPrefab, painelAbasPersonagens);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = combatente.fichaBase.charName;

            CombatenteData capturado = combatente;
            obj.GetComponent<Button>().onClick.AddListener(() => MostrarStatus(capturado));
        }
    }

    public void MostrarStatus(CombatenteData combatente)
    {
        txtNome.text  = combatente.fichaBase.charName;
        txtNivel.text = $"Nível {combatente.nivelAtual}";
        txtXP.text    = $"XP {combatente.xpAtual} / {combatente.fichaBase.xpBaseNecessario}";

        txtHP.text    = $"HP {combatente.vidaAtual} / {combatente.GetMaxVidaTotal()}";
        txtMP.text    = $"MP {combatente.manaAtual} / {combatente.GetMaxManaTotal()}";
        txtForca.text         = $"Força {combatente.GetForca()}";
        txtInteligencia.text  = $"Inteligência {combatente.GetInteligencia()}";
        txtAgilidade.text     = $"Agilidade {combatente.GetAgilidade()}";
        txtResiliencia.text   = $"Resiliência {combatente.GetResiliencia()}";
        txtSorte.text         = $"Sorte {combatente.GetSorte()}";

        if (combatente.fichaBase.charPortrait != null)
            imgPortrait.sprite = combatente.fichaBase.charPortrait;
    }
}