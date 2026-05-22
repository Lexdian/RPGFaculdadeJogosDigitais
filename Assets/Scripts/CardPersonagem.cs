using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardPersonagem : MonoBehaviour
{
    [SerializeField] private Image imgPortrait;
    [SerializeField] private TextMeshProUGUI txtNome;
    [SerializeField] private TextMeshProUGUI txtHP;
    [SerializeField] private TextMeshProUGUI txtMP;

    public void Setup(CombatenteData dados)
    {
        txtNome.text = dados.fichaBase.charName;
        txtHP.text = $"HP {dados.vidaAtual}/{dados.GetMaxVidaTotal()}";
        txtMP.text = $"MP {dados.manaAtual}/{dados.GetMaxManaTotal()}";

        if (dados.fichaBase.charPortrait != null)
            imgPortrait.sprite = dados.fichaBase.charPortrait;
    }
}