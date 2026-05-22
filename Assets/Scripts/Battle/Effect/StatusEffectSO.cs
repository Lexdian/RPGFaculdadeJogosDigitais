using UnityEngine;

public enum TipoDanoStatus { Nenhum, Normal, Fogo, Gelo, Eletrico, Sombrio, Luz, Agua, Vento, Terra }

public enum DuracaoStatus { Combate, Persistente };

public abstract class StatusEffectSO : ScriptableObject
{
    [Header("Identidade")]
    public string effectName;
    [TextArea(2,3)] public string effectDescription;
    public Sprite icon;

    [Header("Regras")]
    public DuracaoStatus duracaoStatus;
    public TipoDanoStatus tipoDano;
    public int valorDano;
    public int duracao;

    [Header("Efeitos Visuais e Sonoros")]
    public string gatilhoAnimacao = "StatusEffect";
    public GameObject efeitoVisual;

    public abstract void OnApply(BattleEntity target, BattleEntity source);

    public abstract void OnTick(BattleEntity target, StatusEffectInstance instance);

    public abstract void OnExpire(BattleEntity target);

    public virtual bool IsStackable => false;
}
