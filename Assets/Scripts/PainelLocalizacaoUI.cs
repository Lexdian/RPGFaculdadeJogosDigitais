using UnityEngine;
using TMPro;

public class PainelLocalizacao : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txtCidade;

    void Start() => Atualizar();
    void OnEnable() => Atualizar();

    void Atualizar()
    {
        if (GameManager.Instance == null) return;
        txtCidade.text = $"Cidade: {GameManager.Instance.cidadeAtual}";
    }
}