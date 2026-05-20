using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class LiderCharacter : Character
{
    public Character[] followers = new Character[3];
    private int limite = 60;
    private Queue<Vector2> trace = new Queue<Vector2>();
    public List<Vector2> Trace;
    [SerializeField]
    private float speed = 5f;
    private Rigidbody2D rb;

    public Vector2 Dir;
    private Vector2 lastDir;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        for (int i = 0; i < limite; i++)
        {
            trace.Enqueue(transform.position);
        }
    }
    private void FixedUpdate()
    {
        rb.linearVelocity = Dir * speed;
        if (Dir != Vector2.zero)
        {
            UpdateQueue(transform.position);
        }
        Trace = trace.ToList();
    }

    public void OnMove(InputValue input)
    {
        Dir = input.Get<Vector2>();
        if (Dir != Vector2.zero)
        {
            anim.SetBool("Moving", true);
            anim.SetFloat("DirX", input.Get<Vector2>().x);
            anim.SetFloat("DirY", input.Get<Vector2>().y);
            lastDir = Dir;
        }
        else
        {
            anim.SetBool("Moving", false);
            for (int i = 0; i < 3; i++)
            {
                if (followers[i] != null)
                {
                    followers[i].Anim.SetBool("Moving", false);
                }
            }
        }
    }
    public void OnInteract()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, lastDir, 1f, LayerMask.GetMask("Objects"));
        if (hit.collider != null)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this.gameObject);
            }
        }
    }
    private void UpdateQueue(Vector2 newPos)
    {
        trace.Dequeue();
        trace.Enqueue(newPos);
        UpdateFollower();
    }
    private void UpdateFollower()
    {
        for(int i = 0; i<3; i++)
        {
            if (followers[i] != null)
            {
                int index = limite + 1 - ((i + 1) * (int)(limite/followers.Length));
                Vector2 newPos = trace.ElementAt(index);
                followers[i].Anim.SetBool("Moving", true);
                followers[i].Anim.SetFloat("DirX", newPos.x - followers[i].transform.position.x);
                followers[i].Anim.SetFloat("DirY", newPos.y - followers[i].transform.position.y);
                followers[i].transform.position = newPos;
            }
        }
    }
}
