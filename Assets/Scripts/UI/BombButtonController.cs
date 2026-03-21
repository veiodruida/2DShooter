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

        // Configura o clique do botao
        if (botaoBomba != null)
        {
            botaoBomba.onClick.AddListener(TentarDispararBomba);
        }


    }

    void Update()
    {
        if (scriptBomba == null) return;

        // Gerir visual do botao: fica cinzento (desativado) se nao houver bombas
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
            // Chamamos o metodo publico que ja criaste no teu script!
            // Como o LancarNucleoBomba e privado no teu codigo original, 
            // vamos precisar de uma pequena alteracao la ou criar um metodo publico.
            scriptBomba.DispararBombaPeloBotao();
        }
    }
}
