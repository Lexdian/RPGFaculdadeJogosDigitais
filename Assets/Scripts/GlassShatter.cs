using System.Collections;
using UnityEngine;

public class GlassShatter : MonoBehaviour
{
    public GameObject wholeGlass;
    public Rigidbody[] shatteredPieces;

    public float shatterForce = 10f;
    public float shatterRadius = 5f;
    public float upwardsModifier = 0.5f;

    private bool hasShattered = false;

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

        //StartCoroutine(this.ShatterCoroutine(this.transform.position));
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

        // Se estiver usando URP e não atualizar, descomente as linhas abaixo:
        // mat.SetTextureScale("_BaseMap", new Vector2(1, -1));
        // mat.SetTextureOffset("_BaseMap", new Vector2(0, 1));
    }

    // MODIFICADO: Agora é um IEnumerator para suportar yields e delays temporais
    public IEnumerator ShatterCoroutine(Vector3 hitPoint)
    {
        if (hasShattered) yield break;
        hasShattered = true;

        if (wholeGlass != null)
            wholeGlass.SetActive(false);

        // 1. Ativa a física e explode os estilhaços de vidro
        foreach (Rigidbody piece in shatteredPieces)
        {
            if (piece == null) continue;

            piece.gameObject.SetActive(true);
            piece.isKinematic = false;
            piece.AddExplosionForce(shatterForce, hitPoint, shatterRadius, upwardsModifier);
        }

        // 2. Aguarda os pedaços voarem e caírem no chão (ajuste o tempo se quiser mais que 1.0f)
        yield return new WaitForSeconds(2.5f);

        // 3. Opcional: Desativa os pedaços ou faz sumirem antes de deletar o pai
        foreach (Rigidbody piece in shatteredPieces)
        {
            if (piece != null)
                piece.gameObject.SetActive(false);
        }

        // 4. Limpa o objeto da cena definitivamente
        Destroy(gameObject);
    }
}