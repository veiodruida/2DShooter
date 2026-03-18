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
    [Range(0f, 1f)] public float volumeGeral = 0.8f;

    [Header("Efeitos Visuais")]
    public GameObject prefabParticulasResiduo;
    public GameObject[] efeitosExplosao;

    [Header("Configuracoes de Inventario")]
    public int bombasAtuais = 0;
    public int maximoDeBombas = 5;

    [Header("Input (Novo System)")]
    public InputAction detonateAction;

    [Header("Configuracoes do Projetil")]
    public GameObject bombProjectilePrefab;
    public Transform pontoDeDisparo;

    [Header("Tags de Projeteis")]
    public string[] tagsDeTiro = { "EnemyProjectile" };

    private AudioSource myAudioSource;

    private void Awake()
    {
        myAudioSource = GetComponent<AudioSource>();

        // Força as configurações 2D para não bugar
        myAudioSource.playOnAwake = false;
        myAudioSource.spatialBlend = 0f;
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

        // --- TOCA O SOM SEM CORTAR ---
        if (somCompletoBomba != null)
        {
            // Se o som travou antes, o PlayOneShot aqui vai rodar de forma independente
            myAudioSource.PlayOneShot(somCompletoBomba, volumeGeral);
        }

        GameObject projetil = Instantiate(bombProjectilePrefab, pontoDeDisparo.position, Quaternion.identity);

        PlayerBomb bp = projetil.GetComponent<Health>() != null ? projetil.GetComponent<PlayerBomb>() : projetil.GetComponent<PlayerBomb>();

        // Pegando o componente de forma segura
        PlayerBomb bombComponent = projetil.GetComponent<PlayerBomb>();
        if (bombComponent != null)
        {
            bombComponent.Inicializar(this);
            bombComponent.tempoDeVida = tempoParaExplodirNoAudio;
        }
    }

    // Recebe a posição da bomba para estourar no lugar certo
    public void AtivarOndaDeChoque(Vector3 posicaoDaBomba)
    {
        if (prefabParticulasResiduo != null)
        {
            Instantiate(prefabParticulasResiduo, posicaoDaBomba, Quaternion.identity);
        }

        SpawnExplosao(posicaoDaBomba);

        StartCoroutine(OndaDeChoque());
    }

    // Caso algo chame sem posição (retrocompatibilidade)
    public void AtivarLimpezaTotal() => AtivarOndaDeChoque(transform.position);

    IEnumerator OndaDeChoque()
    {
        float duracao = 0.5f;
        float timer = 0f;

        if (CameraShake.instance != null) CameraShake.instance.Shake(0.6f, 0.4f);

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