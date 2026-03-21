using UnityEngine;

public class ShieldController : MonoBehaviour
{
    private Health health;

    // Mudamos para OnEnable para garantir que ele reseta sempre que o item e pego
    public void ActivarEscudo()
    {
        // Se ainda nao temos a referencia do Health, procuramos agora
        if (health == null) health = GetComponent<Health>();

        // 1. Ativamos o objeto primeiro
        gameObject.SetActive(true);

        // 2. Resetamos a vida imediatamente
        if (health != null)
        {
            //health.SetVidaManual(3); // Define 3 vidas
            health.currentLives = health.maximumLives; // Restaura para o maximo
            health.currentHealth = health.defaultHealth; // Restaura a vida atual para o valor padrao
        }
        if (UIManager.instance != null) UIManager.instance.UpdateUI();

        Debug.Log("<color=cyan>ESCUDO REATIVADO!</color> vida: "+ health.currentLives);
    }
}
