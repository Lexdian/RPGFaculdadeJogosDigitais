using UnityEngine;

public class Porta : MonoBehaviour
{
    [SerializeField]
    private Sprite aberta;
    [SerializeField]
    private Sprite fechada;

    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = fechada;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && GetComponent<SpriteRenderer>().sprite == fechada)
        {
            GetComponent<SpriteRenderer>().sprite = aberta;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
