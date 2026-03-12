using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class ScreenClearBomb : MonoBehaviour
{
    [Header("Configurações de Inventário")]
    public int bombasAtuais = 0;
    public int maximoDeBombas = 5;

    [Header("Input (Novo Sistema)")]
    public InputAction detonateAction;

    [Header("Configurações do Projétil")]
    public GameObject bombProjectilePrefab; // O prefab que brilha e cresce
    public Transform pontoDeDisparo;

    [Header("Efeitos de Explosão")]
    public GameObject[] efeitosExplosao;

    [Header("Tags de Projéteis")]
    public string[] tagsDeTiro = { "EnemyProjectile" };

    private void OnEnable() => detonateAction.Enable();
    private void OnDisable() => detonateAction.Disable();

    void Update()
    {
        if (detonateAction.triggered && bombasAtuais > 0)
        {
            LancarNucleoBomba();
        }
    }

    public void AdicionarBomba(int quantidade)
    {
        if (bombasAtuais < maximoDeBombas)
        {
            bombasAtuais = Mathf.Min(bombasAtuais + quantidade, maximoDeBombas);
            if (UIManager.instance != null) UIManager.instance.UpdateUI();
        }
    }

    void LancarNucleoBomba()
    {
        bombasAtuais--;
        if (UIManager.instance != null) UIManager.instance.UpdateUI();

        // Cria o núcleo que vai viajar e crescer
        GameObject projétil = Instantiate(bombProjectilePrefab, pontoDeDisparo.position, Quaternion.identity);

        // Passa a referência deste script para o projétil saber quem ativar depois
        PlayerBomb bp = projétil.GetComponent<PlayerBomb>();
        bp = projétil.GetComponent<PlayerBomb>();
        if (bp != null) bp.Inicializar(this);
    }

    // Esta é a tua função antiga, mas agora chamada pelo projétil quando ele para
    public void AtivarLimpezaTotal()
    {
        StartCoroutine(OndaDeChoque());
    }
    // Esta é a função que o projétil vai procurar!
    public void AtivarOndaDeChoque()
    {
        StartCoroutine(OndaDeChoque());
    }
    IEnumerator OndaDeChoque()
    {
        float duracao = 0.5f;
        float timer = 0f;

        // Tremor forte e longo para a bomba de limpeza
        if (CameraShake.instance != null)
        {
            CameraShake.instance.Shake(0.5f, 0.4f);
        }

        // Limpeza instantânea de balas (como tinhas antes)
        foreach (string tag in tagsDeTiro)
        {
            GameObject[] tiros = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject tiro in tiros) Destroy(tiro);
        }

        while (timer < duracao)
        {
            Health[] inimigosVivos = Object.FindObjectsByType<Health>(FindObjectsSortMode.None);
            int playerTeam = GetComponent<Health>().teamId;

            foreach (Health h in inimigosVivos)
            {
                if (h == null) continue;
                if (h.teamId != playerTeam && h.gameObject != this.gameObject)
                {
                    // O TakeDamage agora já filtra AlwaysInvincible, mas checamos aqui por performance
                    h.TakeDamage(15);
                }
            }

            timer += 0.1f; // Em vez de rodar todo frame, roda a cada 0.1s
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void DispararBombaPeloBotao()
    {
        if (bombasAtuais > 0)
        {
            LancarNucleoBomba();
        }
    }

    void SpawnExplosao(Vector3 posicao)
    {
        if (efeitosExplosao != null && efeitosExplosao.Length > 0)
        {
            int index = Random.Range(0, efeitosExplosao.Length);
            Instantiate(efeitosExplosao[index], posicao, Quaternion.identity);
        }
    }
}