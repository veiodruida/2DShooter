using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public class HistoricoDisplay : MonoBehaviour
{
    public TextMeshProUGUI textoLista;

    void OnEnable()
    {
        MostrarHistorico();
    }

    void MostrarHistorico()
    {
        if (textoLista == null) return;

        string historicoRaw = PlayerPrefs.GetString("historico_partidas", "");

        if (string.IsNullOrEmpty(historicoRaw))
        {
            textoLista.text = "No Records Found";
            return;
        }

        var partidas = new List<(int pontos, string tempo, string dif)>();

        foreach (string entrada in historicoRaw.Split(','))
        {
            string e = entrada.Trim();
            if (string.IsNullOrEmpty(e)) continue;

            // Suporta novo formato (;) e formato legado (|)
            char sep = e.Contains(';') ? ';' : '|';
            string[] dados = e.Split(sep);

            if (dados.Length < 2) continue;

            int p = int.TryParse(dados[0].Trim(), out int parsed) ? parsed : 0;
            string t = dados.Length > 1 ? dados[1].Trim() : "0.00";
            string d = dados.Length > 2 ? dados[2].Trim() : "N/A";

            partidas.Add((p, t, d));
        }

        if (partidas.Count == 0)
        {
            textoLista.text = "No Records Found";
            return;
        }

        var top10 = partidas.OrderByDescending(x => x.pontos).Take(10).ToList();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<color=yellow> RANK | POINTS  |  TIME  | MODE</color>");
        sb.AppendLine("<color=white>----------------------------------</color>");

        for (int i = 0; i < top10.Count; i++)
        {
            var item = top10[i];
            string rank   = $"#{i + 1:D2}".PadRight(4);
            string points = item.pontos.ToString().PadLeft(6);
            string time   = item.tempo.PadLeft(6);
            string mode   = item.dif.PadRight(7);

            sb.AppendLine($" {rank} | {points} | {time}s | {mode}");
        }

        textoLista.text = sb.ToString();
    }

    public void LimparTudo()
    {
        PlayerPrefs.DeleteKey("historico_partidas");
        MostrarHistorico();
    }
}
