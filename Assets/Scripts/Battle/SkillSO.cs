using UnityEngine;
using System.Collections.Generic;

// Enums para categorizar as regras da habilidade
public enum TipoAlvo { UnicoInimigo, TodosInimigos, UnicoAliado, TodosAliados, OProprio }
public enum CategoriaHabilidade { Fisico, Magia, Cura, Suporte }
public enum TipoDano { Normal, Fogo, Gelo, Eletrico, Sombrio, Luz, Agua, Vento, Terra}
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
    public CategoriaHabilidade categoria;
    public TipoDano tipoDano;
    public Elemento elemento;
    public List<AbstactSkillEfect> efeitosExtras;

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
    public GameObject efeitoVisualPrefab; // Prefab de partículas (ex: fogo estourando no inimigo)
    public AudioClip somExecucao;
}