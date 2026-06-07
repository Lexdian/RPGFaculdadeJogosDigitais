using System.Collections;
using System.Collections.Generic; // Necessário para usar List
using UnityEngine;
using DG.Tweening;

public class BattleAnimationManager : MonoBehaviour
{
    public static BattleAnimationManager Instance;

    void Awake() => Instance = this;

    /// <summary>
    /// Gerencia o fluxo da animação de magia aceitando um array de alvos.
    /// </summary>
    public IEnumerator ExecutarAnimacaoMagia(SkillSO skill, BattleEntity conjurador, BattleEntity[] alvos)
    {
        if (skill.prefabEfeitoVisual == null)
        {
            Debug.LogWarning($"A skill {skill.name} não tem prefab de animação!");
            yield break; 
        }

        if (alvos == null || alvos.Length == 0) yield break;

        // Lista para monitorar o status de conclusão de cada VFX gerado
        List<bool> statusAnimacoes = new List<bool>();

        // Se a skill for configurada para atingir a área toda com apenas UM efeito visual centralizado
        // Nota: Você pode criar uma bool 'atingeAreaNoCentro' no seu SkillSO se quiser essa distinção
        bool instanciarNoCentro = false; 

        if (instanciarNoCentro)
        {
            // Calcula o centro geométrico entre todos os alvos vivos
            Vector3 posicaoCentral = Vector3.zero;
            int alvosContados = 0;

            foreach (var alvo in alvos)
            {
                if (alvo != null && alvo.IsAlive)
                {
                    posicaoCentral += alvo.transform.position;
                    alvosContados++;
                }
            }
            if (alvosContados > 0) posicaoCentral /= alvosContados;

            // Instancia apenas um efeito no meio deles
            CriarEInstanciarVFX(skill, posicaoCentral, statusAnimacoes);
        }
        else
        {
            // COMPORTAMENTO PADRÃO: Instancia uma cópia do efeito em cima de cada alvo vivo
            foreach (var alvo in alvos)
            {
                if (alvo == null || !alvo.IsAlive) continue;

                Vector3 posicaoAlvo = alvo.transform.position;
                CriarEInstanciarVFX(skill, posicaoAlvo, statusAnimacoes);
            }
        }

        // ESPERA COLETIVA: Fica no loop até que TODOS os status na lista virem 'true'
        bool todasConcluidas = false;
        while (!todasConcluidas)
        {
            todasConcluidas = true;
            foreach (bool concluida in statusAnimacoes)
            {
                if (!concluida)
                {
                    todasConcluidas = false;
                    break;
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Método auxiliar para instanciar o VFX e vincular o evento de término à lista de controle.
    /// </summary>
    // CORRIGIDO: Nome tudo junto, sem espaços!
    private void CriarEInstanciarVFX(SkillSO skill, Vector3 posicao, List<bool> listaStatus)
    {
        // 1. Cria o objeto visual na cena na posição do alvo
        GameObject vfxGO = Instantiate(skill.prefabEfeitoVisual, posicao, Quaternion.identity);

        // 2. Pega o componente que avisa quando a animação acabou
        EfeitoMagia efeito = vfxGO.GetComponent<EfeitoMagia>();

        if (efeito != null)
        {
            // 3. Adiciona um 'false' na lista para dizer: "tem uma animação rodando aqui"
            int indexAtual = listaStatus.Count;
            listaStatus.Add(false);

            // 4. Quando o efeito terminar de rodar, ele muda o seu próprio 'false' para 'true'
            efeito.OnAnimacaoTerminou += () => listaStatus[indexAtual] = true;
        }
        else
        {
            // Segurança: Se o prefab não tiver o script, destrói para o jogo não travar em loop
            Destroy(vfxGO);
            Debug.LogError($"O prefab de VFX da skill {skill.name} não possui o componente 'EfeitoMagia'!");
        }
    }
}