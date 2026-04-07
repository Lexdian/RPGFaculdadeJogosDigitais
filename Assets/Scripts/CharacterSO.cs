using UnityEngine;

[CreateAssetMenu(fileName = "NovoPersonagem", menuName = "RPG/BaseData")]
public class CharacterSO : ScriptableObject
{
    public string charName;
    public RuntimeAnimatorController animatorOverride;
    public Sprite charPortrait;

    [Header("Status Iniciais")]
    public int baseVida;
    public int baseMana;
    public int baseForca;
    public int baseInteligencia;
    public int baseAgilidade;
    public int baseResiliencia;
    public int baseSorte;

    [Header("Aumento de Status por Nével")]
    public int perLevelUpgradeVida;
    public int perLevelUpgradeMana;
    public int perLevelUpgradeForca;
    public int perLevelUpgradeInteligencia;
    public int perLevelUpgradeAgilidade;
    public int perLevelUpgradeResiliencia;
    public int perLevelUpgradeSorte;
}
