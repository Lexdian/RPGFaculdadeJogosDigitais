using System.Collections.Generic;

public class CharacterBattleResult
{
    public CharEntity entidade;
    public int xpGanho;
    public bool subiuDeNivel;
    public int nivelAnterior;
    public int nivelAtual;
    public List<SkillSO> habilidadesAprendidas = new();
}
