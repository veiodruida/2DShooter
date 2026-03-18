using UnityEngine;

public class PowerUpItem : MonoBehaviour
{
    public enum TipoPowerUp { Escudo, Vida, Tiro, Bomba }

    [Header("Configurações do Item")]
    public TipoPowerUp tipoStatus = TipoPowerUp.Escudo;
    public int quantidadeBase = 3;

    [Header("Efeitos Visuais")]
    public GameObject efeitoColeta;

    [Header("Efeitos Sonoros")]
    public AudioClip somColetaEscudo;
    public AudioClip somColetaVida;
    public AudioClip somColetaTiro;
    public AudioClip somColetaBomba;
    [Range(0f, 2f)]
    public float volumeColeta = 1.5f; // Aumentado de 1.0 para 1.5

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
                        TocarSomColeta(somColetaEscudo);
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

                        TocarSomColeta(somColetaVida);
                        FinalizarColeta();
                    }
                    break;

                case TipoPowerUp.Tiro:
                    if (sc != null)
                    {
                        // Chama a evolução da arma (que bloqueia no nível 3 automaticamente)
                        sc.UpgradeWeapon();
                        TocarSomColeta(somColetaTiro);
                        FinalizarColeta();
                    }
                    break;

                case TipoPowerUp.Bomba:
                    if (playerController != null)
                    {
                        // Toca o som ANTES de adicionar a bomba para garantir execução
                        TocarSomColeta(somColetaBomba);
                        playerController.GanharBomba(1); // Bomba é sempre 1 conforme o teu padrão
                        FinalizarColeta();
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Retorna a quantidade de recurso ajustada pela dificuldade do jogo.
    /// Lógica: Fácil (+1), Médio (base), Difícil (-1 mínimo 1), Fúria (1).
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

            case GameSettings.Dificuldade.Medio:
                return quantidadeBase; // Quantidade base sem modificações

            case GameSettings.Dificuldade.Dificil:
                return Mathf.Max(1, quantidadeBase - 1);

            case GameSettings.Dificuldade.Furia:
                return 1; // No modo Fúria, a sobrevivência é mínima

            default:
                return quantidadeBase;
        }
    }

    /// <summary>
    /// Toca o som do powerup coletado de forma robusta.
    /// Usa uma posição fixa para garantir que o som toque mesmo que a câmera esteja em movimento.
    /// </summary>
    private void TocarSomColeta(AudioClip clip)
    {
        if (clip == null) return;

        // Usa sempre a posição da câmera para evitar atenuação por distância
        Vector3 posicaoAudio = (Camera.main != null) ? Camera.main.transform.position : transform.position;
        
        // PlayClipAtPoint cria um AudioSource temporário que toca e se destrói automaticamente
        AudioSource.PlayClipAtPoint(clip, posicaoAudio, volumeColeta);

        Debug.Log($"[POWERUP SOM] Som coletado: {clip.name} | Volume: {volumeColeta}");
    }

    /// <summary>
    /// Atualiza UI, cria efeitos visuais e remove o item da cena.
    /// O som agora é tocado ANTES dessa função.
    /// </summary>
    private void FinalizarColeta()
    {
        // Força a atualização da interface (Vidas, Escudos, Bombas)
        if (UIManager.instance != null) UIManager.instance.UpdateUI();

        // Efeito de partículas
        if (efeitoColeta != null) Instantiate(efeitoColeta, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}