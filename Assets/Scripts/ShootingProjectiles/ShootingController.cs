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

    void OnEnable() => fireAction.Enable();
    void OnDisable() => fireAction.Disable();

    private void Update() => ProcessInput();

    // PEGA O VALOR DIRETAMENTE DO ARQUIVO DE DIFICULDADE
    public float GetCurrentFireRate()
    {
        if (GameSettings.instance != null && GameSettings.instance.configAtual != null)
        {
            var cfg = GameSettings.instance.configAtual;

            if (isPlayerControlled)
            {
                // USA O CAMPO "Taxa De Tiro" DO PLAYER NO ARQUIVO
                return cfg.taxaDeTiro;
            }
            else
            {
                // USA O CAMPO "Intervalo Tiro Inimigo" DO ARQUIVO
                return cfg.intervaloTiroInimigo;
            }
        }

        return fireRateBase; // Fallback caso o sistema falhe
    }

    void ProcessInput()
    {
        if (isPlayerControlled && fireAction.ReadValue<float>() >= 1)
        {
            Fire();
        }
    }

    public void Fire()
    {
        // Agora usa o valor vindo do DifficultyData
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

            if (projectileHolder == null)
                projectileHolder = GameObject.Find("ProjectileHolder")?.transform;

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
}