using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nomeLabel;

    private PlayerInput playerInput;

    private string[] currentDialog;
    private int currentIndex;
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        dialoguePanel.SetActive(false);

        GameObject.DontDestroyOnLoad(gameObject);
    }

    public void StartDialog(GameObject actor, string[] dialog, string nome)
    {
        dialoguePanel.SetActive(true);
        playerInput = actor.GetComponent<PlayerInput>();
        nomeLabel.text = nome;

        // Troca o mapa de input para "Dialogue"
        // Isso desativa o movimento automaticamente se "Move" não estiver no mapa Dialogue
        playerInput.SwitchCurrentActionMap("OnDialog");
        currentDialog = dialog;
        currentIndex = 0;
        OnAdvance();
    }

    private void StopDialog()
    {
        dialoguePanel.SetActive(false);
        playerInput.SwitchCurrentActionMap("OutOfBattle");
    }
    public void OnAdvance()
    {
        if (dialoguePanel.gameObject.activeSelf)
        {
            if (currentDialog == null || currentIndex >= currentDialog.Length)
            {
                StopDialog();
                return;
            }
            dialogueText.text = currentDialog[currentIndex];
            currentIndex++;
        }
    }
}
