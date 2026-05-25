using System.Collections.Generic;
using UnityEngine;

public class BattleResult
{
    public bool vitoria;
    public int xpTotal;
    public List<CharacterBattleResult> personagens = new();
    public List<(ItemSO item, int quantidade)> drops = new();
}
