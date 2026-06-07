using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    public string nomeCena; // Nome da cena para a qual o jogador será transportado
    public string nomeLocal;
    public Vector2 posicaoInicial;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            GameManager.Instance.MudarMapa(nomeCena, nomeLocal, posicaoInicial);
        }
    }
}
