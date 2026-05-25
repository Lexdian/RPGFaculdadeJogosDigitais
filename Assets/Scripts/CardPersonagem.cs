using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardPersonagem : MonoBehaviour
{
    [SerializeField] private Image imgPortrait;
    [SerializeField] private TextMeshProUGUI txtNome;
    [SerializeField] private TextMeshProUGUI txtHP;
    [SerializeField] private TextMeshProUGUI txtMP;

    [Header("Barras")]
    [SerializeField] private BarraStatus barraHP;
    [SerializeField] private BarraStatus barraMana;

    public void Setup(CombatenteData dados)
    {
        txtNome.text = dados.fichaBase.charName;
        txtHP.text   = $"HP {dados.vidaAtual}/{dados.GetMaxVidaTotal()}";
        txtMP.text   = $"MP {dados.manaAtual}/{dados.GetMaxManaTotal()}";

        if (dados.fichaBase.charPortrait != null)
            imgPortrait.sprite = dados.fichaBase.charPortrait;

        barraHP.SetValor(dados.vidaAtual,  dados.GetMaxVidaTotal());
        barraMana.SetValor(dados.manaAtual, dados.GetMaxManaTotal());
    }
}