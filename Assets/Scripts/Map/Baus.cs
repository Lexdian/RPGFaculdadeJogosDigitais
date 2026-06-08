using UnityEngine;

public class Baus : DialogTrigger
{
    [SerializeField]
    private Sprite aberto;
    [SerializeField]
    private Sprite fechado;

    private bool abertoBau = false;

    [TextArea(3, 10)]
    public string[] dialogAberto;
    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = fechado;
    }

    public override void Interact(GameObject actor)
    {
        if(!abertoBau)
        {
            GetComponent<SpriteRenderer>().sprite = aberto;
            dialog = dialogAberto;
        }
        base.Interact(actor);
    }
}
