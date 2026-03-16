using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Page Management")]
    public List<UIPage> pages = new List<UIPage>();
    public bool allowPause = true;

    [Header("Input Actions")]
    public InputAction pauseAction;

    [Header("Referências de UI")]
    public TextMeshProUGUI textoGhost;
    public GameObject botaoFuria;
    public Text textoBotaoFuria;
    public float velocidadePulsoFuria = 5f;

    private bool isPaused = false;
    private List<UIelement> UIelements;
    [HideInInspector] public EventSystem eventSystem;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    // Método Mágico para o UIManager se auto-criar
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (instance == null)
        {
            // Tenta carregar o prefab da pasta Resources
            GameObject prefab = Resources.Load<GameObject>("UIManager");

            if (prefab != null)
            {
                Debug.Log("<color=yellow>Auto-Spawner: Criando UIManager automaticamente no início da cena.</color>");
                GameObject clone = Instantiate(prefab);
                instance = clone.GetComponent<UIManager>();
            }
            else
            {
                Debug.LogError("Auto-Spawner: Prefab 'UIManager' não encontrado na pasta Assets/Resources!");
            }
        }
    }
    private void OnEnable()
    {
        pauseAction.Enable();
        SceneManager.sceneLoaded += AoMudarDeCena;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnDisable()
    {
        pauseAction.Disable();
        SceneManager.sceneLoaded -= AoMudarDeCena;
    }

    private void AoMudarDeCena(Scene cena, LoadSceneMode modo)
    {
        isPaused = false;
        Time.timeScale = 1f;

        // Re-ativa as ações de input
        pauseAction.Disable();
        pauseAction.Enable();

        eventSystem = Object.FindFirstObjectByType<EventSystem>();

        // Busca as páginas da cena atual
        pages = Resources.FindObjectsOfTypeAll<UIPage>()
            .Where(p => p.gameObject.scene == cena).ToList();

        foreach (var p in pages) p.gameObject.SetActive(false);

        // BUSCA DINÂMICA DO TEXTO RECORD (Evita o erro de sumir o texto)
        if (textoGhost == null)
        {
            GameObject obj = GameObject.Find("TextoRecorde"); // Garante que o nome na hierarquia seja este
            if (obj != null) textoGhost = obj.GetComponent<TextMeshProUGUI>();
        }

        if (cena.name == "MainMenu")
        {
            allowPause = false;
            ConfigurarCursor(true);
            GoToPageByName("MainMenu");
        }
        else
        {
            allowPause = true;
            ConfigurarCursor(true); // MANTÉM O CURSOR ATIVO PARA O JOGO
        }
        UpdateUI();
    }

    public void ConfigurarCursor(bool visivel)
    {
        Cursor.visible = visivel;
        Cursor.lockState = visivel ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void TogglePause()
    {
        if (!allowPause || (GameManager.instance != null && GameManager.instance.gameIsOver)) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        ConfigurarCursor(isPaused);

        if (isPaused) GoToPageByName("PausePage");
        else SetActiveAllPages(false);
    }

    public void GoToPageByName(string pageName)
    {
        // LOG DE ENTRADA - Se não aparecer no Console, o EventSystem da cena está quebrado
        Debug.Log($"<color=orange>UIManager: Tentando abrir a página: {pageName}</color>");

        if (pages == null || pages.Count == 0)
        {
            pages = Resources.FindObjectsOfTypeAll<UIPage>()
                .Where(p => p.gameObject.scene == SceneManager.GetActiveScene()).ToList();
        }

        UIPage page = pages.Find(item => item != null && item.gameObject.name == pageName);

        if (page != null)
        {
            SetActiveAllPages(false);
            page.gameObject.SetActive(true);

            // CORREÇÃO AQUI: Chamamos o método diretamente. 
            // Se a página existe, o método existe.
            page.SetSelectedUIToDefault();

            Debug.Log($"<color=green>UIManager: Sucesso! {pageName} aberta.</color>");
        }
        else
        {
            string disponiveis = string.Join(", ", pages.Select(p => p.gameObject.name));
            Debug.LogError($"UIManager: Erro! '{pageName}' não encontrada. Disponíveis: {disponiveis}");
        }
    }

    public void SetActiveAllPages(bool status)
    {
        foreach (UIPage page in pages) if (page != null) page.gameObject.SetActive(status);
    }

    public void UpdateUI()
    {
        UIelements = FindObjectsByType<UIelement>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
        foreach (UIelement ui in UIelements) ui.UpdateUI();
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    private void Update()
    {
        if (pauseAction != null && pauseAction.triggered) TogglePause();

        // Só atualiza o Ghost se estivermos no jogo e houver um recorde válido
        if (GameManager.instance != null && GameManager.instance.tempoRecordeAnterior < 9999f)
        {
            AtualizarTextoGhost();
            AnimarBotaoFuria();
        }
    }
    /*
    private void Update()
    {
        if (pauseAction != null && pauseAction.triggered) TogglePause();
        AtualizarTextoGhost();
        
    }*/

    private void AtualizarTextoGhost()
    {
        if (textoGhost == null || GameManager.instance == null) return;

        float recorde = GameManager.instance.tempoRecordeAnterior;

        // Se o recorde for o valor padrão (9999), não mostramos o comparador ainda
        if (recorde >= 9999f)
        {
            textoGhost.text = "";
            return;
        }

        if (!GameManager.instance.gameIsOver)
        {
            float diferenca = GameManager.instance.tempoDaFase - recorde;

            // Lógica de cores: Verde se estiver mais rápido (-), Vermelho se estiver lento (+)
            textoGhost.text = "RECORD: " + (diferenca < 0 ? "" : "+") + diferenca.ToString("F2") + "s";
            textoGhost.color = diferenca < 0 ? Color.green : Color.red;
        }
    }

    private void AnimarBotaoFuria()
    {
        if (botaoFuria != null && botaoFuria.activeSelf && textoBotaoFuria != null)
        {
            float alfa = 0.4f + Mathf.Abs(Mathf.Sin(Time.time * velocidadePulsoFuria)) * 0.6f;
            textoBotaoFuria.color = new Color(1, 0, 0, alfa);
            float escala = 1.0f + Mathf.Sin(Time.time * velocidadePulsoFuria) * 0.03f;
            botaoFuria.transform.localScale = new Vector3(escala, escala, 1);
        }
    }
}