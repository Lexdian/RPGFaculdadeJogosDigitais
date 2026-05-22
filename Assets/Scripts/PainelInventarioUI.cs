using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PainelInventarioUI : MonoBehaviour
{
    [SerializeField] private GameObject cardItemPrefab;
    [SerializeField] private Transform painelListaItens;

    [SerializeField] private Button btnConsumiveis;
    [SerializeField] private Button btnEquipaveis;
    [SerializeField] private Button btnMateriais;

    private ItemCategory categoriaAtual = ItemCategory.Consumivel;

    private void Start()
    {
        btnConsumiveis.onClick.AddListener(() => MostrarCategoria(ItemCategory.Consumivel));
        btnEquipaveis.onClick.AddListener(() => MostrarCategoria(ItemCategory.Equipavel));
        btnMateriais.onClick.AddListener(() => MostrarCategoria(ItemCategory.Material));

        MostrarCategoria(ItemCategory.Consumivel);
    }
    private void OnEnable()
    {
        if (GameManager.Instance != null)
            MostrarCategoria(categoriaAtual);
    }

    public void MostrarCategoria(ItemCategory categoria)
    {
        categoriaAtual = categoria;

        foreach (Transform filho in painelListaItens)
            Destroy(filho.gameObject);

        foreach (var slot in GameManager.Instance.inventarioGrupo.Slots)
        {
            if (slot.item.category != categoria) continue;

            GameObject obj = Instantiate(cardItemPrefab, painelListaItens);
            obj.GetComponent<CardItem>().Setup(slot);
        }
    }
}