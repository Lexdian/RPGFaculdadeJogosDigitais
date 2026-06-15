using System.Collections;
using UnityEngine;
using DG.Tweening; // <--- Certifique-se de que o DOTween est� importado aqui

public class GlassShatter : MonoBehaviour
{
    public GameObject wholeGlass;
    public Rigidbody[] shatteredPieces;

    [Header("Configura��es do Efeito Pulsar (Vidro Inteiro)")]
    [Tooltip("A cor que o vidro vai piscar antes de quebrar")]
    public Color corDoPulso = Color.red;
    [Tooltip("O multiplicador de tamanho no pico do pulso (ex: 1.1f aumenta em 10%)")]
    public float intensidadePulsoEscala = 1.2f;
    [Tooltip("Dura��o total para o vidro ir e voltar uma vez")]
    public float duracaoDoPulso = 0.5f;

    [Header("Configura��es da Explos�o (Fragmentos)")]
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
            Debug.LogError("[GlassShatter] A textura passada � nula!");
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

            MeshRenderer rendererPedao = piece.GetComponent<MeshRenderer>();
            if (rendererPedao != null)
            {
                ConfigurarMaterial(rendererPedao.material, novaTextura);
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
        // STEP 1: ANIMA��O DE PULSO E COR NO VIDRO INTEIRO (WHOLEGLASS)
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

            // Aguarda o t�rmino das duas piscadas/pulsos antes de quebrar de fato
            yield return new WaitUntil(() => fimDoPreparo);

            // Esconde o vidro inteiro
            wholeGlass.SetActive(false);
            source.Play();
        }

        // ====================================================================
        // STEP 2: ATIVA OS FRAGMENTOS E APLICA A EXPLOS�O
        // ====================================================================
        foreach (Rigidbody piece in shatteredPieces)
        {
            if (piece == null) continue;

            piece.gameObject.SetActive(true);
            piece.isKinematic = false;
            piece.AddExplosionForce(shatterForce, hitPoint, shatterRadius, upwardsModifier);
        }

        // Aguarda os peda�os ca�rem (2.5 segundos)
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