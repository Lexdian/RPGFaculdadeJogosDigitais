using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Importante para detectar foco/seleção

public class BotaoHabilidadeUI : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("Referências de Texto")]
    public TextMeshProUGUI txtNome;
    public TextMeshProUGUI txtMP;
    public TextMeshProUGUI txtDelay;
    public TextMeshProUGUI txtResting;

    [Header("Componente de Botão")]
    public Button componenteBotao;

    // Cache interno para não quebrar a assinatura do Setup original
    private SkillSO skillArmazenada;
    private MenuFocadoNoPlayer menuPrincipal;

    void Awake()
    {
        // Encontra o menu principal subindo na hierarquia do objeto
        menuPrincipal = GetComponentInParent<MenuFocadoNoPlayer>();
    }

    // Método utilitário para preencher os dados visualmente (Mantido idêntico ao seu)
    public void Setup(SkillSO skill)
    {
        skillArmazenada = skill; // Guarda a referência para usar no OnSelect

        if (txtNome != null) txtNome.text = skill.skillName;

        // Custo de Mana (Ex: "15 MP" ou apenas "15")
        if (txtMP != null) txtMP.text = skill.custoMana > 0 ? $"{skill.custoMana} MP" : "0 MP";

        // Turnos para Executar (Delay)
        if (txtDelay != null) txtDelay.text = $"DL: {skill.turnosParaExecutar}";

        // Turnos de Recuperação (Resting)
        if (txtResting != null) txtResting.text = $"RT: {skill.turnosRecuperacao}";
    }

    // Disparado automaticamente pelo EventSystem quando o botão ganha foco
    public void OnSelect(BaseEventData eventData)
    {
        if (menuPrincipal != null && skillArmazenada != null)
        {
            menuPrincipal.AtualizarPreviewDaSkill(skillArmazenada);
        }
    }

    // Disparado automaticamente pelo EventSystem quando o foco muda para outro lugar
    public void OnDeselect(BaseEventData eventData)
    {
        if (menuPrincipal != null)
        {
            menuPrincipal.LimparPreview();
        }
    }
}