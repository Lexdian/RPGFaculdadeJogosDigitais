using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NovoConsumivel", menuName = "RPG/Itens/Consumivel")]
public class ConsumableItemSO : ItemSO
{
    [Header("Restrições de Uso")]
    public bool podeUsarNoMenu = true;
    public bool podeUsarEmBatalha = true;

    [Header("Efeitos do Item")]
    [Tooltip("Arraste os ScriptableObjects de efeito para esta lista")]
    public List<EfeitoConsumivel> efeitos = new List<EfeitoConsumivel>();

    /// <summary>
    /// Aplica o item fora de combate (Menu)
    /// </summary>
    public void Aplicar(CombatenteData alvo)
    {
        if (!podeUsarNoMenu) return;

        foreach (var efeito in efeitos)
        {
            if (efeito != null) efeito.AplicarEfeito(alvo);
        }
    }

    /// <summary>
    /// Aplica o item dentro de combate (Batalha)
    /// </summary>
    public void Aplicar(BattleEntity alvo)
    {
        if (!podeUsarEmBatalha) return;

        foreach (var efeito in efeitos)
        {
            if (efeito != null) efeito.AplicarEfeito(alvo);
        }
    }
}