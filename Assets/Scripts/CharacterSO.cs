using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NovoPersonagem", menuName = "RPG/CharData")]
public class CharacterSO : ScriptableObject
{
    [Header("Identidade e Lore")]
    public string charName;
    public Sprite charPortrait; // Foto para menus/UI de turnos
    public Sprite charBattle;

    [Header("Arte e Animações (Mundo vs Batalha)")]
    public RuntimeAnimatorController overworldAnimator; // O que você já usava no mapa

    [Header("Status Iniciais (Nível 1)")]
    public int baseVida;
    public int baseMana;
    public int baseForca;
    public int baseInteligencia;
    public int baseAgilidade;
    public int baseResiliencia;
    public int baseSorte;

    [Header("Crescimento por Nível")]
    public int perLevelUpgradeVida;
    public int perLevelUpgradeMana;
    public int perLevelUpgradeForca;
    public int perLevelUpgradeInteligencia;
    public int perLevelUpgradeAgilidade;
    public int perLevelUpgradeResiliencia;
    public int perLevelUpgradeSorte;

    [Header("Progressão de Experiência")]
    public int xpBaseNecessario = 100;
    public float curvaXPMultiplicador = 1.2f; // Ex: Próximo nível pede 20% a mais de XP

    [Header("Combate e Habilidades")]
    // Referências para ScriptableObjects de habilidades que o herói conhece desde o início
    public List<SkillSO> habilidadesIniciais; 
    
    // Lista de habilidades aprendidas por nível (Estrutura explicada abaixo)
    public List<SkillAprendidaPorNivel> habilidadesPorNivel;
}

// Uma struct simples para o designer ditar qual skill abre em qual nível no Inspector
[System.Serializable]
public struct SkillAprendidaPorNivel
{
    public int nivelNecessario;
    public SkillSO habilidade;
}