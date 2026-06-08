using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class CharInfos : MonoBehaviour
{
    [SerializeField]
    private Image portrait;
    [SerializeField]
    private TextMeshProUGUI Name;
    [SerializeField]
    private TextMeshProUGUI HPText;
    [SerializeField]
    private TextMeshProUGUI MPText;
    [SerializeField]
    private Image HPBar;
    [SerializeField]
    private Image MPBar;

    [Header("Ícones de Status Effect")]
    [Tooltip("Container (HorizontalLayoutGroup) onde os ícones de status serão instanciados.")]
    [SerializeField] private Transform statusIconsContainer;
    [Tooltip("Prefab de um Image simples para cada ícone de status.")]
    [SerializeField] private GameObject statusIconPrefab;

    [Header("Configurações de Animação")]
    [SerializeField] private float distanciaMoverEsquerda = 155f;
    [SerializeField] private float duracaoAnimacao = 0.3f;
    [SerializeField] private Ease tipoCurva = Ease.OutQuad;

    [Header("Animação de Barras")]
    [SerializeField] private float duracaoMudancaBarras = 0.8f;
    [SerializeField] private Ease tipoCurvaBarras = Ease.OutQuad;

    private RectTransform rectTransform;
    [SerializeField]
    private float posicaoXInicial;
    private bool posicaoInicialSalva = false;

    // Guardamos o valor anterior para saber de onde o texto/número deve começar a animar
    private int hpAnterior;
    private int mpAnterior;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        SalvarPosicaoInicial();
    }

    private void SalvarPosicaoInicial()
    {
        if (rectTransform != null && !posicaoInicialSalva)
        {
            posicaoXInicial = rectTransform.anchoredPosition.x;
            posicaoInicialSalva = true;
        }
    }

    public void MoverParaEsquerda()
    {
        if (rectTransform == null) return;
        SalvarPosicaoInicial();

        rectTransform.DOKill();
        float alvoX = posicaoXInicial - distanciaMoverEsquerda;
        rectTransform.DOAnchorPosX(alvoX, duracaoAnimacao).SetEase(tipoCurva);
    }

    public void VoltarParaPosicaoInicial()
    {
        if (rectTransform == null || !posicaoInicialSalva) return;

        rectTransform.DOKill();
        rectTransform.DOAnchorPosX(posicaoXInicial, duracaoAnimacao).SetEase(tipoCurva);
    }

    public void Sesup(CombatenteData data)
    {
        SalvarPosicaoInicial();

        portrait.sprite = data.fichaBase.charPortrait;
        Name.text = data.fichaBase.charName;

        // Guarda os valores iniciais reais
        hpAnterior = data.vidaAtual;
        mpAnterior = data.manaAtual;

        HPText.text = data.vidaAtual + "/" + data.GetMaxVidaTotal();
        MPText.text = data.manaAtual + "/" + data.GetMaxManaTotal();
        HPBar.fillAmount = (float)data.vidaAtual / data.GetMaxVidaTotal();
        MPBar.fillAmount = (float)data.manaAtual / data.GetMaxManaTotal();
    }
    public IEnumerator UpdateInfos(CharEntity data)
    {
        if (rectTransform == null) yield break;
        SalvarPosicaoInicial();

        // Para qualquer animação anterior rodando nesta HUD para evitar conflitos
        rectTransform.DOKill();
        HPBar.DOKill();
        MPBar.DOKill();

        // Calcula os novos alvos
        float alvoFillHP = (float)data.CurrentHP / data.MaxHP;
        float alvoFillMP = (float)data.CurrentMP / data.MaxMP;
        float alvoXEsquerda = posicaoXInicial - distanciaMoverEsquerda;

        int maxVida = data.MaxHP;
        int maxMana = data.MaxMP;

        // Criando a linha do tempo da animação (Sequence)
        Sequence sequenciaUpdate = DOTween.Sequence();

        // PASSO 1: Move para a esquerda
        sequenciaUpdate.Append(rectTransform.DOAnchorPosX(alvoXEsquerda, duracaoAnimacao).SetEase(tipoCurva));

        // PASSO 2: Anima as duas barras juntas (usando Join) e os textos contando ao mesmo tempo
        int hpTemporario = hpAnterior;
        sequenciaUpdate.Join(HPBar.DOFillAmount(alvoFillHP, duracaoMudancaBarras).SetEase(tipoCurvaBarras));
        sequenciaUpdate.Join(DOTween.To(() => hpTemporario, x => {
            hpTemporario = x;
            HPText.text = hpTemporario + "/" + maxVida;
        }, data.CurrentHP, duracaoMudancaBarras).SetEase(tipoCurvaBarras));

        int mpTemporario = mpAnterior;
        sequenciaUpdate.Join(MPBar.DOFillAmount(alvoFillMP, duracaoMudancaBarras).SetEase(tipoCurvaBarras));
        sequenciaUpdate.Join(DOTween.To(() => mpTemporario, x => {
            mpTemporario = x;
            MPText.text = mpTemporario + "/" + maxMana;
        }, data.CurrentMP, duracaoMudancaBarras).SetEase(tipoCurvaBarras));

        // Adiciona uma pequena pausa de 0.15 segundos com a HUD aberta para o jogador processar o dano/cura
        sequenciaUpdate.AppendInterval(0.15f);

        // PASSO 3: Retorna para a posição original de design
        sequenciaUpdate.Append(rectTransform.DOAnchorPosX(posicaoXInicial, duracaoAnimacao).SetEase(tipoCurva));

        // Atualiza o histórico para o próximo update saber de onde começar
        hpAnterior = data.CurrentHP;
        mpAnterior = data.CurrentMP;

        // Atualiza os ícones de status effect junto com as barras
        AtualizarStatusIcons(data.statusAtivos);

        // ADICIONADO: Força a Coroutine a esperar TODA a sequência acima terminar antes de avançar
        yield return sequenciaUpdate.WaitForCompletion();
    }

    public void AtualizarStatusIcons(List<StatusEffectInstance> statusAtivos)
    {
        if (statusIconsContainer == null) return;

        foreach (Transform filho in statusIconsContainer)
            Destroy(filho.gameObject);

        if (statusAtivos == null || statusAtivos.Count == 0) return;

        foreach (var instancia in statusAtivos)
        {
            if (instancia.status == null || instancia.status.icon == null) continue;
            if (statusIconPrefab == null) continue;

            GameObject iconeGO = Instantiate(statusIconPrefab, statusIconsContainer);
            var img = iconeGO.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = instancia.status.icon;
                img.color = instancia.status is VenenoStatusSO ? new Color(0.6f, 0f, 0.8f)
                          : instancia.status is AtordoamentoStatusSO ? new Color(1f, 0.9f, 0f)
                          : Color.white;
            }
        }
    }
}