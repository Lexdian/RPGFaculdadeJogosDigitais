using UnityEngine;

[CreateAssetMenu(fileName = "NovoConsumivel", menuName = "RPG/Itens/Consumivel")]
public class ConsumableItemSO : ItemSO
{
    [Header("Cura")]
    public int curaVida;
    public int curaMana;

    [Header("Bônus de Status")]
    public int bonusForca;
    public int bonusInteligencia;
    public int bonusAgilidade;
    public int bonusResiliencia;
    public int bonusSorte;

    [Header("Duração")]
    public bool temporario;
    public float duracaoSegundos;

    public void Aplicar(CombatenteData alvo)
    {
        alvo.vidaAtual = Mathf.Min(alvo.vidaAtual + curaVida, alvo.GetMaxVida());
        alvo.manaAtual = Mathf.Min(alvo.manaAtual + curaMana, alvo.GetMaxMana());
    }
}