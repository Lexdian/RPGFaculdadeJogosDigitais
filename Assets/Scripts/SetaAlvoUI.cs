using UnityEngine;
using System.Collections.Generic;

public class SetaAlvoUI : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    [Header("Configurações do Piscar (Estilo Sprite)")]
    public float velocidadePiscar = 4f; // Sincronizado com o seu "* 4f" da corrotina
    // Cor estourada (HDR) idêntica à do seu script que funciona
    public Color corBrancaBrilhante = new Color(5f, 5f, 5f, 1f);

    private Camera cam;
    private RectTransform rectTransform;
    private Transform alvoAtual;

    // Listas focadas estritamente em SpriteRenderer agora
    private List<SpriteRenderer> spritesAlvo = new List<SpriteRenderer>();
    private List<Color> coresOriginais = new List<Color>();

    // Tempo acumulado para o PingPong funcionar de forma contínua
    private float tempoAcumulado = 0f;

    void Awake()
    {
        cam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
    }

    public void Inicializar(Transform alvoTransform)
    {
        // Garante que o alvo anterior volte ao normal antes de trocar
        LimparEfeitoPiscar();

        alvoAtual = alvoTransform;
        tempoAcumulado = 0f; // Reseta o tempo para o pulso começar do zero

        if (alvoAtual != null)
        {
            ObterSpritesDoAlvo();
        }

        AtualizarPosicao();
    }

    void LateUpdate()
    {
        AtualizarPosicao();
        AplicarEfeitoPiscar();
    }

    private void OnDestroy() => LimparEfeitoPiscar();
    private void OnDisable() => LimparEfeitoPiscar();

    private void AtualizarPosicao()
    {
        if (alvoAtual == null) return;
        Vector3 posicaoTela = cam.WorldToScreenPoint(alvoAtual.position + offset);
        rectTransform.position = posicaoTela;
    }

    #region LÓGICA DO PISCAR BASEADA NO SEU SCRIPT

    private void ObterSpritesDoAlvo()
    {
        spritesAlvo.Clear();
        coresOriginais.Clear();

        // Pega todos os SpriteRenderers do alvo (no pai e nos filhos)
        SpriteRenderer[] encontrados = alvoAtual.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sr in encontrados)
        {
            if (sr != null)
            {
                spritesAlvo.Add(sr);
                coresOriginais.Add(sr.color); // Salva a cor atual (geralmente Color.white)
            }
        }
    }

    private void AplicarEfeitoPiscar()
    {
        if (spritesAlvo.Count == 0) return;

        // Acumula o tempo frame a frame de forma idêntica à corrotina
        tempoAcumulado += Time.deltaTime;

        // Mesma fórmula matemática que você usou: Mathf.PingPong(tempo * velocidade, 1f)
        float interpolador = Mathf.PingPong(tempoAcumulado * velocidadePiscar, 1f);

        for (int i = 0; i < spritesAlvo.Count; i++)
        {
            if (spritesAlvo[i] == null) continue;

            // Interpola linearmente usando a cor HDR estourada
            spritesAlvo[i].color = Color.Lerp(coresOriginais[i], corBrancaBrilhante, interpolador);
        }
    }

    private void LimparEfeitoPiscar()
    {
        // Devolve rigorosamente a cor original para cada SpriteRenderer antes de limpar as listas
        for (int i = 0; i < spritesAlvo.Count; i++)
        {
            if (spritesAlvo[i] != null)
            {
                spritesAlvo[i].color = coresOriginais[i];
            }
        }

        spritesAlvo.Clear();
        coresOriginais.Clear();
    }

    #endregion
}