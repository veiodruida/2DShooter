using UnityEngine;

public class EnemyContact : MonoBehaviour
{
    [Header("Configurações de Dano")]
    public int danoNoPlayer = 1;
    public int danoNoInimigoAoBater = 1; // Quanto de vida o próprio inimigo perde no impacto

    // Esta função detecta quando o inimigo encosta em algo
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Tenta encontrar o script Health no objeto que o inimigo bateu
        Health saudeDoPlayer = collision.GetComponentInParent<Health>();

        // 2. Se não achou no pai, tenta no próprio objeto
        if (saudeDoPlayer == null) saudeDoPlayer = collision.GetComponent<Health>();

        // 3. Se achou o script Health e o TeamId for 0 (Player)
        if (saudeDoPlayer != null && saudeDoPlayer.teamId == 0)
        {
            // Verificamos o escudo shield_buble primeiro
            Transform escudo = collision.transform.Find("shield_buble");

            if (escudo != null && escudo.gameObject.activeSelf)
            {
                // Se o escudo estiver ativo, ele explode e o player não perde vida
                escudo.gameObject.SetActive(false);
                Debug.Log("Inimigo bateu no Escudo!");
            }
            else
            {
                // Se estiver sem escudo, o player leva 1 de dano
                saudeDoPlayer.TakeDamage(danoNoPlayer);
                Debug.Log("Inimigo bateu no Player e causou dano!");
            }

            // 2. DANO NO PRÓPRIO INIMIGO (Em vez de Destroy imediato)
            Health minhaSaude = GetComponent<Health>(); // Tenta pegar o script Health do próprio inimigo

            if (minhaSaude != null)
            {
                // O inimigo leva dano por ter batido no jogador
                minhaSaude.TakeDamage(danoNoInimigoAoBater);
                Debug.Log("Inimigo levou dano pelo impacto!");
            }
            else
            {
                // Caso o inimigo não tenha um script Health, ele ainda se destrói
                // (Isso evita que inimigos "imortais" fiquem colidindo sem parar)
                Destroy(gameObject);
            }
        }
    }

}