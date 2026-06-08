using UnityEngine;
using System.Collections.Generic;

// Enums para categorizar as regras da habilidade
public enum TipoAlvo { Unico, Grupo}
public enum CategoriaHabilidade { Fisico, Magia, Cura, Suporte }
public enum TipoDano { Normal, Fogo, Gelo, Eletrico, Sombrio, Luz, Agua, Vento, Terra}
public enum Prioridade { Aliados, Inimigos }

[CreateAssetMenu(fileName = "NovaHabilidade", menuName = "RPG/Habilidade")]
public class SkillSO : ScriptableObject
{
    [Header("Identidade")]
    public string skillName;
    [TextArea(2, 3)] public string description;

    [Header("Regras de Alvo e Tipo")]
    public TipoAlvo alvo;
    public CategoriaHabilidade categoria;
    public TipoDano tipoDano;
    public Prioridade prioridade;
    public bool podeSerUsadaEmMortos;
    public List<StatusEffectSO> efeitosExtras;

    [Header("Custos e Valores")]
    public int custoMana;
    public int poderBase; // Usado para cálculo de dano ou cura
    [Range(0f, 1f)] public float chanceAcerto = 1f; // 1 = 100% de chance

    [Header("Tempo")]
    [Tooltip("Quantos segmentos/ticks o personagem precisa esperar após selecionar a skill para ela EXECUTAR.")]
    public int turnosParaExecutar;

    [Tooltip("Modificador de recuo. Ex: Um golpe pesado pode fazer o próximo turno demorar mais (muda o tempo inicial pós-execução).")]
    public int turnosRecuperacao = 0;

    [Header("Efeitos Visuais e Sonoros")]
    public string gatilhoAnimacao = "CastSkill"; // Nome do Trigger no Animator do personagem
    public AudioClip somExecucao;
    public GameObject prefabEfeitoVisual; // O prefab que tem o script EfeitoMagia

    /// <summary>
    /// Nosso "Construtor" customizado e seguro para o Unity.
    /// </summary>
    public static SkillSO AtaqueBasico(
        int delay,
        int recuperacao,
        TipoAlvo tipoAlvo,
        TipoDano dano)
    {
        // 1. Instancia corretamente na memória do Unity
        SkillSO novaSkill = ScriptableObject.CreateInstance<SkillSO>();

        // 2. Inicializa os campos desejados
        novaSkill.skillName = "Ataque Básico";
        novaSkill.custoMana = 0;
        novaSkill.poderBase = 10;
        novaSkill.turnosParaExecutar = delay;
        novaSkill.turnosRecuperacao = recuperacao;
        novaSkill.alvo = tipoAlvo;
        novaSkill.categoria = CategoriaHabilidade.Fisico;
        novaSkill.tipoDano = dano;
        novaSkill.prioridade = Prioridade.Inimigos;

        // Inicializações padrão para evitar NullReference comuns
        novaSkill.efeitosExtras = new List<StatusEffectSO>();
        novaSkill.chanceAcerto = 1f;
        novaSkill.gatilhoAnimacao = "CastSkill";

        return novaSkill;
    }
}