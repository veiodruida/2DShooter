using UnityEngine;

public class EnemyContact : MonoBehaviour
{
    /*[Header("Configurações de Dano")]
    public int danoNoPlayer = 1;
        public int danoNoInimigoAoBater = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"<color=magenta>OnTriggerEnter2D acionado! Colidiu com: {collision.gameObject.name}</color>");

        // Tenta encontrar Health no objeto ou no pai
        Health saudeDoPlayer = collision.GetComponent<Health>();
        if (saudeDoPlayer == null) saudeDoPlayer = collision.GetComponentInParent<Health>();

        // Se é o player (teamId == 0)
        if (saudeDoPlayer != null && saudeDoPlayer.teamId == 0)
        {
            Debug.Log($"<color=green>Detectou Player! Aplicando dano...</color>");

            // Dano no player
            saudeDoPlayer.TakeDamage(danoNoPlayer);

            // Dano no próprio inimigo
            Health minhaSaude = GetComponent<Health>();
            if (minhaSaude != null)
            {
                Debug.Log($"<color=red>Inimigo levando dano: {danoNoInimigoAoBater}</color>");
                minhaSaude.TakeDamage(danoNoInimigoAoBater);
            }
            else
            {
                Debug.Log($"<color=yellow>Inimigo SEM Health component, destruindo diretamente</color>");
                Destroy(gameObject);
            }
        }
    }*/
}