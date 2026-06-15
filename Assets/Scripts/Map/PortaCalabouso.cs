using UnityEngine;
using UnityEngine.InputSystem;
public class PortaCalabouso : MonoBehaviour
{
    [Header("Visuais")]
    public SpriteRenderer doorRenderer; 
    public Sprite spriteFechada;        
    public Sprite spriteAberta;         

    [Header("Física")]
    public BoxCollider2D colisorParede; 

    private bool estaAberta = false;
    private bool jogadorPerto = false;

    public MaterialItemSO chave;

    private AudioSource audioSource;
    public AudioClip Open;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        if (jogadorPerto && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            InteragirComPorta();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) 
        {
            jogadorPerto = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jogadorPerto = false;
        }
    }

    private void InteragirComPorta()
    {
        if(chave != null && !GameManager.Instance.inventarioGrupo.HasItem(chave) && !estaAberta)
        {
            Debug.Log("Você precisa da chave para abrir esta porta!");
            return;
        }
        estaAberta = !estaAberta; 

        doorRenderer.sprite = estaAberta ? spriteAberta : spriteFechada;

        colisorParede.enabled = !estaAberta; 
        if (audioSource != null && Open != null && estaAberta)
        {
            audioSource.PlayOneShot(Open);
        }
    }
}