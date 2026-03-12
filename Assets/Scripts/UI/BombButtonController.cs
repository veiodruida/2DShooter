using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BombButtonController : MonoBehaviour
{
    public Button botaoBomba;
    public TextMeshProUGUI textoQuantidade;
    private ScreenClearBomb scriptBomba;

    void Start()
    {
        // Procura o script ScreenClearBomb no Jogador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            scriptBomba = player.GetComponent<ScreenClearBomb>();
        }

        // Configura o clique do botão
        if (botaoBomba != null)
        {
            botaoBomba.onClick.AddListener(TentarDispararBomba);
        }


    }

    void Update()
    {
        if (scriptBomba == null) return;

        // Gerir visual do botão: fica cinzento (desativado) se não houver bombas
        botaoBomba.interactable = (scriptBomba.bombasAtuais > 0);

        // Atualiza o contador de texto (ex: "3")
        if (textoQuantidade != null)
        {
            //textoQuantidade.text = scriptBomba.bombasAtuais.ToString();
            textoQuantidade.text = "BOMB!!!";
        }
    }

    void TentarDispararBomba()
    {
        if (scriptBomba != null && scriptBomba.bombasAtuais > 0)
        {
            // Chamamos o método público que já criaste no teu script!
            // Como o LancarNucleoBomba é privado no teu código original, 
            // vamos precisar de uma pequena alteração lá ou criar um método público.
            scriptBomba.DispararBombaPeloBotao();
        }
    }
}