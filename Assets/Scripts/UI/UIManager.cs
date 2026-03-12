using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
    public int pausePageIndex = 1;
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        pauseAction.Enable();
        SceneManager.sceneLoaded += AoMudarDeCena;
    }

    private void OnDisable()
    {
        pauseAction.Disable();
        SceneManager.sceneLoaded -= AoMudarDeCena;
    }

    private void AoMudarDeCena(Scene cena, LoadSceneMode modo)
    {
        // Reset fundamental para evitar que o jogo comece travado
        isPaused = false;
        Time.timeScale = 1f;

        // Reconfigura o EventSystem antes de qualquer ação de UI
        SetUpEventSystem();

        // Busca todas as páginas (UIPage) presentes na nova cena
        pages.Clear();
        pages = Resources.FindObjectsOfTypeAll<UIPage>().Where(p => p.gameObject.scene == cena).ToList();

        // Lógica específica para a cena MainMenu
        if (cena.name == "MainMenu")
        {
            allowPause = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Tenta abrir o painel principal do menu automaticamente
            GoToPageByName("MainMenu");
        }
        else
        {
            allowPause = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            SetActiveAllPages(false); // Garante que nenhum menu comece aberto no level
        }

        SetUpUIElements();
        UpdateUI();
    }

    private void SetUpUIElements()
    {
        UIelements = FindObjectsByType<UIelement>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
    }

    private void SetUpEventSystem()
    {
        eventSystem = Object.FindFirstObjectByType<EventSystem>();
    }

    public void TogglePause()
    {
        if (!allowPause) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
        {
            // Busca a página de Pause pelo nome ou pelo index configurado
            UIPage pausePage = pages.Find(p => p.gameObject.name.Contains("Pause") || p.gameObject.name.Contains("Menu"));
            if (pausePage != null) GoToPage(pages.IndexOf(pausePage));
            else if (pages.Count > pausePageIndex) GoToPage(pausePageIndex);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            SetActiveAllPages(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Update()
    {
        if (pauseAction.triggered) TogglePause();
        AtualizarTextoGhost();
        AnimarBotaoFuria();
    }

    public void GoToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < pages.Count && pages[pageIndex] != null)
        {
            SetActiveAllPages(false);
            pages[pageIndex].gameObject.SetActive(true);
            pages[pageIndex].SetSelectedUIToDefault();
        }
    }

    public void GoToPageByName(string pageName)
    {
        UIPage page = pages.Find(item => item != null && item.name == pageName);
        if (page != null) GoToPage(pages.IndexOf(page));
    }

    private void AtualizarTextoGhost()
    {
        if (textoGhost == null || GameManager.instance == null) return;
        float recorde = GameManager.instance.tempoRecordeAnterior;
        if (recorde > 0 && !GameManager.instance.gameIsOver)
        {
            float diferenca = GameManager.instance.tempoDaFase - recorde;
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

    public void SetActiveAllPages(bool activated)
    {
        foreach (UIPage page in pages)
        {
            if (page != null) page.gameObject.SetActive(activated);
        }
    }

    public void UpdateUI()
    {
        SetUpUIElements();
        foreach (UIelement ui in UIelements) ui.UpdateUI();
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}