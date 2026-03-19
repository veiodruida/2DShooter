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

    // NOVA VARIÁVEL PARA MOBILE
    private bool isFiringMobile = false;

    [Header("Firing Settings")]
    public float fireRateBase = 0.15f;
    public float projectileSpread = 1.0f;

    private float lastFired = Mathf.NegativeInfinity;

    [Header("Effects")]
    public GameObject fireEffect;
    public AudioSource fireSound;

    [Header("Power Up Tiro")]
    [Range(1, 3)]
    public int weaponLevel = 1;

    void OnEnable() => fireAction.Enable();
    void OnDisable() => fireAction.Disable();

    void Update()
    {
        ProcessInput();

        if (Time.timeScale > 0 && !Cursor.visible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    private void Start()
    {
        if (projectileHolder == null)
        {
            GameObject holder = GameObject.Find("ProjectileHolder");
            if (holder != null) projectileHolder = holder.transform;
        }
    }

    public float GetCurrentFireRate()
    {
        if (GameSettings.instance != null && GameSettings.instance.configAtual != null)
        {
            var cfg = GameSettings.instance.configAtual;
            return isPlayerControlled ? cfg.taxaDeTiro : cfg.intervaloTiroInimigo;
        }
        return fireRateBase;
    }

    // --- MUDANÇA AQUI: Aceita tanto Teclado quanto Botão Mobile ---
    void ProcessInput()
    {
        if (!isPlayerControlled) return;

        // Atira se a tecla estiver pressionada OU se o botão mobile estiver ativo
        bool isFiring = fireAction.ReadValue<float>() >= 1 || isFiringMobile;

        if (isFiring)
        {
            Fire();
        }
    }

    // FUNÇÃO PARA O BOTÃO DA UI CHAMAR
    public void SetMobileFiring(bool firing)
    {
        isFiringMobile = firing;
    }

    public void Fire()
    {
        // Usa Time.time para evitar bugs com o LevelLoad se o jogo reiniciar rápido
        if ((Time.time - lastFired) > GetCurrentFireRate())
        {
            SpawnProjectile();
            if (fireEffect != null) Instantiate(fireEffect, transform.position, transform.rotation);
            if (fireSound != null) fireSound.Play();
            lastFired = Time.time;
        }
    }

    public void SpawnProjectile()
    {
        if (projectilePrefab == null) return;

        for (int i = 0; i < weaponLevel; i++)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position, transform.rotation);
            Vector3 rotationEuler = proj.transform.rotation.eulerAngles;

            if (weaponLevel == 2)
            {
                float offset = (i == 0) ? -0.25f : 0.25f;
                proj.transform.position += transform.right * offset;
            }
            else if (weaponLevel == 3)
            {
                float angleOffset = (i - 1) * 15f;
                rotationEuler.z += angleOffset;
            }

            rotationEuler.z += Random.Range(-projectileSpread, projectileSpread);
            proj.transform.rotation = Quaternion.Euler(rotationEuler);

            if (projectileHolder != null) proj.transform.SetParent(projectileHolder);
        }
    }

    public void UpgradeWeapon()
    {
        if (weaponLevel < 3) weaponLevel++;
    }

    public void ResetWeapon() => weaponLevel = 1;
}