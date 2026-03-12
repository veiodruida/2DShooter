using UnityEngine;

public class PowerUpItem : MonoBehaviour
{
    public enum TipoPowerUp { Escudo, Vida, Tiro, Bomba }

    [Header("Configurações do Item")]
    public TipoPowerUp tipoStatus = TipoPowerUp.Escudo;
    public int quantidadeBase = 3;

    [Header("Efeitos Visuais")]
    public GameObject efeitoColeta;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Controller playerController = other.GetComponent<Controller>();
            Health playerHealth = other.GetComponent<Health>();
            ShootingController sc = other.GetComponent<ShootingController>();

            // CALCULA QUANTIDADE BASEADO NA DIFICULDADE
            int quantidadeFinal = CalcularQuantidadePelaDificuldade();

            switch (tipoStatus)
            {
                case TipoPowerUp.Escudo:
                    if (playerController != null)
                    {
                        playerController.GanharEscudo(quantidadeFinal);
                        FinalizarColeta();
                    }
                    break;

                case TipoPowerUp.Vida:
                    if (playerHealth != null && playerHealth.currentLives < playerHealth.maximumLives)
                    {
                        playerHealth.currentLives += quantidadeFinal;
                        if (playerHealth.currentLives > playerHealth.maximumLives)
                            playerHealth.currentLives = playerHealth.maximumLives;
                        FinalizarColeta();
                    }
                    break;

                case TipoPowerUp.Tiro:
                    if (sc != null)
                    {
                        sc.UpgradeWeapon();
                        FinalizarColeta();
                    }
                    break;

                case TipoPowerUp.Bomba:
                    if (playerController != null)
                    {
                        playerController.GanharBomba(1); // Bomba é sempre 1
                        FinalizarColeta();
                    }
                    break;
            }
        }
    }

    // Retorna a quantidade de recurso ajustada pela dificuldade do jogo
    private int CalcularQuantidadePelaDificuldade()
    {
        // Verifica se AMBAS as instâncias existem
        if (GameManager.instance == null || GameSettings.instance == null)
        {
            return quantidadeBase;
        }

        switch (GameSettings.instance.dificuldadeSelecionada)
        {
            case GameSettings.Dificuldade.Facil:
                return quantidadeBase + 1;
            case GameSettings.Dificuldade.Furia:
                return 1; // No modo Fúria, sobrevivência pura
            case GameSettings.Dificuldade.Dificil:
                return Mathf.Max(1, quantidadeBase - 1);
            default:
                return quantidadeBase;
        }
    }
    private void FinalizarColeta()
    {
        if (UIManager.instance != null) UIManager.instance.UpdateUI();
        if (efeitoColeta != null) Instantiate(efeitoColeta, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}