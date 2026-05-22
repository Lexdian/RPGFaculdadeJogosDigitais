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
    public int ataqueFisico;
    public int ataqueMagico;
    public int defesaFisica;
    public int defesaMagica;
    public int evasao;
    public int agilidade;

    [Header("Comportamento")]
    public AbstractEnemyBehavior behavior;
}
