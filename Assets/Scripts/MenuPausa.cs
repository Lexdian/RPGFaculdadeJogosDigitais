using UnityEngine;
using UnityEngine.InputSystem;

public class MenuPausa : MonoBehaviour
{
    [SerializeField] private GameObject painelMenuPausa;
    [SerializeField] private GameObject painelPersonagens;
    [SerializeField] private GameObject painelInventario;
    [SerializeField] private GameObject painelEquipamentos;
    [SerializeField] private GameObject painelStatus;
    [SerializeField] private GameObject painelSelecaoPersonagem;

    [SerializeField] private GameObject painelLocalizacao;

    [SerializeField] private GameObject painelTitulo;

    private bool pausado = false;

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (pausado)
                Fechar();
            else
                Abrir();
        }
    }

    public void Abrir()
    {
        painelMenuPausa.SetActive(true);
        Time.timeScale = 0f;
        pausado = true;
    }

    public void Fechar()
    {
        painelSelecaoPersonagem.SetActive(false);
        painelLocalizacao.SetActive(true);
        painelTitulo.SetActive(true);
        painelInventario.SetActive(false);
        painelEquipamentos.SetActive(false);
        painelStatus.SetActive(false);
        painelPersonagens.SetActive(true);
        painelMenuPausa.SetActive(false);
        Time.timeScale = 1f;
        pausado = false;
    }
}