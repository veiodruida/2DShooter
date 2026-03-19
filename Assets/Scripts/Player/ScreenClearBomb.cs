using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class ScreenClearBomb : MonoBehaviour
{
    [Header("Configurações de Áudio (2D Fixo)")]
    public AudioClip somCompletoBomba;
    [Tooltip("Momento do 'BOOM' no áudio (Ex: 1.0)")]
    public float tempoParaExplodirNoAudio = 1.0f;
    [Range(0f, 1f)] public float volumeGeral = 1.0f;

    [Header("Efeito de Partículas")]
    public GameObject prefabParticulasResiduo; // Adicionado apenas este campo

    [Header("Configuracoes de Inventario")]
    public int bombasAtuais = 0;
    public int maximoDeBombas = 5;

    [Header("Input (Novo System)")]
    public InputAction detonateAction;

    [Header("Configuracoes do Projetil")]
    public GameObject bombProjectilePrefab;
    public Transform pontoDeDisparo;

    [Header("Efeitos de Explosao")]
    public GameObject[] efeitosExplosao;

    [Header("Tags de Projeteis")]
    public string[] tagsDeTiro = { "EnemyProjectile" };

    private AudioSource myAudioSource;

    private void Awake()
    {
        myAudioSource = GetComponent<AudioSource>();

        // --- CONFIGURAÇÃO FORÇADA DE ÁUDIO 2D ---
        myAudioSource.playOnAwake = false;
        myAudioSource.spatialBlend = 0f; // 0 = 2D total (Volume não muda com a distância)
        myAudioSource.loop = false;
    }

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

        // --- TOCA O SOM EM 2D REAL ---
        if (somCompletoBomba != null)
        {
            myAudioSource.PlayOneShot(somCompletoBomba, volumeGeral);
        }

        GameObject projetil = Instantiate(bombProjectilePrefab, pontoDeDisparo.position, Quaternion.identity);

        PlayerBomb bp = projetil.GetComponent<PlayerBomb>();
        if (bp != null)
        {
            bp.Inicializar(this);
            bp.tempoDeVida = tempoParaExplodirNoAudio;
        }
    }

    public void AtivarLimpezaTotal() => AtivarOndaDeChoque(transform.position);

    public void AtivarOndaDeChoque(Vector3 posicaoDaExplosao)
    {
        // 1. Criar partículas no local da BOMBA
        if (prefabParticulasResiduo != null)
        {
            Instantiate(prefabParticulasResiduo, posicaoDaExplosao, Quaternion.identity);
        }

        // 2. Criar explosão visual no local da BOMBA
        SpawnExplosao(posicaoDaExplosao);

        // 3. Iniciar a rotina de dano
        StartCoroutine(OndaDeChoque());
    }

    IEnumerator OndaDeChoque()
    {
        float duracao = 0.5f;
        float timer = 0f;

        if (CameraShake.instance != null)
        {
            CameraShake.instance.Shake(0.6f, 0.4f);
        }

        foreach (string tag in tagsDeTiro)
        {
            GameObject[] tiros = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject tiro in tiros) Destroy(tiro);
        }

        Health myHealth = GetComponent<Health>();
        if (myHealth == null) yield break;
        int playerTeam = myHealth.teamId;

        Health[] todosOsHealth = Object.FindObjectsByType<Health>(FindObjectsSortMode.None);
        List<Health> alvosValidos = new List<Health>();
        foreach (Health h in todosOsHealth)
        {
            if (h != null && h.teamId != playerTeam && h.gameObject != this.gameObject && !h.isAlwaysInvincible)
                alvosValidos.Add(h);
        }

        while (timer < duracao)
        {
            foreach (Health h in alvosValidos)
            {
                if (h != null) h.TakeDamage(20);
            }
            timer += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void DispararBombaPeloBotao()
    {
        if (bombasAtuais > 0) LancarNucleoBomba();
    }

    void SpawnExplosao(Vector3 posicao)
    {
        if (efeitosExplosao != null && efeitosExplosao.Length > 0)
        {
            int index = Random.Range(0, efeitosExplosao.Length);
            if (efeitosExplosao[index] != null)
                Instantiate(efeitosExplosao[index], posicao, Quaternion.identity);
        }
    }
}