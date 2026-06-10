using UnityEngine;
using UnityEngine.Rendering.Universal; // Essencial para acessar o Light2D

public class FlickerLight2D : MonoBehaviour
{
    private Light2D _light2D; // Referência para a luz

    [Header("Configurações de Intensidade")]
    [Tooltip("A intensidade mínima que a luz pode atingir.")]
    public float minIntensity = 0.8f;
    [Tooltip("A intensidade máxima que a luz pode atingir.")]
    public float maxIntensity = 1.2f;

    [Header("Configurações de Velocidade")]
    [Tooltip("Quão rápido a luz muda entre as intensidades. Valores maiores piscam mais rápido.")]
    public float flickerSpeed = 0.1f;

    private float _nextFlickerTime;

    void Start()
    {
        // Pega o componente Light2D no mesmo GameObject
        _light2D = GetComponent<Light2D>();

        if (_light2D == null)
        {
            Debug.LogError($"FlickerLight2D: Nenhum componente Light2D encontrado em {gameObject.name}. O script foi desativado.");
            enabled = false;
        }
    }

    void Update()
    {
        // Verifica se é hora de mudar a intensidade novamente
        if (Time.time >= _nextFlickerTime)
        {
            // Define uma nova intensidade aleatória dentro do intervalo
            _light2D.intensity = Random.Range(minIntensity, maxIntensity);

            // Define quando será a próxima oscilação
            _nextFlickerTime = Time.time + flickerSpeed;
        }
    }
}