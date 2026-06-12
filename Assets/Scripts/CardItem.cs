using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardItem : MonoBehaviour
{
    [SerializeField] private Image imgIcone;
    [SerializeField] private TextMeshProUGUI txtNome;
    [SerializeField] private TextMeshProUGUI txtQuantidade;
    [SerializeField] private Button btnUsar;
    [SerializeField] private Button btnEquipar;

    private InventorySlot slotAtual;
    private PainelSelecaoPersonagemUI painelSelecao;

    public void Setup(InventorySlot slot)
    {
        slotAtual = slot;
        txtNome.text = slot.item.itemName;
        txtQuantidade.text = $"x{slot.quantity}";

        if (slot.item.icon != null)
            imgIcone.sprite = slot.item.icon;

        btnUsar.gameObject.SetActive(slot.item is ConsumableItemSO && ((ConsumableItemSO)slot.item).podeUsarNoMenu);
        btnEquipar.gameObject.SetActive(slot.item is EquipableItemSO);

        painelSelecao = FindObjectOfType<PainelSelecaoPersonagemUI>(true);

        btnUsar.onClick.AddListener(UsarItem);
        btnEquipar.onClick.AddListener(EquiparItem);
    }

    private void UsarItem()
    {
        painelSelecao.Abrir("Usar em qual personagem?", alvo =>
        {
            if (slotAtual.item is ConsumableItemSO consumivel)
            {
                consumivel.Aplicar(alvo);
                GameManager.Instance.inventarioGrupo.TryRemove(slotAtual.item);
                GetComponentInParent<PainelInventarioUI>().MostrarCategoria(ItemCategory.Consumivel);
            }
        });
    }

    private void EquiparItem()
    {
        painelSelecao.Abrir("Equipar em qual personagem?", alvo =>
        {
            if (slotAtual.item is EquipableItemSO equipavel)
            {
                alvo.Equipar(equipavel);
                GetComponentInParent<PainelInventarioUI>().MostrarCategoria(ItemCategory.Equipavel);
            }
        });
    }
}