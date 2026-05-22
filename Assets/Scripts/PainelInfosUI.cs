using UnityEngine;
using TMPro;

public class PainelInfosUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtTempo;
    [SerializeField] private TextMeshProUGUI txtGold;

    private float tempoTotal = 0f;

    private void Update()
    {
        tempoTotal += Time.unscaledDeltaTime;
        int horas = (int)(tempoTotal / 3600);
        int minutos = (int)(tempoTotal % 3600 / 60);
        int segundos = (int)(tempoTotal % 60);
        txtTempo.text = $"Tempo {horas:00}:{minutos:00}:{segundos:00}";
        txtGold.text = $"Gold {GameManager.Instance.gold}";
    }

}