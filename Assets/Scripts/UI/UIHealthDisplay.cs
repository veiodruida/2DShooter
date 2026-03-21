using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Note que usamos UIelement em vez de MonoBehaviour
public class UIHealthDisplay : UIelement
{
    [Header("Referencias")]
    public Health playerHealth; // O script de vida do seu Player
    public List<Image> hearts;  // As imagens dos corações que você criou

    [Header("Sprites")]
    public Sprite fullHeart;    // Imagem do coração cheio
    public Sprite emptyHeart;   // Imagem do coração vazio

    // Essa função é o que o seu UIManager chama automaticamente
    public override void UpdateUI()
    {
        // Se a referência estiver vazia, tenta encontrar o jogador pela Tag
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
        }

        if (playerHealth == null) return;

        for (int i = 0; i < hearts.Count; i++)
        {
            // Se o índice for menor que a vida atual, fica cheio, senão, vazio
            if (i < playerHealth.currentLives)
                hearts[i].sprite = fullHeart;
            else
                hearts[i].sprite = emptyHeart;
        }
    }
}
