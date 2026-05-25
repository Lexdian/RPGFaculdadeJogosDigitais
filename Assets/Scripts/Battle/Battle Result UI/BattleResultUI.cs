using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleResultUI : MonoBehaviour
{
    [Header("Painel de Vitória")]
    public GameObject painelVitoria;
    public TMP_Text textoXPTotal;
    public Transform containerPersonagens; // VerticalLayoutGroup
    public GameObject prefabEntryPersonagem;
    public Transform containerDrops;       // VerticalLayoutGroup
    public GameObject prefabEntryDrop;
    public TMP_Text textoSemDrops;

    [Header("Painel de Derrota")]
    public GameObject painelDerrota;

    [Header("Animação")]
    public float delayEntreEntries = 0.3f;

    public void MostrarDerrota()
    {
        gameObject.SetActive(true);
        painelVitoria.SetActive(false);
        painelDerrota.SetActive(true);
    }

    public void MostrarVitoria(BattleResult resultado)
    {
        gameObject.SetActive(true);
        painelDerrota.SetActive(false);
        painelVitoria.SetActive(true);

        if (textoXPTotal != null)
            textoXPTotal.text = $"+ {resultado.xpTotal} XP";

        LimparContainer(containerPersonagens);
        LimparContainer(containerDrops);

        StartCoroutine(PopularResultados(resultado));
    }

    private IEnumerator PopularResultados(BattleResult resultado)
    {
        foreach (var rp in resultado.personagens)
        {
            if (prefabEntryPersonagem == null || containerPersonagens == null) break;
            var go = Instantiate(prefabEntryPersonagem, containerPersonagens);
            go.GetComponent<EntryPersonagemUI>()?.Setup(rp);
            yield return new WaitForSeconds(delayEntreEntries);
        }

        yield return new WaitForSeconds(0.2f);

        if (resultado.drops.Count == 0)
        {
            if (textoSemDrops != null) textoSemDrops.gameObject.SetActive(true);
        }
        else
        {
            if (textoSemDrops != null) textoSemDrops.gameObject.SetActive(false);
            foreach (var (item, qty) in resultado.drops)
            {
                if (prefabEntryDrop == null || containerDrops == null) break;
                var go = Instantiate(prefabEntryDrop, containerDrops);
                go.GetComponent<EntryDropUI>()?.Setup(item, qty);
                yield return new WaitForSeconds(delayEntreEntries * 0.5f);
            }
        }
    }

    private void LimparContainer(Transform container)
    {
        if (container == null) return;
        foreach (Transform filho in container)
            Destroy(filho.gameObject);
    }

    // Testar sem comabte!
    [ContextMenu("Testar Vitória")]
    private void TestarVitoria()
    {
        var resultado = new BattleResult { vitoria = true, xpTotal = 350 };

        resultado.personagens.Add(new CharacterBattleResult
        {
            xpGanho = 350,
            subiuDeNivel = true,
            nivelAnterior = 3,
            nivelAtual = 4,
            habilidadesAprendidas = new List<SkillSO>()
        });

        resultado.drops.Add((null, 2)); // sem item real por enquanto

        MostrarVitoria(resultado);
    }

    [ContextMenu("Testar Derrota")]
    private void TestarDerrota()
    {
        MostrarDerrota();
    }
}