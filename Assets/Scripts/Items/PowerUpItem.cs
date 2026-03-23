using UnityEngine;

public class PowerUpItem : MonoBehaviour
{
    public enum TipoPowerUp { Escudo, Vida, Tiro, Bomba }

    [Header("Configurações do Item")]
    public TipoPowerUp tipoStatus = TipoPowerUp.Escudo;
    public int quantidadeBase = 3;

    [Header("Efeitos Visuais")]
    public GameObject efeitoColeta;
    public Color corDasParticulas = Color.white;

    [Header("Configuração de Áudio (2D)")]
    [Tooltip("Coloque aqui o som específico deste prefab no Inspector.")]
    public AudioClip somColeta;
    [Range(0f, 1f)] public float volumeColeta = 1.0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Obtém os componentes do jogador
            Controller playerController = other.GetComponent<Controller>();
            Health playerHealth = other.GetComponent<Health>();
            ShootingController sc = other.GetComponent<ShootingController>();

            int quantidadeFinal = CalcularQuantidadePelaDificuldade();
            bool podeColetar = false;

            // Executa a lógica baseada no tipo de item
            switch (tipoStatus)
            {
                case TipoPowerUp.Escudo:
                    if (playerController != null)
                    {
                        playerController.GanharEscudo(quantidadeFinal);
                        podeColetar = true;
                    }
                    break;

                case TipoPowerUp.Vida:
                    if (playerHealth != null && playerHealth.currentLives < playerHealth.maximumLives)
                    {
                        playerHealth.currentLives += quantidadeFinal;
                        // Clamp manual para não estourar o limite
                        if (playerHealth.currentLives > playerHealth.maximumLives)
                            playerHealth.currentLives = playerHealth.maximumLives;

                        podeColetar = true;
                    }
                    break;

                case TipoPowerUp.Tiro:
                    if (sc != null)
                    {
                        sc.UpgradeWeapon();
                        podeColetar = true;
                    }
                    break;

                case TipoPowerUp.Bomba:
                    if (playerController != null)
                    {
                        playerController.GanharBomba(1);
                        podeColetar = true;
                    }
                    break;
            }

            // Se a lógica do item foi aplicada, finaliza a coleta
            if (podeColetar)
            {
                FinalizarColeta();
            }
        }
    }

    private void FinalizarColeta()
    {
        // 1. Som (Usando o seu Audio2DManager)
        if (somColeta != null)
        {
            Audio2DManager.Play2D(somColeta, volumeColeta);
        }

        // 2. Interface
        if (UIManager.instance != null) UIManager.instance.UpdateUI();

        // 3. Partículas com cor dinâmica
        if (efeitoColeta != null)
        {
            GameObject sistemaGo = Instantiate(efeitoColeta, transform.position, Quaternion.identity);
            ParticleSystem ps = sistemaGo.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = new ParticleSystem.MinMaxGradient(corDasParticulas);
            }
            Destroy(sistemaGo, 2f);
        }

        // 4. Remove o item
        Destroy(gameObject);
    }

    private int CalcularQuantidadePelaDificuldade()
    {
        if (GameSettings.instance == null) return quantidadeBase;

        switch (GameSettings.instance.dificuldadeSelecionada)
        {
            case GameSettings.Dificuldade.Facil:
                return quantidadeBase + 1;

            case GameSettings.Dificuldade.Furia:
                return 1;

            case GameSettings.Dificuldade.Dificil:
                return Mathf.Max(1, quantidadeBase - 1);

            default:
                return quantidadeBase;
        }
    }
}