using UnityEngine;

[CreateAssetMenu(fileName = "EfeitoCura", menuName = "RPG/Itens/Efeitos/Ether")]
public class EfeitoEther : EfeitoConsumivel
{
    public int quantidadeRecuperacaoMana;

    public override void AplicarEfeito(CombatenteData alvo)
    {
        alvo.manaAtual = Mathf.Min(alvo.manaAtual + quantidadeRecuperacaoMana, alvo.GetMaxManaTotal());
    }

    public override void AplicarEfeito(BattleEntity alvo)
    {
        alvo.CurrentMP = Mathf.Min(alvo.CurrentMP + quantidadeRecuperacaoMana, alvo.MaxMP);
    }
}
