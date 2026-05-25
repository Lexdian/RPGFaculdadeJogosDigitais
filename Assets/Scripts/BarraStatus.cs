using UnityEngine;
using UnityEngine.UI;

public class BarraStatus : MonoBehaviour
{
    [SerializeField] RectTransform fill;

    [Range(0f, 1f)]
    [SerializeField] float valor = 1f;

    void OnValidate() => Atualizar();

    public void SetValor(float atual, float maximo)
    {
        valor = Mathf.Clamp01(atual / maximo);
        Atualizar();
    }

    void Atualizar()
    {
        if (fill == null) return;
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(valor, 1f);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;
    }
}