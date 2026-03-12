using UnityEngine;

/// <summary>
/// Elemento de UI que gerencia a exibição visual das bombas
/// </summary>
public class UIBombDisplay : UIelement
{
    [Header("Configurações das Bombas")]
    public GameObject bombIconPrefab; // O prefab com o Image da bomba
    public Transform container;      // O objeto com o Horizontal Layout Group

    /// <summary>
    /// Sobrescreve o método UpdateUI que o seu UIManager chama
    /// </summary>
    public override void UpdateUI()
    {
        // 1. Verifica se o Player e o script de bomba existem
        if (GameManager.instance == null || GameManager.instance.player == null || container == null)
            return;

        ScreenClearBomb bombScript = GameManager.instance.player.GetComponent<ScreenClearBomb>();
        if (bombScript == null) return;

        // 2. Sincroniza a quantidade de ícones com a quantidade de bombas
        int iconesAtuais = container.childCount;
        int bombasParaExibir = bombScript.bombasAtuais;

        // Adiciona ícones se faltarem
        if (iconesAtuais < bombasParaExibir)
        {
            for (int i = 0; i < bombasParaExibir - iconesAtuais; i++)
            {
                Instantiate(bombIconPrefab, container);
            }
        }
        // Remove ícones se sobrarem
        else if (iconesAtuais > bombasParaExibir)
        {
            for (int i = 0; i < iconesAtuais - bombasParaExibir; i++)
            {
                Destroy(container.GetChild(container.childCount - 1).gameObject);
            }
        }
    }
}