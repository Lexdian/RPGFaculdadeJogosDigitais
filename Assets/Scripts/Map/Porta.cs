using UnityEngine;

public class Porta : MonoBehaviour
{
    [SerializeField]
    private Sprite aberta;
    [SerializeField]
    private Sprite fechada;

    private AudioSource audioSource;
    public AudioClip Open;

    public MaterialItemSO chave;

    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = fechada;
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && GetComponent<SpriteRenderer>().sprite == fechada)
        {
            if (chave != null && !GameManager.Instance.inventarioGrupo.HasItem(chave))
            {
                Debug.Log("Você precisa da chave para abrir esta porta!");
                return;
            }
            GetComponent<SpriteRenderer>().sprite = aberta;
            GetComponent<BoxCollider2D>().enabled = false;
            if (audioSource != null && Open != null)
            {
                audioSource.PlayOneShot(Open);
            }
        }
    }
}
