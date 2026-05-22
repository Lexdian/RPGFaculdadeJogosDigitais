using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PainelEquipamentosUI : MonoBehaviour
{
    [SerializeField] private GameObject btnPersonagemPrefab;
    [SerializeField] private Transform painelAbasPersonagens;

    [SerializeField] private TextMeshProUGUI txtCabeca;
    [SerializeField] private TextMeshProUGUI txtCorpo;
    [SerializeField] private TextMeshProUGUI txtBotas;
    [SerializeField] private TextMeshProUGUI txtArma;
    [SerializeField] private TextMeshProUGUI txtAcessorio1;
    [SerializeField] private TextMeshProUGUI txtAcessorio2;

    [SerializeField] private Button btnDesequiparCabeca;
    [SerializeField] private Button btnDesequiparCorpo;
    [SerializeField] private Button btnDesequiparBotas;
    [SerializeField] private Button btnDesequiparArma;
    [SerializeField] private Button btnDesequiparAcessorio1;
    [SerializeField] private Button btnDesequiparAcessorio2;

    private CombatenteData personagemAtual;

    private void Start()
    {
        CriarAbasPersonagens();
        ConfigurarBotoesDesequipar();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            CriarAbasPersonagens();
            MostrarEquipamentos(GameManager.Instance.equipeAtual[0]);
        }
    }

    private void ConfigurarBotoesDesequipar()
    {
        btnDesequiparCabeca.onClick.AddListener(()    => Desequipar(SlotEquipamento.Cabeca));
        btnDesequiparCorpo.onClick.AddListener(()     => Desequipar(SlotEquipamento.Corpo));
        btnDesequiparBotas.onClick.AddListener(()     => Desequipar(SlotEquipamento.Botas));
        btnDesequiparArma.onClick.AddListener(()      => Desequipar(SlotEquipamento.Arma));
        btnDesequiparAcessorio1.onClick.AddListener(() => Desequipar(SlotEquipamento.Acessorio1));
        btnDesequiparAcessorio2.onClick.AddListener(() => Desequipar(SlotEquipamento.Acessorio2));
    }

    private void Desequipar(SlotEquipamento slot)
    {
        if (personagemAtual == null) return;
        personagemAtual.Desequipar(slot);
        MostrarEquipamentos(personagemAtual);
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
            obj.GetComponent<Button>().onClick.AddListener(() => MostrarEquipamentos(capturado));
        }
    }

    public void MostrarEquipamentos(CombatenteData combatente)
    {
        personagemAtual = combatente;

        txtCabeca.text     = combatente.cabeca      != null ? combatente.cabeca.itemName      : "---";
        txtCorpo.text      = combatente.corpo       != null ? combatente.corpo.itemName       : "---";
        txtBotas.text      = combatente.botas       != null ? combatente.botas.itemName       : "---";
        txtArma.text       = combatente.arma        != null ? combatente.arma.itemName        : "---";
        txtAcessorio1.text = combatente.acessorio1  != null ? combatente.acessorio1.itemName  : "---";
        txtAcessorio2.text = combatente.acessorio2  != null ? combatente.acessorio2.itemName  : "---";

        btnDesequiparCabeca.interactable    = combatente.cabeca     != null;
        btnDesequiparCorpo.interactable     = combatente.corpo      != null;
        btnDesequiparBotas.interactable     = combatente.botas      != null;
        btnDesequiparArma.interactable      = combatente.arma       != null;
        btnDesequiparAcessorio1.interactable = combatente.acessorio1 != null;
        btnDesequiparAcessorio2.interactable = combatente.acessorio2 != null;
    }
}