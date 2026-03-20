using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ScrollableText : MonoBehaviour
{
    [Header("References")]
    public ScrollRect scrollRect;
    public TextMeshProUGUI text;

    [Header("Initial Content (optional)")]
    [TextArea(4, 10)]
    public string initialText = "";

    [Header("Auto Scroll")]
    public float autoScrollSpeed = 0.05f;

    [Header("Input")]
    public float wheelSensitivity = 0.1f;

    public bool autoAssignReferences = true;
    public bool autoStartAutoScroll = false;
    public bool logSizesOnStart = true;

    private Coroutine autoScrollRoutine;

    void Reset()
    {
        TryAutoAssign();
    }

    void OnValidate()
    {
        if (autoAssignReferences) TryAutoAssign();
    }

    void Awake()
    {
        if (autoAssignReferences) TryAutoAssign();
    }

    void Start()
    {
        // Usamos uma corotina para dar tempo ao sistema de UI do Unity de inicializar
        StartCoroutine(InitialSetupRoutine());
    }

    IEnumerator InitialSetupRoutine()
    {
        // Espera o final do frame para garantir que o TMP calculou o tamanho real
        yield return new WaitForEndOfFrame();

        if (text != null && !string.IsNullOrEmpty(initialText))
        {
            SetText(initialText, true);
        }
        else
        {
            ForceRebuildAndScrollTop(true);
        }

        if (logSizesOnStart) LogSizes();
        if (autoStartAutoScroll) StartAutoScroll();
    }

    void Update()
    {
        float scrollY = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 wheel = Mouse.current.scroll.ReadValue();
            scrollY = wheel.y;
        }
#else
        scrollY = Input.mouseScrollDelta.y;
#endif

        if (scrollRect != null && Mathf.Abs(scrollY) > 0.01f)
        {
            float delta = (scrollY > 0 ? 1 : -1) * wheelSensitivity;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + delta);
            if (autoScrollRoutine != null) StopAutoScroll();
        }
    }

    void TryAutoAssign()
    {
        if (scrollRect == null) scrollRect = GetComponentInChildren<ScrollRect>();
        if (scrollRect != null && text == null && scrollRect.content != null)
        {
            text = scrollRect.content.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public void SetText(string content, bool scrollToTop = true)
    {
        if (text == null) return;

        text.text = content;
        // Espera um frame para o TMP atualizar o preferredHeight
        StartCoroutine(RefreshLayoutDelayed(scrollToTop));
    }

    IEnumerator RefreshLayoutDelayed(bool scrollToTop)
    {
        yield return new WaitForEndOfFrame();
        ForceRebuildAndScrollTop(scrollToTop);
    }

    void ForceRebuildAndScrollTop(bool toTop = true)
    {
        if (scrollRect == null || scrollRect.content == null || text == null) return;

        // 1. Garante o Pivot correto para o scroll não "fugir"
        scrollRect.content.pivot = new Vector2(0.5f, 1f);

        // 2. Força o TextMeshPro a calcular o tamanho real agora mesmo
        text.ForceMeshUpdate();
        float preferred = text.preferredHeight;

        // 3. Se o Content Size Fitter falhar, nós forçamos o tamanho via código
        // O sizeDelta ajusta a altura do container (Content)
        scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, preferred);

        // 4. Força a UI a se reconstruir imediatamente
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        if (toTop) ScrollToTop();

        // Log de verificação (O Content Height não pode mais ser 0)
        Debug.Log($"<color=yellow>Novo Content Height após rebuild: {scrollRect.content.rect.height}</color>");
    }

    public void ScrollToTop()
    {
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
    }

    public void ScrollToBottom()
    {
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    }

    public void StartAutoScroll(float speed = -1f)
    {
        if (scrollRect == null) return;
        if (speed > 0f) autoScrollSpeed = speed;
        if (autoScrollRoutine != null) StopCoroutine(autoScrollRoutine);
        autoScrollRoutine = StartCoroutine(AutoScrollCoroutine());
    }

    public void StopAutoScroll()
    {
        if (autoScrollRoutine != null) StopCoroutine(autoScrollRoutine);
        autoScrollRoutine = null;
    }

    IEnumerator AutoScrollCoroutine()
    {
        yield return new WaitForEndOfFrame();
        while (scrollRect != null && scrollRect.verticalNormalizedPosition > 0.001f)
        {
            scrollRect.verticalNormalizedPosition -= autoScrollSpeed * Time.deltaTime;
            yield return null;
        }
        autoScrollRoutine = null;
    }

    public void LogSizes()
    {
        if (scrollRect == null || scrollRect.content == null || text == null) return;

        var contentRect = scrollRect.content.rect;
        var viewportRect = scrollRect.viewport != null ? scrollRect.viewport.rect : scrollRect.GetComponent<RectTransform>().rect;

        Debug.Log($"<color=cyan>ScrollableText Stats:</color>\n" +
                  $"- Content Height: {contentRect.height}\n" +
                  $"- Viewport Height: {viewportRect.height}\n" +
                  $"- Text Preferred: {text.preferredHeight}\n" +
                  $"- Scrollable: {(contentRect.height > viewportRect.height)}", this);
    }
}