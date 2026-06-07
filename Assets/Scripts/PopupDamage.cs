using TMPro;
using UnityEngine;

public enum DamageType
{
    Normal,
    Critical,
    Heal,
    Imune,
    Errou
}
public class PopupDamage : MonoBehaviour
{
    private TextMeshPro _textMesh;
    public void Setup(int damageAmount, DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Normal:
                _textMesh.color = Color.white;
                _textMesh.text = "-"+damageAmount.ToString();
                break;
            case DamageType.Critical:
                _textMesh.color = Color.red;
                _textMesh.text = "-"+damageAmount.ToString();
                break;
            case DamageType.Heal:
                _textMesh.color = Color.green;
                _textMesh.text = "+"+damageAmount.ToString();
                break;
            case DamageType.Imune:
                _textMesh.color = Color.white;
                _textMesh.text = "Imune";
                break;
            case DamageType.Errou:
                _textMesh.color = Color.white;
                _textMesh.text = "Errou";
                break;
        }
    }
    private void Awake()
    {
        _textMesh = gameObject.GetComponent<TextMeshPro>();
    }
    private void Update()
    {
        transform.position += new Vector3(0, 1, 0) * Time.deltaTime;
        _textMesh.color -= new Color(0, 0, 0, 1) * Time.deltaTime;
        if (_textMesh.color.a <= 0)
        {
            Destroy(gameObject);
        }
    }

    public static PopupDamage Create(Vector3 position, int damageAmount, DamageType damageType)
    {
        PopupDamage popup = Instantiate(Resources.Load<PopupDamage>("DamagePopUp"));
        popup.transform.position = position;
        popup.Setup(damageAmount, damageType);
        return popup;
    }
}
