using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla a interface visual da barra de vida e escudo do Boss.
/// Gerencia efeitos de alerta (piscar) e ativa as fases de furia da MotherShip.
/// </summary>
public class UIBossHealthBar : UIelement
{
    [Header("Referencias do Boss")]
    public string bossName = "MotherShip";
    private MotherShip motherShipScript;
    private Health bossHealth;

    [Header("Imagens de Preenchimento (Fill)")]
    public Image healthFillImage;
    public Image shieldFillImage;

    [Header("Alerta de Vida (Vermelha)")]
    [Range(0, 1)] public float thresholdHealth = 0.25f;
    public Color corHealthNormal = Color.red;
    public Color corHealthAlerta = Color.white;

    [Header("Alerta de Escudo (Azul)")]
    [Range(0, 1)] public float thresholdShield = 0.30f;
    public Color corShieldNormal = Color.cyan;
    public Color corShieldAlerta = Color.blue;

    [Header("Configuracao Global")]
    public float velocidadePiscar = 12f;

    /// <summary>
    /// Update executa a cada frame para garantir que o efeito de piscar 
    /// seja suave, mesmo que o Boss nao esteja recebendo dano no momento.
    /// </summary>
    void Update()
    {
        if (bossHealth != null)
        {
            // Calcula as porcentagens atuais para decidir se precisa atualizar o visual (piscar)
            float healthPct = (float)bossHealth.currentHealth / bossHealth.maximumHealth;
            float shieldPct = 0;

            if (motherShipScript != null && motherShipScript.escudoHealth != null)
            {
                shieldPct = (float)motherShipScript.escudoHealth.currentHealth / motherShipScript.escudoHealth.maximumHealth;
            }

            // Se algum dos medidores estiver em estado de alerta, chamamos o UpdateUI para processar as cores
            if (healthPct <= thresholdHealth || (shieldPct > 0 && shieldPct <= thresholdShield))
            {
                UpdateUI();
            }
        }
    }

    /// <summary>
    /// Atualiza os valores das barras, cores e verifica gatilhos de furia.
    /// </summary>
    public override void UpdateUI()
    {
        // Busca as referencias do Boss caso ainda nao existam (Lazy Loading)
        if (motherShipScript == null)
        {
            GameObject bossObj = GameObject.Find(bossName);
            if (bossObj != null)
            {
                motherShipScript = bossObj.GetComponent<MotherShip>();
                bossHealth = bossObj.GetComponent<Health>();
            }
            else return; // Se nao encontrou o boss, interrompe a execucao
        }

        if (bossHealth == null) return;

        // 1. CaLCULO DAS PERCENTAGENS ATUAIS
        float healthPct = (float)bossHealth.currentHealth / bossHealth.maximumHealth;
        float shieldPct = 1f; // Inicia em 100% caso o escudo nao exista ou ja tenha morrido

        bool temEscudoAtivo = motherShipScript.escudoHealth != null && motherShipScript.escudoHealth.gameObject.activeSelf;

        if (temEscudoAtivo)
        {
            float maxS = motherShipScript.escudoHealth.maximumHealth > 0 ? motherShipScript.escudoHealth.maximumHealth : 1;
            shieldPct = (float)motherShipScript.escudoHealth.currentHealth / maxS;
        }
        else
        {
            shieldPct = 0; // Se o objeto do escudo sumiu, a porcentagem e 0
        }

        // 2. LoGICA DE ATIVAcaO DAS FASES DE FuRIA (PROGRESSIVA)
        // Prioridade para Fase 2 (Vida Critica)
        if (healthPct <= 0.25f && healthPct > 0)
        {
            motherShipScript.AtivarModoFuria(2);
        }
        // Gatilho para Fase 1 (Escudo Critico ou Destruido)
        else if (shieldPct <= 0.25f || !temEscudoAtivo)
        {
            motherShipScript.AtivarModoFuria(1);
        }

        // Valor oscilante entre 0 e 1 para o efeito de piscar as cores
        float pingPong = Mathf.PingPong(Time.time * velocidadePiscar, 1);

        // 3. ATUALIZAcaO VISUAL DA BARRA DE VIDA
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = Mathf.Clamp01(healthPct);

            // Aplica efeito de piscar se estiver abaixo do limite de alerta
            if (healthPct <= thresholdHealth && healthPct > 0)
                healthFillImage.color = Color.Lerp(corHealthNormal, corHealthAlerta, pingPong);
            else
                healthFillImage.color = corHealthNormal;
        }

        // 4. ATUALIZAcaO VISUAL DA BARRA DE ESCUDO
        if (shieldFillImage != null)
        {
            shieldFillImage.fillAmount = Mathf.Clamp01(shieldPct);

            // Aplica efeito de piscar se estiver abaixo do limite de alerta
            if (shieldPct <= thresholdShield && shieldPct > 0)
                shieldFillImage.color = Color.Lerp(corShieldNormal, corShieldAlerta, pingPong);
            else
                shieldFillImage.color = corShieldNormal;
        }
    }
}
