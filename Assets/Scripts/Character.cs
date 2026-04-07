using UnityEngine;

public class Character : MonoBehaviour
{
    protected Animator anim;

    public Animator Anim => anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void Setup(RuntimeAnimatorController controller)
    {
        anim.runtimeAnimatorController = controller;
    }
}
