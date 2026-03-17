using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class ScreenClearBomb : MonoBehaviour
{
    [Header("Configuracoes de Inventario")]
    public int bombasAtuais = 0;
    public int maximoDeBombas = 5;

    [Header("Input (Novo Sistema)")]
    public InputAction detonateAction;

    [Header("Configuracoes do Projetil")]
    public GameObject bombProjectilePrefab; // O prefab que brilha e cresce
    public Transform pontoDeDisparo;

    [Header("Efeitos de Explosao")]
    public GameObject[] efeitosExplosao;

    [Header("Tags de Projeteis")]
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
        if (bombProjectilePrefab == null || pontoDeDisparo == null) return;

        bombasAtuais--;
        if (UIManager.instance != null) UIManager.instance.UpdateUI();

        // Cria o nucleo que vai viajar e crescer
        GameObject projetil = Instantiate(bombProjectilePrefab, pontoDeDisparo.position, Quaternion.identity);

        // Passa a referencia deste script para o projetil saber quem ativar depois
        PlayerBomb bp = projetil.GetComponent<PlayerBomb>();
        if (bp != null) bp.Inicializar(this);
    }

    // Esta e a funcao antiga, mas agora chamada pelo projetil quando ele para
    public void AtivarLimpezaTotal()
    {
        StartCoroutine(OndaDeChoque());
    }
    // Esta e a funcao que o projetil vai procurar!
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

        // Limpeza instantanea de balas
        foreach (string tag in tagsDeTiro)
        {
            GameObject[] tiros = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject tiro in tiros) Destroy(tiro);
        }

        Health myHealth = GetComponent<Health>();
        if (myHealth == null) yield break;
        int playerTeam = myHealth.teamId;

        // Snapshot dos alvos ANTES do loop: so inclui quem esta vulneravel AGORA.
        // Evita que a nave mae seja atingida apos o escudo morrer no meio da explosao.
        Health[] todosOsHealth = Object.FindObjectsByType<Health>(FindObjectsSortMode.None);
        List<Health> alvosValidos = new List<Health>();
        foreach (Health h in todosOsHealth)
        {
            if (h == null) continue;
            if (h.teamId != playerTeam && h.gameObject != this.gameObject && !h.isAlwaysInvincible)
                alvosValidos.Add(h);
        }

        while (timer < duracao)
        {
            foreach (Health h in alvosValidos)
            {
                if (h == null) continue;
                h.TakeDamage(15);
            }

            timer += 0.1f;
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
