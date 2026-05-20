using UnityEngine;

// Enums para categorizar as regras da habilidade
public enum TipoAlvo { UnicoInimigo, TodosInimigos, UnicoAliado, TodosAliados, OProprio }
public enum TipoDano { Fisico, Fogo, Gelo, Eletrico, Sombrio, Luz, Agua, Vento, Terra, Cura, Suporte }
public enum Elemento { Neutro, Fogo, Gelo, Eletrico, Sombrio }

[CreateAssetMenu(fileName = "NovaHabilidade", menuName = "RPG/Habilidade")]
public class SkillSO : ScriptableObject
{
    [Header("Identidade")]
    public string skillName;
    [TextArea(2, 3)] public string description;
    public Sprite icon; // Ícone para o menu de batalha

    [Header("Regras de Alvo e Tipo")]
    public TipoAlvo alvo;
    public TipoDano tipoDano;
    public Elemento elemento;

    [Header("Custos e Valores")]
    public int custoMana;
    public int poderBase; // Usado para cálculo de dano ou cura
    [Range(0f, 1f)] public float chanceAcerto = 1f; // 1 = 100% de chance

    [Header("Tempo")]
    [Tooltip("Quantos segmentos/ticks o personagem precisa esperar após selecionar a skill para ela EXECUTAR.")]
    public int segmentosParaCast;

    [Tooltip("Modificador de recuo. Ex: Um golpe pesado pode fazer o próximo turno demorar mais (muda o tempo inicial pós-execução).")]
    public float penalidadeTempoPosUso = 0f;

    [Header("Efeitos Visuais e Sonoros")]
    public string gatilhoAnimacao = "CastSkill"; // Nome do Trigger no Animator do personagem
    public GameObject efeitoVisualPrefab; // Prefab de partículas (ex: fogo estourando no inimigo)
    public AudioClip somExecucao;
}