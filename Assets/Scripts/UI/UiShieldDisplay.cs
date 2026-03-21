using UnityEngine;
using UnityEngine.UI;

public class UiShieldDisplay : UIelement
{
    public Image displayImage;
    public Sprite[] shieldSprites; // 0 = empty, 1 = 1 life, 2 = 2 lives
    private Health shieldHealth;

    private void Start()
    {
        UpdateUI();
    }

    public override void UpdateUI()
    {
        if (shieldHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Controller controller = player.GetComponent<Controller>();
                if (controller != null && controller.shieldObject != null)
                {
                    shieldHealth = controller.shieldObject.GetComponent<Health>();
                }
            }
        }

        if (shieldHealth == null || !shieldHealth.gameObject.activeSelf)
        {
            if (displayImage != null && shieldSprites.Length > 0)
            {
                displayImage.sprite = shieldSprites[0];
                displayImage.enabled = true;
                displayImage.gameObject.SetActive(true);
            }
            return;
        }

        int vidasAtuais = shieldHealth.currentLives;
        
        if (displayImage != null && shieldSprites != null && vidasAtuais < shieldSprites.Length)
        {
            displayImage.sprite = shieldSprites[vidasAtuais];
            displayImage.enabled = true;
            displayImage.gameObject.SetActive(true);
        }
        else if (displayImage != null && shieldSprites != null && shieldSprites.Length > 0)
        {
            displayImage.sprite = shieldSprites[shieldSprites.Length - 1];
            displayImage.enabled = true;
            displayImage.gameObject.SetActive(true);
        }
    }
}
