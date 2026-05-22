using System.Collections.Generic;
using UnityEngine;

public class PainelPersonagensUI : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform container;

    private List<CardPersonagem> cards = new();

    private void Start()
    {
        if (GameManager.Instance != null)
            Atualizar();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            Atualizar();
    }

    public void Atualizar()
    {
        foreach (Transform filho in container)
            Destroy(filho.gameObject);

        cards.Clear();

        foreach (var combatente in GameManager.Instance.equipeAtual)
        {
            GameObject obj = Instantiate(cardPrefab, container);
            CardPersonagem card = obj.GetComponent<CardPersonagem>();
            card.Setup(combatente);
            cards.Add(card);
        }
    }
}