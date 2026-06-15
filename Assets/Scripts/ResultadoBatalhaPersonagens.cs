using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultadoBatalhaPersonagens : MonoBehaviour
{
    public Image portrait;
    public Image xpFill;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI levelUpText;

    [Header("Configurações de Animação")]
    [Tooltip("Quanta XP visual a barra ganha por segundo durante a animação")]
    [SerializeField] private float velocidadeXpPorSegundo = 20f;

    private CombatenteData dataAtual;
    private int levelVisual;
    private float xpVisual;
    private Coroutine animacaoXPCoroutine;

    public void Setup(CombatenteData data)
    {
        dataAtual = data;
        portrait.sprite = dataAtual.fichaBase.charPortrait;

        // Começamos o "visual" com o nível e XP antigos/iniciais (antes da animação)
        // Se você chama o Setup ANTES de dar a XP no CombatenteData, guarde o valor atual.
        // Se você chama o Setup DEPOIS de dar a XP, precisamos que você passe quanta XP ele tinha antes, 
        // ou faremos a animação partir de onde a barra estava guardada.
        levelVisual = dataAtual.nivelAtual;
        xpVisual = dataAtual.xpAtual; // Caso queira animar o ganho, veja a nota abaixo.

        levelText.text = $"Lv. {levelVisual}";
        levelUpText.gameObject.SetActive(false);

        AtualizarBarraInstante();
    }

    /// <summary>
    /// Inicia a animação da barra de XP até alcançar o valor real do CombatenteData.
    /// </summary>
    public void IniciarAnimacaoXP()
    {
        if (animacaoXPCoroutine != null) StopCoroutine(animacaoXPCoroutine);
        velocidadeXpPorSegundo = Mathf.Max((dataAtual.xpAtual - xpVisual) / 5, 5f); // Garante que a velocidade seja positiva
        animacaoXPCoroutine = StartCoroutine(AnimarBarraXP());
    }

    private IEnumerator AnimarBarraXP()
    {
        // O alvo final que a animação precisa alcançar na memória
        int xpAlvoFinal = dataAtual.xpAtual;

        // Enquanto a nossa barra visual não alcançar o dado real do personagem
        while (xpVisual < xpAlvoFinal)
        {
            // Sobe o XP visual de forma constante independente do frame rate
            xpVisual += velocidadeXpPorSegundo * Time.deltaTime;

            // Se o XP visual passar do limite atual, garante que não ultrapasse o teto final bruscamente
            if (xpVisual > xpAlvoFinal) xpVisual = xpAlvoFinal;

            // Descobre os limites do nível visual atual
            int xpInicioNivelAtual = dataAtual.GetXpTotalNecessariaParaNivel(levelVisual);
            int xpProximoNivel = dataAtual.GetXpTotalNecessariaParaNivel(levelVisual + 1);
            int xpRequeridaNesseNivel = xpProximoNivel - xpInicioNivelAtual;

            // Se subiu de nível na simulação visual
            if (xpVisual >= xpProximoNivel)
            {
                levelVisual++;
                levelText.text = $"Lv. {levelVisual}";
                levelUpText.gameObject.SetActive(true);
                levelUpText.text = "Level Up!";

                // Força a barra a zerar visualmente para o próximo ciclo
                xpFill.fillAmount = 0f;
            }
            else
            {
                // Calcula a porcentagem apenas baseada no "degrau" do nível atual
                float xpGanProcessada = xpVisual - xpInicioNivelAtual;
                xpFill.fillAmount = Mathf.Clamp01(xpGanProcessada / xpRequeridaNesseNivel);
            }

            yield return null; // Espera o próximo frame
        }

        // Garante o ajuste perfeito no último frame
        AtualizarBarraInstante();
    }

    private void AtualizarBarraInstante()
    {
        int xpInicio = dataAtual.GetXpTotalNecessariaParaNivel(levelVisual);
        int xpProximo = dataAtual.GetXpTotalNecessariaParaNivel(levelVisual + 1);

        float xpNivelAtual = xpVisual - xpInicio;
        float totalNivelAtual = xpProximo - xpInicio;

        xpFill.fillAmount = totalNivelAtual > 0 ? Mathf.Clamp01(xpNivelAtual / totalNivelAtual) : 1f;
        levelText.text = $"Lv. {levelVisual}";
    }
}