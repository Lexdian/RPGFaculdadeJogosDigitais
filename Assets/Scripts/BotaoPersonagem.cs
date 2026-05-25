using UnityEngine;
using UnityEngine.UI;

public class BotaoPersonagem : MonoBehaviour
{
    private Button _button;
    public System.Action<BotaoPersonagem> OnCharacterSelected;

    void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        OnCharacterSelected?.Invoke(this);
    }
}