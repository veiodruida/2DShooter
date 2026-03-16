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
        // Verifica se quem colidiu foi o jogador
        if (other.CompareTag("Player"))
        {
            // Tenta obter todos os componentes necessários do jogador
            Controller playerController = other.GetComponent<Controller>();
            Health playerHealth = other.GetComponent<Health>();
            ShootingController sc = other.GetComponent<ShootingController>();

            // CALCULA QUANTIDADE BASEADO NA DIFICULDADE (Usando a tua lógica original)
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
                    // Só coleta se o jogador não estiver com vidas no máximo
                    if (playerHealth != null && playerHealth.currentLives < playerHealth.maximumLives)
                    {
                        playerHealth.currentLives += quantidadeFinal;

                        // Garante que não ultrapassa o limite máximo definido no Health
                        if (playerHealth.currentLives > playerHealth.maximumLives)
                            playerHealth.currentLives = playerHealth.maximumLives;

                        FinalizarColeta();
                    }
                    break;

                case TipoPowerUp.Tiro:
                    if (sc != null)
                    {
                        // Chama a evolução da arma (que bloqueia no nível 3 automaticamente)
                        sc.UpgradeWeapon();
                        FinalizarColeta();
                    }
                    break;

                case TipoPowerUp.Bomba:
                    if (playerController != null)
                    {
                        playerController.GanharBomba(1); // Bomba é sempre 1 conforme o teu padrão
                        FinalizarColeta();
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Retorna a quantidade de recurso ajustada pela dificuldade do jogo.
    /// Mantém a lógica: Fácil (+1), Difícil (-1), Fúria (Sempre 1).
    /// </summary>
    private int CalcularQuantidadePelaDificuldade()
    {
        // Verifica se as instâncias globais existem para evitar erros
        if (GameManager.instance == null || GameSettings.instance == null)
        {
            return quantidadeBase;
        }

        switch (GameSettings.instance.dificuldadeSelecionada)
        {
            case GameSettings.Dificuldade.Facil:
                return quantidadeBase + 1;

            case GameSettings.Dificuldade.Furia:
                return 1; // No modo Fúria, a sobrevivência é mínima

            case GameSettings.Dificuldade.Dificil:
                // Garante que nunca retorna menos de 1
                return Mathf.Max(1, quantidadeBase - 1);

            default:
                return quantidadeBase;
        }
    }

    /// <summary>
    /// Atualiza UI, cria efeitos visuais e remove o item da cena.
    /// </summary>
    private void FinalizarColeta()
    {
        // Força a atualização da interface (Vidas, Escudos, Bombas)
        if (UIManager.instance != null) UIManager.instance.UpdateUI();

        // Efeito de partículas ou som
        if (efeitoColeta != null) Instantiate(efeitoColeta, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}