using UnityEngine;

public class ShieldController : MonoBehaviour
{
    private Health health;
    private Controller ownerController;
    private Collider2D shieldCollider;

    [Header("Collision Repulsion")]
    [SerializeField] private float boundarySeparationPadding = 0.05f;
    [SerializeField] private float boundaryImpulseToDistanceFactor = 0.02f;

    private void Awake()
    {
        health = GetComponent<Health>();
        shieldCollider = GetComponent<Collider2D>();
        ownerController = GetComponentInParent<Controller>();
    }

    // Mudamos para OnEnable para garantir que ele reseta sempre que o item e pego
    public void ActivarEscudo()
    {
        // Se ainda nao temos a referencia do Health, procuramos agora
        if (health == null) health = GetComponent<Health>();

        // 1. Ativamos o objeto primeiro
        gameObject.SetActive(true);

        // 2. Resetamos a vida imediatamente
        if (health != null)
        {
            //health.SetVidaManual(3); // Define 3 vidas
            health.currentLives = health.maximumLives; // Restaura para o maximo
            health.currentHealth = health.defaultHealth; // Restaura a vida atual para o valor padrao
        }
        if (UIManager.instance != null) UIManager.instance.UpdateUI();

        Debug.Log("<color=cyan>ESCUDO REATIVADO!</color> vida: "+ health.currentLives);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollisionRepulsion(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        HandleCollisionRepulsion(other);
    }

    private void HandleCollisionRepulsion(Collider2D other)
    {
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy) return;
        if (!ShouldApplyRepulsion(other)) return;
        if (ownerController == null || shieldCollider == null) return;

        Damage collisionDamage = other.GetComponent<Damage>();
        if (collisionDamage == null) collisionDamage = other.GetComponentInParent<Damage>();

        float repulsionForce = collisionDamage != null ? collisionDamage.repulsionForce : 0f;
        if (repulsionForce <= 0f) return;

        Vector2 shieldCenter = shieldCollider.bounds.center;
        Vector2 closestPoint = other.ClosestPoint(shieldCenter);
        Vector2 pushDirection = shieldCenter - closestPoint;

        if (pushDirection.sqrMagnitude < 0.0001f)
        {
            pushDirection = shieldCenter - (Vector2)other.bounds.center;
        }

        if (pushDirection.sqrMagnitude < 0.0001f)
        {
            pushDirection = Vector2.up;
        }

        pushDirection.Normalize();

        ColliderDistance2D distance = shieldCollider.Distance(other);
        float penetrationDepth = distance.isOverlapped ? Mathf.Abs(distance.distance) : 0f;
        float immediatePush = penetrationDepth + boundarySeparationPadding + (repulsionForce * boundaryImpulseToDistanceFactor);

        ownerController.ApplyImmediateRepulsion(pushDirection * immediatePush, pushDirection * repulsionForce);
    }

    private bool ShouldApplyRepulsion(Collider2D other)
    {
        return other.CompareTag("Boundary")
            || other.CompareTag("Boss")
            || other.CompareTag("BossShield");
    }
}
