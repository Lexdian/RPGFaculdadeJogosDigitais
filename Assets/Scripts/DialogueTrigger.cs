using UnityEngine;

public class DialogTrigger : MonoBehaviour, IInteractable
{
    [TextArea(3, 10)]
    public string[] dialog;
    public void Interact(GameObject actor)
    {
        DialogManager.Instance.StartDialog(actor, dialog);
    }

}
