using UnityEngine;

public abstract class EfeitoConsumivel : ScriptableObject
{
    // Método abstrato que as classes filhas DEVERÃO implementar para o uso fora de batalha
    public abstract void AplicarEfeito(CombatenteData alvo);

    // Método abstrato que as classes filhas DEVERÃO implementar para o uso dentro de batalha
    public abstract void AplicarEfeito(BattleEntity alvo);
}