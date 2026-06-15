using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BotaoItemUI : MonoBehaviour
{
    [Header("Referências de Texto")]
    public TextMeshProUGUI txtNome;
    public TextMeshProUGUI txtQuantidade;

    [Header("Componente de Botão")]
    public Button componenteBotao;

    // Cache interno para não quebrar a assinatura do Setup original
    private ConsumableItemSO itemArmazenado;
    private MenuFocadoNoPlayer menuPrincipal;
    void Awake()
    {
        // Encontra o menu principal subindo na hierarquia do objeto
        menuPrincipal = GetComponentInParent<MenuFocadoNoPlayer>();
    }

    public void Setup(ConsumableItemSO item, int quantidade)
    {
        itemArmazenado = item; // Guarda a referência para usar no OnSelect

        if (txtNome != null) txtNome.text = item.itemName;
        if (txtQuantidade != null) txtQuantidade.text = "x" + quantidade;
    }

    // Disparado automaticamente pelo EventSystem quando o botão ganha foco
    public void OnSelect(BaseEventData eventData)
    {
        if (menuPrincipal != null && itemArmazenado != null)
        {
            menuPrincipal.AtualizarPreviewDoItem(itemArmazenado);
        }
    }

    // Disparado automaticamente pelo EventSystem quando o foco muda para outro lugar
    public void OnDeselect(BaseEventData eventData)
    {
        if (menuPrincipal != null)
        {
            menuPrincipal.LimparPreview();
        }
    }
}
