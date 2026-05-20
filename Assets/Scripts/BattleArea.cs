using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public struct EnemyGroup
{
    public EnemySO[] colunaFrente;  // Coluna 1
    public EnemySO[] colunaMeio;    // Coluna 2
    public EnemySO[] colunaTras;    // Coluna 3

    [Range(1, 5)]
    public int rarity;

    // A validação agora deve checar cada array individualmente
    public bool IsValid(out string error)
    {
        error = "";
        return ValidarColuna(colunaFrente, "Frente", out error) &&
               ValidarColuna(colunaMeio, "Meio", out error) &&
               ValidarColuna(colunaTras, "Trás", out error);
    }

    private bool ValidarColuna(EnemySO[] coluna, string nome, out string error)
    {
        error = "";
        if (coluna == null || coluna.Length == 0) return true;

        bool temGrande = coluna.Any(e => e != null && e.tamanho == EnemySize.Grande);
        bool temPequeno = coluna.Any(e => e != null && e.tamanho == EnemySize.Pequeno);

        if (temGrande && temPequeno)
        {
            error = $"Coluna {nome} mista! Não pode ter grandes e pequenos juntos.";
            return false;
        }

        if (temGrande && coluna.Count(e => e != null) > 2)
        {
            error = $"Coluna {nome} excedeu limite de 2 grandes.";
            return false;
        }

        if (temPequeno && coluna.Count(e => e != null) > 3)
        {
            error = $"Coluna {nome} excedeu limite de 3 pequenos.";
            return false;
        }

        return true;
    }
}
public class BattleArea : MonoBehaviour
{
    [SerializeField]
    public EnemyGroup[] enemyGroups;

    private EnemyGroup[] currentEnemyGroups;

    public int EncounterChance = 20;
    private float encounterTimer = 0f;
    private float nextEncounterThreshold = 2f; // Tempo mínimo entre tentativas

    private void Awake()
    {
        AtualizarGruposValidos();
    }

    private void OnValidate()
    {
        List<EnemyGroup> validGroups = new List<EnemyGroup>();
        foreach (var group in enemyGroups)
        {
            string error;
            if (!group.IsValid(out error))
            {
                Debug.LogError($"<color=red>Erro no Grupo de Inimigos:</color> {error}");
            }
            else
            {
                validGroups.Add(group);
            }
        }
        currentEnemyGroups = validGroups.ToArray();
    }

    private EnemyGroup GetRandomEnemyGroup()
    {
        if (currentEnemyGroups == null || currentEnemyGroups.Length == 0)
        {
            Debug.LogWarning("No enemy groups defined in BattleArea.");
            return default;
        }
        int totalRarity = 0;
        foreach (var group in currentEnemyGroups)
        {
            totalRarity += (6 - group.rarity);
        }

        int randomValue = UnityEngine.Random.Range(0, totalRarity);
        int cumulativeRarity = 0;

        foreach (var group in currentEnemyGroups)
        {
            cumulativeRarity += (6 - group.rarity);
            if (randomValue < cumulativeRarity)
            {
                return group;
            }
        }
        return currentEnemyGroups[currentEnemyGroups.Length - 1];
    }
    

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var player = collision.GetComponent<LiderCharacter>();

            if (player.Dir != Vector2.zero)
            {
                encounterTimer += Time.deltaTime;

                if (encounterTimer >= nextEncounterThreshold)
                {
                    encounterTimer = 0f;

                    if (UnityEngine.Random.Range(0, 100) < EncounterChance)
                    {
                        EnemyGroup selectedGroup = GetRandomEnemyGroup();

                        Debug.Log("Selected group: " + selectedGroup);
                        // Passa os dados para o GameManager
                        GameManager.Instance.inimigosParaSpawnar = selectedGroup;

                        Debug.Log("Saindo do mapa para a batalha...");
                        SceneManager.LoadScene("BattleScene");
                    }
                }
            }
        }
    }

    private void AtualizarGruposValidos()
    {
        if (enemyGroups == null) return;

        List<EnemyGroup> validGroups = new List<EnemyGroup>();
        foreach (var group in enemyGroups)
        {
            string error;
            if (group.IsValid(out error))
            {
                validGroups.Add(group);
            }
        }
        currentEnemyGroups = validGroups.ToArray();
    }
}
