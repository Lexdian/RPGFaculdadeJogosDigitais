using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image icone;
    [SerializeField] Image seta;
    [SerializeField] TextMeshProUGUI texto;

    readonly Color corHoverFundo  = new Color(0.18f, 0.28f, 0.65f, 0.86f);
    readonly Color corNormalTexto = Color.white;
    readonly Color corHoverTexto  = new Color(0.94f, 0.75f, 0.25f);

    Image background;
    RectTransform setaRT;
    Coroutine animacaoSeta;
    Vector2 setaPosOriginal;

    void Awake()
    {
        background = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
        background.color = Color.clear;

        if (seta != null)
        {
            setaRT = seta.GetComponent<RectTransform>();
            setaPosOriginal = setaRT.anchoredPosition;
        }
    }

    public void OnPointerEnter(PointerEventData e) => SetEstado(true);
    public void OnPointerExit(PointerEventData e)  => SetEstado(false);

    void SetEstado(bool hover)
    {
        background.color = hover ? corHoverFundo : Color.clear;
        icone.color      = hover ? corHoverTexto : corNormalTexto;
        texto.color      = hover ? corHoverTexto : corNormalTexto;

        if (seta == null) return;

        seta.gameObject.SetActive(hover);
        seta.color = corHoverTexto;

        if (animacaoSeta != null)
            StopCoroutine(animacaoSeta);

        if (hover)
            animacaoSeta = StartCoroutine(AnimarSeta());
        else
            setaRT.anchoredPosition = setaPosOriginal;
    }

    IEnumerator AnimarSeta()
    {
        float amplitude = 5f;  
        float velocidade = 6f; 

        while (true)
        {
            float offset = Mathf.Sin(Time.unscaledTime * velocidade) * amplitude;
            setaRT.anchoredPosition = setaPosOriginal + new Vector2(offset, 0);
            yield return null;
        }
    }
}