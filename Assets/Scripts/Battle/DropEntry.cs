using UnityEngine;

[System.Serializable]
public class DropEntry
{
    public ItemSO item;

    [Range(0f, 1f)]
    [Tooltip("Chance de 0 a 1.")]
    public float chance = 0.5f;

    [Min(0)] public int quantidadeMin = 0;
    [Min(1)] public int quantidadeMax = 1;

    public int Roll()
    {
        if (item == null) return 0;
        if (Random.value > chance) return 0;
        return Random.Range(quantidadeMin, Mathf.Max(quantidadeMin, quantidadeMax) + 1);
    }
}