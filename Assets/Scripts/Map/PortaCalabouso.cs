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
        estaAberta = !estaAberta; 

        doorRenderer.sprite = estaAberta ? spriteAberta : spriteFechada;

        colisorParede.enabled = !estaAberta; 
    }
}