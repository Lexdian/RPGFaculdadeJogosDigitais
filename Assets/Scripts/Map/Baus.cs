using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Baus : DialogTrigger
{
    [SerializeField]
    private Sprite aberto;
    [SerializeField]
    private Sprite fechado;

    [SerializeField]
    private int moedas;
    [SerializeField]
    private ItemSO item;
    [SerializeField]
    private int quantidadeItem;

    private bool abertoBau = false;

    private string[] realDialog;

    [TextArea(3, 10)]
    public string[] dialogAberto;

    private void Start()
    {
        realDialog = dialog;
        GetComponent<SpriteRenderer>().sprite = fechado;
    }

    public override void Interact(GameObject actor)
    {
        if (!abertoBau)
        {
            // Criamos a lista baseada no diálogo inicial do baú
            List<string> dinamicoDialog = realDialog.ToList();

            // --- NOVO: Revela o que tem dentro do baú por padrão ---
            if (moedas > 0 && item != null)
            {
                dinamicoDialog.Add($"Dentro do baú há {moedas} moedas e x{quantidadeItem} {item.name}!");
            }
            else if (moedas > 0)
            {
                dinamicoDialog.Add($"Dentro do baú há {moedas} moedas!");
            }
            else if (item != null)
            {
                dinamicoDialog.Add($"Dentro do baú há x{quantidadeItem} {item.name}!");
            }
            else
            {
                dinamicoDialog.Add("O baú está vazio!");
            }
            // --------------------------------------------------------

            // Tenta adicionar o item ao inventário
            if (item == null || GameManager.Instance.inventarioGrupo.TryAdd(item))
            {
                // Se deu certo, adiciona o gold e as mensagens de sucesso
                if (moedas > 0)
                {
                    dinamicoDialog.Add($"Você ganhou {moedas} moedas!");
                    GameManager.Instance.gold += moedas;
                }

                if (item != null)
                {
                    dinamicoDialog.Add($"Você obteve x{quantidadeItem} : {item.name}!");
                }

                // Aplica o diálogo final completo ao DialogTrigger
                dialog = dinamicoDialog.ToArray();

                GetComponent<SpriteRenderer>().sprite = aberto;
                base.Interact(actor);

                // Prepara para as próximas interações (baú já aberto)
                realDialog = dialogAberto;
                abertoBau = true;
            }
            else
            {
                // Se o inventário estiver cheio, avisa o jogador e NÃO abre o baú
                dinamicoDialog.Add("Mas o seu inventário está cheio! Você não pode pegar o item.");
                dialog = dinamicoDialog.ToArray();

                base.Interact(actor);
            }
            return;
        }

        // Se o baú já foi aberto no passado, mostra apenas o dialogAberto
        dialog = realDialog;
        base.Interact(actor);
    }
}