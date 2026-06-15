using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Collections;

public class BattleResultUI : MonoBehaviour
{
    [Header("Fundo escuro")]
    [SerializeField] private CanvasGroup overlayFundo;

    [Header("Painel de Vitória")]
    [SerializeField] private GameObject painelVitoria;
    [SerializeField] private TextMeshProUGUI textoXP;

    [Header("Painel de Derrota")]
    [SerializeField] private GameObject painelDerrota;

    [Header("Animação")]
    [SerializeField] private float duracaoFadeOverlay = 0.4f;
    [SerializeField] private float duracaoPainel = 0.5f;
    [SerializeField] private Ease curvaEntrada = Ease.OutBack;
    [SerializeField] private Button botaoContinuar;
    [SerializeField] private Button botaoVoltarAoInicio;

    [SerializeField]
    private ResultadoBatalhaPersonagens[] resultadosPersonagens;

    private bool aguardandoConfirmacao = false;

    private void Awake()
    {
        if (overlayFundo != null)
        {
            overlayFundo.alpha = 0f;
            overlayFundo.blocksRaycasts = false;
        }

        painelVitoria?.SetActive(false);
        painelDerrota?.SetActive(false);
    }

    private void Update()
    {
        if (!aguardandoConfirmacao || Keyboard.current == null) return;

        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            GameObject focado = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (focado != null)
            {
                Button botao = focado.GetComponent<Button>();
                if (botao != null && botao.interactable)
                    botao.onClick.Invoke();
            }
        }
    }

    private IEnumerator AnimarResultado(GameObject painel, Button botaoParaFocar)
    {
        if (overlayFundo != null)
        {
            overlayFundo.blocksRaycasts = true;
            overlayFundo.DOFade(0.75f, duracaoFadeOverlay);
        }

        yield return new WaitForSeconds(duracaoFadeOverlay * 0.6f);

        painel.SetActive(true);
        painel.transform.localScale = Vector3.zero;
        painel.transform
              .DOScale(Vector3.one, duracaoPainel)
              .SetEase(curvaEntrada)
              .OnComplete(() =>
              {
                  UpdateDadosPersonagens();
                  botaoParaFocar?.Select();
                  aguardandoConfirmacao = true;
              });
    }

    public void MostrarVitoria(int xpTotal, CombatenteData[] dadosPersonagens)
    {
        if (textoXP != null)
            textoXP.text = $"XP Obtido: {xpTotal}";

        for (int i = 0; i < resultadosPersonagens.Length; i++)
        {
            if (i < dadosPersonagens.Length)
            {
                resultadosPersonagens[i].Setup(dadosPersonagens[i]);
            }
        }

        StartCoroutine(AnimarResultado(painelVitoria, botaoContinuar));
    }

    public void MostrarDerrota()
    {
        StartCoroutine(AnimarResultado(painelDerrota, botaoVoltarAoInicio));
    }

    public void BotaoContinuar()
    {
        GameManager.Instance.VoltarDosCombate();
    }

    public void BotaoVoltarAoInicio()
    {
        GameManager.Instance.ResetarEquipeEVoltar();
    }
    public void UpdateDadosPersonagens()
    {
        foreach (var resultado in resultadosPersonagens)
        {
            resultado.IniciarAnimacaoXP();
        }
    }
}