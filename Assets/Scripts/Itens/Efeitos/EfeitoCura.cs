using UnityEngine;

[CreateAssetMenu(fileName = "EfeitoCura", menuName = "RPG/Itens/Efeitos/Cura")]
public class EfeitoCura : EfeitoConsumivel
{
    public int quantidadeCuraVida;

    public override void AplicarEfeito(CombatenteData alvo)
    {
        alvo.vidaAtual = Mathf.Min(alvo.vidaAtual + quantidadeCuraVida, alvo.GetMaxVidaTotal());
    }

    public override void AplicarEfeito(BattleEntity alvo)
    {
        alvo.CurrentHP = Mathf.Min(alvo.CurrentHP + quantidadeCuraVida, alvo.MaxHP);
    }
}
