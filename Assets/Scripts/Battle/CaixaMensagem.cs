using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class CaixaMensagem : MonoBehaviour
{
    [Header("Componentes de UI")]
    [SerializeField] private CanvasGroup painelCanvasGroup;
    [SerializeField] private TextMeshProUGUI textoDaMensagem;

    [Header("Configurações de Animação")]
    [SerializeField] private float duracaoFade = 0.25f;
    [SerializeField] private float tempoEsperaLeitura = 1.5f;

    private bool ativo = false;

    private void Awake()
    {
        if (painelCanvasGroup != null)
        {
            painelCanvasGroup.alpha = 0f;
            painelCanvasGroup.blocksRaycasts = false;
        }
        textoDaMensagem.text = "";
    }

    public IEnumerator ExibirMensagem(string mensagem)
    {
        yield return new WaitUntil(() => ativo == false);
        MostrarMensagem(mensagem);
    }

    /// <summary>
    /// Exibe a mensagem de forma assíncrona/paralela usando o sistema de Sequences do DOTween.
    /// </summary>
    private void MostrarMensagem(string mensagem)
    {
        // Correção: usando 'return' em vez de 'break' para métodos void
        if (painelCanvasGroup == null || textoDaMensagem == null) return;

        ativo = true;

        // Para e destrói qualquer sequência anterior que ainda esteja rodando nesta caixa
        painelCanvasGroup.DOKill();

        // 1. Define o texto completo IMEDIATAMENTE
        textoDaMensagem.text = mensagem;
        painelCanvasGroup.blocksRaycasts = true;

        // Criamos uma sequência que vai gerenciar o tempo em paralelo
        Sequence sequenciaMensagem = DOTween.Sequence();

        // 2. APARECER: Adiciona o Fade In na linha do tempo
        sequenciaMensagem.Append(painelCanvasGroup.DOFade(1f, duracaoFade));

        // 3. ESPERAR: Adiciona uma pausa na linha do tempo com o painel aberto
        sequenciaMensagem.AppendInterval(tempoEsperaLeitura);

        // 4. DESAPARECER: Adiciona o Fade Out logo após a pausa
        sequenciaMensagem.Append(painelCanvasGroup.DOFade(0f, duracaoFade));

        // 5. LIMPEZA: Quando TODA a sequência terminar, desativa o raycast e limpa o texto
        sequenciaMensagem.OnComplete(() => {
            painelCanvasGroup.blocksRaycasts = false;
            textoDaMensagem.text = "";
            ativo = false;
        });
    }
}