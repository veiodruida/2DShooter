using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIShieldSpriteDisplay : UIelement
{
    [Header("Configurações")]
    public Image displayImage;
    public List<Sprite> shieldSprites; // 0 (Vazio), 1, 2, 3 (Cheio)

    private Health shieldHealth;

    public override void UpdateUI()
    {
        // Debug para saber se o UIManager pelo menos chamou a função
       // Debug.Log("<color=orange>UpdateUI do Escudo foi CHAMADO!</color>");

        if (shieldHealth == null)
        {
            // Busca o ShieldController mesmo que esteja desativado
            ShieldController sc = FindFirstObjectByType<ShieldController>(FindObjectsInactive.Include);
            if (sc != null)
            {
                shieldHealth = sc.GetComponent<Health>();
            }
        }

        if (shieldHealth == null)
        {
        //    Debug.LogWarning("UI Escudo: Health do escudo não encontrado.");
            return;
        }

        if (displayImage == null) return;

        int vidasAtuais = shieldHealth.currentLives;

        // Regra visual: só mostra se o objeto do escudo estiver ATIVO na cena
        if (!shieldHealth.gameObject.activeInHierarchy || vidasAtuais <= 0)
        {
            displayImage.enabled = false;
        }
        else
        {
            displayImage.enabled = true;
            int index = Mathf.Clamp(vidasAtuais, 0, shieldSprites.Count - 1);
            displayImage.sprite = shieldSprites[index];
         //   Debug.Log("<color=cyan>UI Escudo atualizada para vida: </color>" + vidasAtuais);
        }
    }
}
