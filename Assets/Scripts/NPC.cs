using UnityEngine;

public class NPC : DialogTrigger
{
    [SerializeField]
    private Sprite Esquerda;
    [SerializeField]
    private Sprite Cima;
    [SerializeField]
    private Sprite Baixo;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void Interact(GameObject actor)
    {
        base.Interact(actor);

        if (actor != null && spriteRenderer != null)
        {
            // Tenta pegar o script do líder que interagiu com o NPC
            LiderCharacter lider = actor.GetComponent<LiderCharacter>();

            if (lider != null)
            {
                OlharBaseadoNoJogador(lider.LastDir);
            }
        }
    }

    private void OlharBaseadoNoJogador(Vector2 direcaoJogador)
    {
        // Reseta o flip por padrão
        spriteRenderer.flipX = false;

        // Se o jogador interagiu olhando para a DIREITA, o NPC olha para a ESQUERDA
        if (direcaoJogador.x > 0.1f)
        {
            spriteRenderer.sprite = Esquerda;
        }
        // Se o jogador interagiu olhando para a ESQUERDA, o NPC olha para a DIREITA (Esquerda flipada)
        else if (direcaoJogador.x < -0.1f)
        {
            spriteRenderer.sprite = Esquerda;
            spriteRenderer.flipX = true;
        }
        // Se o jogador interagiu olhando para CIMA, o NPC olha para BAIXO
        else if (direcaoJogador.y > 0.1f)
        {
            spriteRenderer.sprite = Baixo;
        }
        // Se o jogador interagiu olhando para BAIXO, o NPC olha para CIMA
        else if (direcaoJogador.y < -0.1f)
        {
            spriteRenderer.sprite = Cima;
        }
    }
}