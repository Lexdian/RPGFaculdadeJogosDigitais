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
    public Color corMascaraEscolha = new Color(0f, 0.5f, 1f, 1f);

    [Header("Configurações de Animação")]
    [SerializeField] private float duracaoMovimentoIcones = 0.6f;
    [SerializeField] private Ease tipoTransicao = Ease.OutQuad;

    private GameObject previewAcaoInstanciado;
    private GameObject previewRecuperacaoInstanciado;

    private const float VALOR_SEGMENTO = 0.125f; // 1/8
    private List<RectTransform> listaContainersSegmentos = new List<RectTransform>();

    private class DadosTurnoPersonagem
    {
        public Sprite foto;
        public int turnoAcao;
        public int turnoRecuperacao;
    }

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

        barraFilled.DOFillAmount(alvoFill, 0.5f)
            .SetEase(Ease.Linear)
            .OnComplete(() => onComplete?.Invoke());
    }

    public Tweener ZerarBarra(int novoPrimeiroTurno)
    {
        if (barraFilled == null) return null;

        barraFilled.DOKill();
        LimparPrevisaoTurno(); // Garante que previews antigos sumam ao resetar a barra

        primeiroTurno = novoPrimeiroTurno;
        segmentosPreenchidos = 0;

        // Anima a transição de posição dos ícones reais na tela
        AnimarMovimentoDeTodosOsIcones();

        // Retorna a animação do Fill esvaziando para sincronia com o BattleManager
        return barraFilled.DOFillAmount(0f, duracaoMovimentoIcones).SetEase(tipoTransicao);
    }

    public void AdicionarOuMoverIconeDuplo(BattleEntity personagem, int turnoAcao, int turnoRecuperacao, Sprite fotoPersonagem)
    {
        personagem.ReadyTurn = turnoAcao;

        DadosTurnoPersonagem dados = new DadosTurnoPersonagem
        {
            foto = fotoPersonagem,
            turnoAcao = turnoAcao,
            turnoRecuperacao = turnoRecuperacao
        };

        dadosDosIcones[personagem] = dados;

        if (iconesInstanciados.ContainsKey(personagem))
        {
            AnimarMovimentoDeUmPersonagem(personagem, dados);
        }
        else
        {
            InstanciarIconesIniciais(personagem, dados);
        }
    }

    public void AdicionarOuMoverIcone(BattleEntity personagem, int turnoDestino, Sprite fotoPersonagem)
    {
        AdicionarOuMoverIconeDuplo(personagem, turnoDestino, turnoDestino, fotoPersonagem);
    }

    private void InstanciarIconesIniciais(BattleEntity personagem, DadosTurnoPersonagem dados)
    {
        IconesInstanciadosPersonagem novosIcones = new IconesInstanciadosPersonagem();

        int segAcao = dados.turnoAcao - primeiroTurno;
        if (segAcao >= 0 && segAcao < 8)
        {
            Transform container = listaContainersSegmentos[segAcao];
            novosIcones.iconeAcao = Instantiate(prefabIconePersonagem, container);
            novosIcones.iconeAcao.GetComponent<Image>().sprite = dados.foto;
        }

        int segEscolha = dados.turnoRecuperacao - primeiroTurno;
        if (segEscolha >= 0 && segEscolha < 8)
        {
            Transform container = listaContainersSegmentos[segEscolha];
            novosIcones.iconeEscolha = Instantiate(prefabIconePersonagem, container);

            Image imgEscolha = novosIcones.iconeEscolha.GetComponent<Image>();
            imgEscolha.sprite = dados.foto;
            imgEscolha.color = corMascaraEscolha;
        }

        iconesInstanciados[personagem] = novosIcones;
    }

    private void AnimarMovimentoDeTodosOsIcones()
    {
        foreach (var par in dadosDosIcones)
        {
            BattleEntity personagem = par.Key;
            DadosTurnoPersonagem dados = par.Value;

            if (personagem != null && personagem.IsAlive)
            {
                AnimarMovimentoDeUmPersonagem(personagem, dados);
            }
        }
    }

    private void AnimarMovimentoDeUmPersonagem(BattleEntity personagem, DadosTurnoPersonagem dados)
    {
        if (!iconesInstanciados.TryGetValue(personagem, out IconesInstanciadosPersonagem icones)) return;

        // Mover Ícone de Ação
        int segAcao = dados.turnoAcao - primeiroTurno;
        if (icones.iconeAcao != null)
        {
            if (segAcao >= 0 && segAcao < 8)
            {
                MoverIconeParaSegmentoSuave(icones.iconeAcao, listaContainersSegmentos[segAcao]);
            }
            else
            {
                icones.iconeAcao.GetComponent<Image>().DOFade(0f, duracaoMovimentoIcones).OnComplete(() => Destroy(icones.iconeAcao));
            }
        }

        // Mover Ícone de Escolha
        int segEscolha = dados.turnoRecuperacao - primeiroTurno;
        if (icones.iconeEscolha != null)
        {
            if (segEscolha >= 0 && segEscolha < 8)
            {
                MoverIconeParaSegmentoSuave(icones.iconeEscolha, listaContainersSegmentos[segEscolha]);
            }
            else
            {
                icones.iconeEscolha.GetComponent<Image>().DOFade(0f, duracaoMovimentoIcones).OnComplete(() => Destroy(icones.iconeEscolha));
            }
        }
    }

    private void MoverIconeParaSegmentoSuave(GameObject icone, RectTransform segmentoAlvo)
    {
        if (icone == null || segmentoAlvo == null) return;

        RectTransform iconeRect = icone.GetComponent<RectTransform>();
        iconeRect.SetParent(containerSegmentos, true);

        Vector3 posicaoAlvoGlobal = segmentoAlvo.position;

        iconeRect.DOMove(posicaoAlvoGlobal, duracaoMovimentoIcones)
            .SetEase(tipoTransicao)
            .OnComplete(() =>
            {
                if (icone != null && segmentoAlvo != null)
                {
                    iconeRect.SetParent(segmentoAlvo, false);
                    iconeRect.anchoredPosition = Vector2.zero;
                }
            });
    }

    #region SISTEMA DE PREVISÃO VISUAL (RESTAURADO)

    public void MostrarPrevisaoTurno(BattleEntity executor, int turnoAcao, int turnoRecuperacao, Sprite icone)
    {
        LimparPrevisaoTurno();

        // Se os turnos de ação e recuperação caírem no mesmo slot, mescla as cores das máscaras
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

        // Proteção contra index out of bounds caso a previsão aponte para além dos 8 espaços visíveis atuais
        if (indiceSegmento < 0 || indiceSegmento >= 8) return null;

        Transform containerAlvo = listaContainersSegmentos[indiceSegmento];
        GameObject previewGO = Instantiate(prefabIconePreview, containerAlvo);

        Image img = previewGO.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = icone;
            img.color = corMascara;
            img.raycastTarget = false; // Evita que o fantasma bloqueie cliques do mouse na UI
        }

        return previewGO;
    }

    public void LimparPrevisaoTurno()
    {
        if (previewAcaoInstanciado != null) Destroy(previewAcaoInstanciado);
        if (previewRecuperacaoInstanciado != null) Destroy(previewRecuperacaoInstanciado);
    }

    #endregion

    public int GetPrimeiroTurno() => primeiroTurno;

    public void RemoverInconeAcao(BattleEntity personagem)
    {
        if (iconesInstanciados.TryGetValue(personagem, out IconesInstanciadosPersonagem icones))
        {
            if (icones.iconeAcao != null)
            {
                icones.iconeAcao.GetComponent<Image>().DOFade(0f, 0.2f).OnComplete(() => Destroy(icones.iconeAcao));
            }
        }
    }

    public void RemoverInconeEscolha(BattleEntity personagem)
    {
        if (iconesInstanciados.TryGetValue(personagem, out IconesInstanciadosPersonagem icones))
        {
            if (icones.iconeEscolha != null) Destroy(icones.iconeEscolha);
            iconesInstanciados.Remove(personagem);
        }
        dadosDosIcones.Remove(personagem);
    }

    public void RemoverIcone(BattleEntity personagem)
    {
        if (iconesInstanciados.TryGetValue(personagem, out IconesInstanciadosPersonagem icones))
        {
            if (icones.iconeAcao != null) Destroy(icones.iconeAcao);
            if (icones.iconeEscolha != null) Destroy(icones.iconeEscolha);
            iconesInstanciados.Remove(personagem);
        }
        dadosDosIcones.Remove(personagem);
    }
}