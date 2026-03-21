using UnityEngine;
using TMPro;
using System.Linq;

public class HistoricoDisplay : MonoBehaviour
{
    public TextMeshProUGUI textoLista;

    void Start()
    {
        MostrarHistorico();
    }

    void MostrarHistorico()
    {
        string historicoRaw = PlayerPrefs.GetString("historico_partidas", "");

        if (string.IsNullOrEmpty(historicoRaw))
        {
            textoLista.text = "No Records Found";
            return;
        }

        string[] entradas = historicoRaw.Split(',');
        string textoFormatado = " RANK   |   POINTS   |   TIME \n";
        textoFormatado += "----------------------------------\n";

        for (int i = 0; i < entradas.Length; i++)
        {
            if (string.IsNullOrEmpty(entradas[i])) continue;

            string[] dados = entradas[i].Split('|');

            // Se a entrada estiver mal formatada, ignora para nao dar erro
            if (dados.Length < 2) continue;

            string pontos = dados[0];
            string tempo = dados[1];

            textoFormatado += $" #{i + 1:D2}    |   {pontos.PadLeft(5)} pts   |   {tempo}s\n";
        }

        textoLista.text = textoFormatado;
    }

    public void LimparTudo()
    {
        PlayerPrefs.DeleteKey("historico_partidas");
        MostrarHistorico();
    }
}
