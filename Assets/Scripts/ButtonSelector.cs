using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSelector : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] Image seta;

    static ButtonSelector atual;

    void Awake()
    {
        if (seta != null)
            seta.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData e) => Selecionar();
    public void OnPointerExit(PointerEventData e)  => Desselecionar();
    public void OnPointerClick(PointerEventData e) => Selecionar();

    void Selecionar()
    {
        if (atual != null && atual != this)
            atual.Desselecionar();

        atual = this;
        if (seta != null) seta.gameObject.SetActive(true);
    }

    public void Desselecionar()
    {
        if (seta != null) seta.gameObject.SetActive(false);
        if (atual == this) atual = null;
    }
}