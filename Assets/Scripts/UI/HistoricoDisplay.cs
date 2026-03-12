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
            string[] dados = entradas[i].Split('|'); // Separa Pontos de Tempo
            string pontos = dados[0];
            string tempo = dados[1];

            // Formata a linha: #1 | 5000 pts | 45.20s
            textoFormatado += $" #{i + 1:D2}    |   {pontos} pts   |   {tempo}s\n";
        }

        textoLista.text = textoFormatado;
    }

    public void LimparTudo()
    {
        PlayerPrefs.DeleteKey("historico_partidas");
        MostrarHistorico();
    }
}