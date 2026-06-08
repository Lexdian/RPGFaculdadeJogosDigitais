using System.Collections.Generic;
using UnityEngine;

public enum EnemySize { Pequeno, Grande }

[CreateAssetMenu(fileName = "Enemy", menuName = "RPG/EnemyData")]
public class EnemySO : ScriptableObject
{
    public string enemyName;
    public Sprite enemySprite;
    public Sprite enemyIcon;
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
    public int velocidade;
    public int precisao;
    public int evasao;
    public int chanceCritico;

    public int xpReward;


    [Header("Infos Ataque base")]
    public int delay;
    public int recuperacao;
    public TipoAlvo tipoAlvo;
    public TipoDano dano;
    public GameObject prefabEfeitoVisual;

    public List<SkillSO> Skills = new();

    [Header("Comportamento")]
    [SerializeReference, SubclassSelector]
    public AbstractEnemyBehavior behavior;
}
