using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PainelSelecaoPersonagemUI : MonoBehaviour
{
    [SerializeField] private GameObject btnPersonagemPrefab;
    [SerializeField] private Transform painelBotoes;
    [SerializeField] private TextMeshProUGUI txtTitulo;

    private Action<CombatenteData> onPersonagemSelecionado;

    public void Abrir(string titulo, Action<CombatenteData> callback)
    {
        txtTitulo.text = titulo;
        onPersonagemSelecionado = callback;
        gameObject.SetActive(true);

        foreach (Transform filho in painelBotoes)
            Destroy(filho.gameObject);

        foreach (var combatente in GameManager.Instance.equipeAtual)
        {
            GameObject obj = Instantiate(btnPersonagemPrefab, painelBotoes);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = 
                $"{combatente.fichaBase.charName}  HP {combatente.vidaAtual}/{combatente.GetMaxVidaTotal()}";

            CombatenteData capturado = combatente;
            obj.GetComponent<Button>().onClick.AddListener(() => Selecionar(capturado));
        }
    }

    private void Selecionar(CombatenteData combatente)
    {
        onPersonagemSelecionado?.Invoke(combatente);
        gameObject.SetActive(false);
    }

    public void Cancelar()
    {
        gameObject.SetActive(false);
    }
}