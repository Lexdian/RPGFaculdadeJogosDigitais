using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarraProgresso : MonoBehaviour
{
    private int primeiroTurno = 1;
    private int segmentosPreenchidos = 0;

    [Header("Componentes Visuais")]
    public Image barraFilled;

    [Header("Configurações de Ícones")]
    public GameObject prefabIconePersonagem;
    public RectTransform containerSegmentos;

    [Header("Configurações de Preview (Fantasmas)")]
    public GameObject prefabIconePreview;

    [Header("Máscaras de Preview")]
    public Color corAcao = Color.white;
    public Color corRest = Color.white;

    [Header("Configurações do Ícone de Escolha Definitivo")]
    // Defina no inspetor a cor azul para o ícone de retorno definitivo
    public Color corMascaraEscolha = new Color(0f, 0.5f, 1f, 1f);

    private GameObject previewAcaoInstanciado;
    private GameObject previewRecuperacaoInstanciado;

    private const float VALOR_SEGMENTO = 0.125f; // 1/8
    private List<RectTransform> listaContainersSegmentos = new List<RectTransform>();

    // Estrutura para guardar os turnos calculados de um personagem
    private class DadosTurnoPersonagem
    {
        public Sprite foto;
        public int turnoAcao;
        public int turnoRecuperacao;
    }

    // Estrutura para rastrear os dois GameObjects físicos criados na UI
    private class IconesInstanciadosPersonagem
    {
        public GameObject iconeAcao;
        public GameObject iconeEscolha;
    }

    private Dictionary<BattleEntity, DadosTurnoPersonagem> dadosDosIcones = new Dictionary<BattleEntity, DadosTurnoPersonagem>();
    private Dictionary<BattleEntity, IconesInstanciadosPersonagem> iconesInstanciados = new Dictionary<BattleEntity, IconesInstanciadosPersonagem>();

    void Awake()
    {
        ConstruirContainersDosSegmentos();
    }

    private void ConstruirContainersDosSegmentos()
    {
        if (containerSegmentos == null || barraFilled == null) return;

        float larguraTotal = barraFilled.GetComponent<RectTransform>().rect.width;
        float larguraDeUmSegmento = larguraTotal * VALOR_SEGMENTO;

        foreach (Transform child in containerSegmentos) Destroy(child.gameObject);
        listaContainersSegmentos.Clear();

        for (int i = 0; i < 8; i++)
        {
            GameObject segGO = new GameObject($"Segmento_{i}", typeof(RectTransform));
            RectTransform rectSeg = segGO.GetComponent<RectTransform>();
            rectSeg.SetParent(containerSegmentos, false);

            rectSeg.anchorMin = new Vector2(0, 0.5f);
            rectSeg.anchorMax = new Vector2(0, 0.5f);
            rectSeg.pivot = new Vector2(0.5f, 0.5f);
            rectSeg.sizeDelta = new Vector2(larguraDeUmSegmento, containerSegmentos.rect.height);

            float porcentagemCentro = (i * VALOR_SEGMENTO) + (VALOR_SEGMENTO / 2f);
            float posXLocal = porcentagemCentro * larguraTotal;
            rectSeg.anchoredPosition = new Vector2(posXLocal, 0f);

            GridLayoutGroup grid = segGO.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(16, 16);
            grid.spacing = new Vector2(2, 2);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            listaContainersSegmentos.Add(rectSeg);
        }
    }

    public void AtualizarProgressoTurno(System.Action onComplete = null)
    {
        segmentosPreenchidos += 1;
        if (barraFilled == null) return;

        float alvoFill = (segmentosPreenchidos) * VALOR_SEGMENTO;

        if (alvoFill > 1.0f) alvoFill = 1.0f;
        if (alvoFill < 0f) alvoFill = 0f;

        barraFilled.DOFillAmount(alvoFill, 1.0f)
            .SetEase(Ease.Linear)
            .OnComplete(() => onComplete?.Invoke());
    }

    public void ZerarBarra(int novoPrimeiroTurno)
    {
        if (barraFilled == null) return;

        barraFilled.DOKill();
        barraFilled.fillAmount = 0f;

        primeiroTurno = novoPrimeiroTurno;
        segmentosPreenchidos = 0;

        LimparPrevisaoTurno();
        RedesenharTodosOsIcones();
    }

    // MODIFICADO: Agora aceita os dois turnos finais calculados no momento da confirmação da Skill
    public void AdicionarOuMoverIconeDuplo(BattleEntity personagem, int turnoAcao, int turnoRecuperacao, Sprite fotoPersonagem)
    {
        // Atualiza o turno principal da entidade para o gerenciador do combate
        personagem.ReadyTurn = turnoAcao;

        DadosTurnoPersonagem dados = new DadosTurnoPersonagem
        {
            foto = fotoPersonagem,
            turnoAcao = turnoAcao,
            turnoRecuperacao = turnoRecuperacao
        };

        dadosDosIcones[personagem] = dados;

        PosicionarIconesNaTela(personagem, dados);
    }

    // Mantido por compatibilidade caso outros scripts usem a assinatura antiga (ex: Inimigos ou setups iniciais)
    public void AdicionarOuMoverIcone(BattleEntity personagem, int turnoDestino, Sprite fotoPersonagem)
    {
        AdicionarOuMoverIconeDuplo(personagem, turnoDestino, turnoDestino, fotoPersonagem);
    }

    private void PosicionarIconesNaTela(BattleEntity personagem, DadosTurnoPersonagem dados)
    {
        // Limpa os ícones antigos desse personagem antes de reposicionar
        RemoverIconesFisicos(personagem);

        IconesInstanciadosPersonagem novosIcones = new IconesInstanciadosPersonagem();

        // 1. Posiciona o Ícone de Ação (Normal)
        int segAcao = dados.turnoAcao - primeiroTurno;
        if (segAcao >= 0 && segAcao < 8)
        {
            Transform container = listaContainersSegmentos[segAcao];
            novosIcones.iconeAcao = Instantiate(prefabIconePersonagem, container);
            novosIcones.iconeAcao.GetComponent<Image>().sprite = dados.foto;
        }

        // 2. Posiciona o Ícone de Escolha Futura (Com a Máscara Azul definida)
        // Só cria se o turno de recuperação for diferente ou se você quiser os dois juntos no mesmo slot
        int segEscolha = dados.turnoRecuperacao - primeiroTurno;
        if (segEscolha >= 0 && segEscolha < 8)
        {
            Transform container = listaContainersSegmentos[segEscolha];
            novosIcones.iconeEscolha = Instantiate(prefabIconePersonagem, container);

            Image imgEscolha = novosIcones.iconeEscolha.GetComponent<Image>();
            imgEscolha.sprite = dados.foto;
            imgEscolha.color = corMascaraEscolha; // Aplica a MascaraEscolha (Azul)
        }

        iconesInstanciados[personagem] = novosIcones;
    }

    public void RemoverIcone(BattleEntity personagem)
    {
        RemoverIconesFisicos(personagem);
        dadosDosIcones.Remove(personagem);
    }

    private void RemoverIconesFisicos(BattleEntity personaje)
    {
        if (iconesInstanciados.TryGetValue(personaje, out IconesInstanciadosPersonagem icones))
        {
            if (icones.iconeAcao != null) Destroy(icones.iconeAcao);
            if (icones.iconeEscolha != null) Destroy(icones.iconeEscolha);
            iconesInstanciados.Remove(personaje);
        }
    }

    private void RedesenharTodosOsIcones()
    {
        foreach (var par in dadosDosIcones)
        {
            BattleEntity personagem = par.Key;
            DadosTurnoPersonagem dados = par.Value;

            if (personagem != null && personagem.IsAlive)
            {
                PosicionarIconesNaTela(personagem, dados);
            }
        }
    }

    #region SISTEMA DE PREVISÃO VISUAL (PREVIEWS FANTASMAS)

    public void MostrarPrevisaoTurno(BattleEntity executor, int turnoAcao, int turnoRecuperacao, Sprite icone)
    {
        LimparPrevisaoTurno();

        if (turnoAcao == turnoRecuperacao)
        {
            Color corMista = Color.Lerp(corAcao, corRest, 0.5f);
            previewAcaoInstanciado = InstanciarPrevisaoNoTurno(turnoAcao, icone, corMista);
            return;
        }

        previewAcaoInstanciado = InstanciarPrevisaoNoTurno(turnoAcao, icone, corAcao);
        previewRecuperacaoInstanciado = InstanciarPrevisaoNoTurno(turnoRecuperacao, icone, corRest);
    }

    private GameObject InstanciarPrevisaoNoTurno(int turno, Sprite icone, Color corMascara)
    {
        int indiceSegmento = turno - primeiroTurno;
        if (indiceSegmento < 0 || indiceSegmento >= 8) return null;

        Transform containerAlvo = listaContainersSegmentos[indiceSegmento];
        GameObject previewGO = Instantiate(prefabIconePreview, containerAlvo);

        Image img = previewGO.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = icone;
            img.color = corMascara;
            img.raycastTarget = false;
        }

        return previewGO;
    }

    public void LimparPrevisaoTurno()
    {
        if (previewAcaoInstanciado != null) Destroy(previewAcaoInstanciado);
        if (previewRecuperacaoInstanciado != null) Destroy(previewRecuperacaoInstanciado);
    }

    #endregion
}