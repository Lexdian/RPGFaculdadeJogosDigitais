using System.Collections;
using UnityEngine;
using DG.Tweening; // Aproveitando que você já usa DOTween

public class BattleAnimationManager : MonoBehaviour
{
    public static BattleAnimationManager Instance;

    void Awake() => Instance = this;

    // Esta Coroutine gerencia o fluxo da animação
    public IEnumerator ExecutarAnimacaoMagia(SkillSO skill, BattleEntity conjurador, BattleEntity alvo)
    {
        if (skill.prefabEfeitoVisual == null)
        {
            Debug.LogWarning($"A skill {skill.name} não tem prefab de animação!");
            yield break; // Se não tem visual, pula direto
        }

        Vector3 posicaoInicial = alvo.transform.position;
        GameObject vfxGO = Instantiate(skill.prefabEfeitoVisual, posicaoInicial, Quaternion.identity);
        EfeitoMagia efeito = vfxGO.GetComponent<EfeitoMagia>();

        bool animacaoConcluida = false;
        efeito.OnAnimacaoTerminou += () => animacaoConcluida = true;

        // Espera até que o script 'EfeitoMagia' chame 'FinalizarEfeito()'
        while (!animacaoConcluida)
        {
            yield return null;
        }
    }
}