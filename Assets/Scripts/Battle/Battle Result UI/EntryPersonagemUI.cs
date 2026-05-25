using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntryPersonagemUI : MonoBehaviour
{
    public Image imgPortrait;
    public TMP_Text txtNome;
    public TMP_Text txtXP;
    public TMP_Text txtNivel;
    public GameObject painelLevelUp;
    public TMP_Text txtLevelUp;
    public TMP_Text txtHabilidadesAprendidas; // opcional

    public void Setup(CharacterBattleResult rp)
    {
        if (rp == null) return;

        if (imgPortrait != null && rp.entidade?.Data?.fichaBase?.charPortrait != null)
            imgPortrait.sprite = rp.entidade.Data.fichaBase.charPortrait;

        if (txtNome != null)
            txtNome.text = rp.entidade?.EntityName ?? "???";

        if (txtXP != null)
            txtXP.text = $"+{rp.xpGanho} XP";

        if (txtNivel != null)
            txtNivel.text = $"Nível {rp.nivelAtual}";

        if (painelLevelUp != null)
        {
            painelLevelUp.SetActive(rp.subiuDeNivel);
            if (rp.subiuDeNivel && txtLevelUp != null)
                txtLevelUp.text = $"⬆ LEVEL UP!  Lv.{rp.nivelAnterior} → Lv.{rp.nivelAtual}";
        }

        if (txtHabilidadesAprendidas != null)
        {
            bool temNovas = rp.habilidadesAprendidas?.Count > 0;
            txtHabilidadesAprendidas.gameObject.SetActive(temNovas);
            if (temNovas)
            {
                var nomes = string.Join(", ", rp.habilidadesAprendidas.ConvertAll(s => s.skillName));
                txtHabilidadesAprendidas.text = $"Aprendeu: {nomes}";
            }
        }
    }
}