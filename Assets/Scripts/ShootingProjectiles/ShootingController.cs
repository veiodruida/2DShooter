using UnityEngine;
using UnityEngine.InputSystem;

public class ShootingController : MonoBehaviour
{
    [Header("GameObject/Component References")]
    public GameObject projectilePrefab = null;
    public Transform projectileHolder = null;

    [Header("Input Settings")]
    public bool isPlayerControlled = false;
    public InputAction fireAction;

    [Header("Firing Settings")]
    [Tooltip("Base fire rate usado caso o GameSettings não seja encontrado")]
    public float fireRateBase = 0.15f;
    public float projectileSpread = 1.0f;

    private float lastFired = Mathf.NegativeInfinity;

    [Header("Effects")]
    public GameObject fireEffect;

    [Header("Power Up Tiro")]
    [Range(1, 3)]
    public int weaponLevel = 1;

    // --- MANTENDO A LÓGICA DE INPUT ORIGINAL ---
    void OnEnable() => fireAction.Enable();
    void OnDisable() => fireAction.Disable();

    void Update()
    {
        ProcessInput();

        // GARANTIA: Se o jogo estiver a correr e não estiver pausado, mantém o cursor visível
        if (Time.timeScale > 0 && !Cursor.visible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    private void Start()
    {
        // Busca o holder apenas uma vez no início para ganhar performance
        if (projectileHolder == null)
        {
            GameObject holder = GameObject.Find("ProjectileHolder");
            if (holder != null) projectileHolder = holder.transform;
        }
    }

    // PEGA O VALOR DIRETAMENTE DO ARQUIVO DE DIFICULDADE (Sincronizado)
    public float GetCurrentFireRate()
    {
        if (GameSettings.instance != null && GameSettings.instance.configAtual != null)
        {
            var cfg = GameSettings.instance.configAtual;

            if (isPlayerControlled)
            {
                // Garante que o campo no ScriptableObject se chama taxaDeTiro
                return cfg.taxaDeTiro;
            }
            else
            {
                // Garante que o campo no ScriptableObject se chama intervaloTiroInimigo
                return cfg.intervaloTiroInimigo;
            }
        }

        return fireRateBase; // Fallback
    }

    void ProcessInput()
    {
        // Mantém a verificação do novo Input System
        if (isPlayerControlled && fireAction.ReadValue<float>() >= 1)
        {
            Fire();
        }
    }

    public void Fire()
    {
        // Usa o GetCurrentFireRate() que consulta a dificuldade
        if ((Time.timeSinceLevelLoad - lastFired) > GetCurrentFireRate())
        {
            SpawnProjectile();
            if (fireEffect != null) Instantiate(fireEffect, transform.position, transform.rotation);
            lastFired = Time.timeSinceLevelLoad;
        }
    }

    public void SpawnProjectile()
    {
        if (projectilePrefab == null) return;

        for (int i = 0; i < weaponLevel; i++)
        {
            // 1. Instancia o projétil
            GameObject proj = Instantiate(projectilePrefab, transform.position, transform.rotation);
            Vector3 rotationEuler = proj.transform.rotation.eulerAngles;

            // 2. Lógica de Nível de Arma (offsets originais)
            if (weaponLevel == 2)
            {
                float offset = (i == 0) ? -0.25f : 0.25f;
                proj.transform.position += transform.right * offset;
            }
            else if (weaponLevel == 3)
            {
                // Tiro central, esquerda (-15) e direita (+15)
                float angleOffset = (i - 1) * 15f;
                rotationEuler.z += angleOffset;
            }

            // 3. Aplica o Spread aleatório que tinhas definido
            rotationEuler.z += Random.Range(-projectileSpread, projectileSpread);
            proj.transform.rotation = Quaternion.Euler(rotationEuler);

            // 4. Gestão do ProjectileHolder (cache para evitar Find repetido a cada tiro)
            if (projectileHolder == null)
            {
                GameObject holder = GameObject.Find("ProjectileHolder");
                if (holder != null) projectileHolder = holder.transform;
            }

            if (projectileHolder != null) proj.transform.SetParent(projectileHolder);
        }
    }

    public void UpgradeWeapon()
    {
        if (weaponLevel < 3)
        {
            weaponLevel++;
            Debug.Log("<color=cyan>Arma evoluiu para nível: </color>" + weaponLevel);
        }
    }

    // Função de reset chamada pelo Health ao morrer (Importante!)
    public void ResetWeapon()
    {
        weaponLevel = 1;
        Debug.Log("<color=orange>Arma resetada para nível 1</color>");
    }
}