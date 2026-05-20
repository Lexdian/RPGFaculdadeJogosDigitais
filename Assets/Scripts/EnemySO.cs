using UnityEngine;

public enum EnemySize { Pequeno, Grande }

[CreateAssetMenu(fileName = "Enemy", menuName = "RPG/EnemyData")]
public class EnemySO : ScriptableObject
{
    public string enemyName;
    public Sprite enemySprite;
    public EnemySize tamanho;
    public bool isVoador;

    public int PesoEmSlots => (tamanho == EnemySize.Grande) ? 3 : 1;

    [Header("Atributos")]
    public int vida;
    public int mana;
    public int forca;
    public int inteligencia;
    public int agilidade;
    public int resiliencia;
    public int sorte;

}
