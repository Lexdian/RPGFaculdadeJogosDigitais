using System.Collections.Generic;
using UnityEngine;

public class GerenciadorSetasUI : MonoBehaviour
{
    [SerializeField] private SetaAlvoUI setaPrefab; // Arraste o Prefab da seta aqui
    [SerializeField] private Transform containerUI;  // O objeto pai no Canvas onde as setas vão ficar

    private List<SetaAlvoUI> setasAtivas = new List<SetaAlvoUI>();

    // Chame esta função passando a lista de alvos (pode ser 1 ou vários)
    public void MostrarSetasNosAlvos(List<BattleEntity> alvos)
    {
        LimparSetas(); // Garante que não há setas antigas na tela
        Debug.Log("Mostrando setas para " + alvos.Count + " alvos.");
        if (alvos == null || alvos.Count == 0) return;

        foreach (BattleEntity alvo in alvos)
        {
            if (alvo == null) continue;

            // Instancia uma nova seta dentro do Canvas
            SetaAlvoUI novaSeta = Instantiate(setaPrefab, containerUI);
            novaSeta.Inicializar(alvo.transform);

            setasAtivas.Add(novaSeta);
        }
    }

    // Chame esta função quando a seleção terminar ou a ação for executada
    public void LimparSetas()
    {
        foreach (SetaAlvoUI seta in setasAtivas)
        {
            if (seta != null)
            {
                Destroy(seta.gameObject);
            }
        }
        setasAtivas.Clear();
    }
}