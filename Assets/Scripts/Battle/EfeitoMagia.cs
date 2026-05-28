using UnityEngine;

public class EfeitoMagia : MonoBehaviour
{
    // Chame este método no final da sua animação (via Animation Event) 
    // ou no OnParticleSystemStopped() se usar partículas
    public System.Action OnAnimacaoTerminou;

    public void FinalizarEfeito()
    {
        OnAnimacaoTerminou?.Invoke();
        Destroy(gameObject);
    }
}