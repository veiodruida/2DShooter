using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla o comportamento do inimigo, incluindo movimento, tiro e pontuação.
/// </summary>
public class Enemy : MonoBehaviour
{
    // Definições de Enum movidas para o topo para não interferir nos Headers
    public enum ShootMode { None, ShootAll };
    public enum MovementModes { NoMovement, FollowTarget, Scroll };

    [Header("General Settings")]
    [Tooltip("Velocidade de movimento do inimigo.")]
    public float moveSpeed = 5.0f;
    [Tooltip("Pontos concedidos ao destruir este inimigo.")]
    public int scoreValue = 5;

    [Header("Following Settings")]
    [Tooltip("Alvo que o inimigo deve seguir (geralmente o Player).")]
    public Transform followTarget = null;
    [Tooltip("Distância mínima para começar a seguir.")]
    public float followRange = 10.0f;

    [Header("Shooting Settings")]
    [Tooltip("Lista de armas (ShootingControllers) acopladas ao inimigo.")]
    public List<ShootingController> guns = new List<ShootingController>();
    public ShootMode shootMode = ShootMode.ShootAll;

    [Header("Movement Settings")]
    public MovementModes movementMode = MovementModes.FollowTarget;
    [SerializeField] private Vector3 scrollDirection = Vector3.right;

    private Vector3 originalPosition;
    private Vector3 turnPosition;

    private void Start()
    {
        // Busca automática do Player pela Tag
        if (followTarget == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) followTarget = p.transform;
        }

        // Configura lógica de patrulha (Scroll)
        if (movementMode == MovementModes.Scroll)
        {
            originalPosition = transform.position;
            turnPosition = originalPosition + scrollDirection;
        }
        // 1. AJUSTE DE VIDA: Faz o inimigo ter mais ou menos vida conforme a dificuldade
        Health h = GetComponent<Health>();
        if (h != null && GameManager.instance != null)
        {
            // Pede ao GameManager para calcular a vida baseada no valor padrão
            h.defaultHealth = GameManager.instance.CalcularVidaInimigo(h.defaultHealth);
            h.currentHealth = h.defaultHealth;
        }
        ConfigurarDificuldade();
    }

    private void LateUpdate()
    {
        HandleBehaviour();
    }

    void ConfigurarDificuldade()
    {
        if (GameSettings.instance != null && GameSettings.instance.configAtual != null)
        {
            AplicarConfiguracoes(GameSettings.instance.configAtual);
        }
        else
        {
            GameSettings settings = FindFirstObjectByType<GameSettings>();
            if (settings != null && settings.configAtual != null)
                AplicarConfiguracoes(settings.configAtual);
        }
    }

    void AplicarConfiguracoes(DifficultyData config)
    {
        moveSpeed = config.velocidadeInimigoComum;

        if (guns.Count == 0) guns.AddRange(GetComponentsInChildren<ShootingController>());

        foreach (ShootingController gun in guns)
        {
            if (gun != null) gun.fireRateBase = config.intervaloTiroInimigo;
        }
    }

    private void HandleBehaviour()
    {
        MoveEnemy();
        TryToShoot();
    }

    public void DoBeforeDestroy()
    {
        if (GameManager.instance != null && !GameManager.instance.gameIsOver)
        {
            GameManager.AddScore(scoreValue);
            GameManager.instance.IncrementEnemiesDefeated();
        }
    }

    private void MoveEnemy()
    {
        Vector3 movement = GetDesiredMovement();
        Quaternion rotationToTarget = GetDesiredRotation();

        transform.position += movement;
        transform.rotation = rotationToTarget;
    }

    protected virtual Vector3 GetDesiredMovement()
    {
        switch (movementMode)
        {
            case MovementModes.FollowTarget: return GetFollowPlayerMovement();
            case MovementModes.Scroll: return GetScrollingMovement();
            default: return Vector3.zero;
        }
    }

    protected virtual Quaternion GetDesiredRotation()
    {
        switch (movementMode)
        {
            case MovementModes.FollowTarget: return GetFollowPlayerRotation();
            case MovementModes.Scroll: return GetScrollingRotation();
            default: return transform.rotation;
        }
    }

    private void TryToShoot()
    {
        if (shootMode == ShootMode.ShootAll)
        {
            foreach (ShootingController gun in guns)
            {
                if (gun != null) gun.Fire();
            }
        }
    }

    private Vector3 GetFollowPlayerMovement()
    {
        if (followTarget != null && (followTarget.position - transform.position).magnitude < followRange)
        {
            Vector3 moveDirection = (followTarget.position - transform.position).normalized;
            return moveDirection * moveSpeed * Time.deltaTime;
        }
        return Vector3.zero;
    }

    private Quaternion GetFollowPlayerRotation()
    {
        if (followTarget == null) return transform.rotation;
        Vector3 direction = (followTarget.position - transform.position).normalized;
        float angle = Vector3.SignedAngle(Vector3.down, direction, Vector3.forward);
        return Quaternion.Euler(0, 0, angle);
    }

    private Vector3 GetScrollingMovement()
    {
        scrollDirection = GetScrollDirection();
        return scrollDirection.normalized * moveSpeed * Time.deltaTime;
    }

    private Quaternion GetScrollingRotation() => Quaternion.identity;

    private Vector3 GetScrollDirection()
    {
        Vector3 directionToTarget = turnPosition - transform.position;
        bool overX = (Mathf.Abs(directionToTarget.x) < 0.01f) || (Mathf.Sign(directionToTarget.x) != Mathf.Sign(scrollDirection.x));
        bool overY = (Mathf.Abs(directionToTarget.y) < 0.01f) || (Mathf.Sign(directionToTarget.y) != Mathf.Sign(scrollDirection.y));

        if (overX && overY)
        {
            scrollDirection *= -1;
            turnPosition = (scrollDirection.x > 0 || scrollDirection.y > 0) ? originalPosition + scrollDirection : originalPosition;
        }
        return scrollDirection;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Shield"))
        {
            Health shieldHealth = other.GetComponent<Health>();
            if (shieldHealth != null) shieldHealth.TakeDamage(1);

            Health myHealth = GetComponent<Health>();
            if (myHealth != null) myHealth.TakeDamage(1);
        }
    }
}