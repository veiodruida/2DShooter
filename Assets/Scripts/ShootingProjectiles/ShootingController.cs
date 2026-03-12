using System.Collections;
using System.Collections.Generic;
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
    [Tooltip("Base fire rate antes dos modificadores")]
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

    // Calcula o Fire Rate atual baseado na dificuldade do GameManager
    public float GetCurrentFireRate()
    {
        // Se for Inimigo ou Nave Mãe, usa o valor exato injetado (Autoridade do Arquivo)
        if (!isPlayerControlled)
        {
            return fireRateBase;
        }

        // Lógica apenas para o PLAYER
        float rate = fireRateBase;

        if (GameManager.instance != null && GameSettings.instance != null)
        {
            rate -= (GameSettings.instance.nivelAtual * 0.01f);

            if (GameSettings.instance.dificuldadeSelecionada == GameSettings.Dificuldade.Furia)
            {
                rate *= 1.25f;
            }
            else if (GameSettings.instance.dificuldadeSelecionada == GameSettings.Dificuldade.Facil)
            {
                rate *= 0.9f;
            }
        }

        return Mathf.Max(rate, 0.05f);
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
        // Usa o Fire Rate dinâmico baseado na dificuldade
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