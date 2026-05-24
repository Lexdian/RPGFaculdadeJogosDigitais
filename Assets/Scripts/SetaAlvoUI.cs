using UnityEngine;

public class SetaAlvoUI : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Altura acima da cabeça do inimigo
    private Camera cam;
    private RectTransform rectTransform;

    void Awake()
    {
        cam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
    }

    public void MoverParaAlvo(BattleEntity alvo)
    {
        if (alvo == null) return;

        // Ativa a seta caso esteja desativada
        gameObject.SetActive(true);

        // Converte a posição do mundo 3D do alvo para a tela da UI
        Vector3 posicaoTela = cam.WorldToScreenPoint(alvo.transform.position + offset);
        rectTransform.position = posicaoTela;
    }

    public void Esconder()
    {
        gameObject.SetActive(false);
    }
}