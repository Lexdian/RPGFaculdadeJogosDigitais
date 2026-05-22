using UnityEngine;

[System.Serializable]
public class CombatenteData
{
    public CharacterSO fichaBase;
    public int nivelAtual = 1;
    public int xpAtual = 0;

    // Status Dinâmicos (que mudam na batalha)
    public int vidaAtual;
    public int manaAtual;

    // Construtor para inicializar o personagem com status cheios
    public CombatenteData(CharacterSO baseData, int nivel)
    {
        this.fichaBase = baseData;
        this.nivelAtual = nivel;

        // Inicializa calculando o status baseado no nível
        this.vidaAtual = GetMaxVida();
        this.manaAtual = GetMaxMana();
    }

    // Fórmulas para calcular os status totais com base no nível atual
    public int GetMaxVida() => fichaBase.baseVida + (fichaBase.perLevelUpgradeVida * (nivelAtual - 1));
    public int GetMaxMana() => fichaBase.baseMana + (fichaBase.perLevelUpgradeMana * (nivelAtual - 1));
    public int GetForca() => fichaBase.baseForca + (fichaBase.perLevelUpgradeForca * (nivelAtual - 1));
    public int GetInteligencia() => fichaBase.baseInteligencia + (fichaBase.perLevelUpgradeInteligencia * (nivelAtual - 1));
    public int GetAgilidade() => fichaBase.baseAgilidade + (fichaBase.perLevelUpgradeAgilidade * (nivelAtual - 1));
    public int GetResiliencia() => fichaBase.baseResiliencia + (fichaBase.perLevelUpgradeResiliencia * (nivelAtual - 1));
    public int GetSorte() => fichaBase.baseSorte + (fichaBase.perLevelUpgradeSorte * (nivelAtual - 1));
}