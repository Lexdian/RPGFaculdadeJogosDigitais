using System.Collections;
using UnityEngine;
using DG.Tweening; // <--- Certifique-se de que o DOTween está importado aqui

public class GlassShatter : MonoBehaviour
{
    public GameObject wholeGlass;
    public Rigidbody[] shatteredPieces;

    [Header("Configurações do Efeito Pulsar (Vidro Inteiro)")]
    [Tooltip("A cor que o vidro vai piscar antes de quebrar")]
    public Color corDoPulso = Color.red;
    [Tooltip("O multiplicador de tamanho no pico do pulso (ex: 1.1f aumenta em 10%)")]
    public float intensidadePulsoEscala = 1.2f;
    [Tooltip("Duração total para o vidro ir e voltar uma vez")]
    public float duracaoDoPulso = 0.5f;

    [Header("Configurações da Explosão (Fragmentos)")]
    public float shatterForce = 10f;
    public float shatterRadius = 5f;
    public float upwardsModifier = 0.5f;

    private bool hasShattered = false;

    private AudioSource source;

    void Awake()
    {
        foreach (Rigidbody piece in shatteredPieces)
        {
            if (piece == null) continue;
            piece.gameObject.SetActive(false);
            piece.isKinematic = true;
        }

        if (wholeGlass != null)
            wholeGlass.SetActive(true);

        source = GetComponent<AudioSource>();
    }

    public void AplicarTextura(Texture2D novaTextura)
    {
        if (novaTextura == null)
        {
            Debug.LogError("[GlassShatter] A textura passada é nula!");
            return;
        }

        if (wholeGlass != null)
        {
            MeshRenderer rendererInteiro = wholeGlass.GetComponent<MeshRenderer>();
            if (rendererInteiro != null)
            {
                ConfigurarMaterial(rendererInteiro.material, novaTextura);
            }
        }

        foreach (Rigidbody piece in shatteredPieces)
        {
            if (piece == null) continue;

            MeshRenderer rendererPedaço = piece.GetComponent<MeshRenderer>();
            if (rendererPedaço != null)
            {
                ConfigurarMaterial(rendererPedaço.material, novaTextura);
            }
        }

        Debug.Log("[GlassShatter] Nova textura aplicada com sucesso a toda a estrutura!");
    }

    private void ConfigurarMaterial(Material mat, Texture2D tex)
    {
        mat.mainTexture = tex;
        mat.mainTextureScale = new Vector2(1, -1);
        mat.mainTextureOffset = new Vector2(0, 1);
    }

    public IEnumerator ShatterCoroutine(Vector3 hitPoint)
    {
        if (hasShattered) yield break;
        hasShattered = true;

        // PULO DO GATO: Espera a Unity renderizar o objeto completamente neste frame
        // antes de permitir que o DOTween comece a calcular as escalas e cores!
        yield return new WaitForEndOfFrame();

        // ====================================================================
        // STEP 1: ANIMAÇÃO DE PULSO E COR NO VIDRO INTEIRO (WHOLEGLASS)
        // ====================================================================
        if (wholeGlass != null)
        {
            MeshRenderer rendInteiro = wholeGlass.GetComponent<MeshRenderer>();
            bool fimDoPreparo = false;

            Sequence seqPreparo = DOTween.Sequence();

            // Pulsa a escala do vidro inteiro (vai e volta 2 vezes)
            seqPreparo.Join(wholeGlass.transform.DOScale(wholeGlass.transform.localScale * intensidadePulsoEscala, duracaoDoPulso / 2f)
                .SetLoops(4, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad));

            // Pisca a cor do vidro inteiro (vai e volta 2 vezes)
            if (rendInteiro != null)
            {
                seqPreparo.Join(rendInteiro.material.DOColor(corDoPulso, duracaoDoPulso / 2f)
                    .SetLoops(4, LoopType.Yoyo)
                    .SetEase(Ease.InOutQuad));
            }

            seqPreparo.OnComplete(() => fimDoPreparo = true);

            // Aguarda o término das duas piscadas/pulsos antes de quebrar de fato
            yield return new WaitUntil(() => fimDoPreparo);

            // Esconde o vidro inteiro
            wholeGlass.SetActive(false);
            source.Play();
        }

        // ====================================================================
        // STEP 2: ATIVA OS FRAGMENTOS E APLICA A EXPLOSÃO
        // ====================================================================
        foreach (Rigidbody piece in shatteredPieces)
        {
            if (piece == null) continue;

            piece.gameObject.SetActive(true);
            piece.isKinematic = false;
            piece.AddExplosionForce(shatterForce, hitPoint, shatterRadius, upwardsModifier);
        }

        // Aguarda os pedaços caírem (2.5 segundos)
        yield return new WaitForSeconds(2.5f);

        // ====================================================================
        // STEP 3: LIMPEZA DA CENA
        // ====================================================================
        foreach (Rigidbody piece in shatteredPieces)
        {
            if (piece != null)
                piece.gameObject.SetActive(false);
        }

        Destroy(gameObject);
    }
}