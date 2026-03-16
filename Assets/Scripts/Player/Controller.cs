using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    [Header("GameObject/Component References")]
    public Rigidbody2D myRigidbody = null;

    [Header("Movement Variables")]
    public float moveSpeed = 10.0f;
    public float rotationSpeed = 60f;

    [Header("Animation")]
    public Animator turbineAnimator;

    [Header("Input Actions & Controls")]
    public InputAction moveAction;
    public InputAction lookAction;

    public enum AimModes { AimTowardsMouse, AimForwards };
    public AimModes aimMode = AimModes.AimTowardsMouse;

    public enum MovementModes { MoveHorizontally, MoveVertically, FreeRoam, Astroids };
    public MovementModes movementMode = MovementModes.FreeRoam;

    [Header("Shield Settings")]
    public GameObject shieldObject;

    [Header("Audio")]
    public AudioSource startEngineSound;
    public AudioSource engineSound;

    private bool canAimWithMouse => aimMode == AimModes.AimTowardsMouse;
    private bool lockXCoordinate => movementMode == MovementModes.MoveVertically;
    private bool lockYCoordinate => movementMode == MovementModes.MoveHorizontally;

    void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
    }

    private void Start()
    {
        if (myRigidbody == null) myRigidbody = GetComponent<Rigidbody2D>();

        // APLICAÇÃO DA DIFICULDADE (Velocidade)
        if (GameSettings.instance != null && GameSettings.instance.configAtual != null)
        {
            moveSpeed = GameSettings.instance.configAtual.velocidadePlayer;
            Debug.Log($"Controller: Velocidade ajustada para {moveSpeed} pela dificuldade.");
        }
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // 1. Coleta Input de Movimento (Novo Sistema)
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 movementVector = new Vector3(moveInput.x, moveInput.y, 0);

        // 2. Coleta Input de Rotação (Novo Sistema)
        Vector2 lookInput = GetLookPosition();

        // 3. Executa as transformações
        MovePlayer(movementVector);
        LookAtPoint(lookInput);

        // 4. Animação e Som da Turbina
        if (turbineAnimator != null)
        {
            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            turbineAnimator.SetBool("isMoving", isMoving);

            if (engineSound != null)
            {
                if (isMoving && !engineSound.isPlaying)
                {
                    if (startEngineSound != null) startEngineSound.Play();
                    engineSound.Play();
                }
                else if (!isMoving && engineSound.isPlaying)
                {
                    engineSound.Stop();
                }
            }
        }
    }

    public Vector2 GetLookPosition()
    {
        // No Novo Sistema, se a binding for "Pointer > Position", 
        // ele retorna a coordenada do ecrã (pixels).
        if (aimMode != AimModes.AimForwards)
        {
            return lookAction.ReadValue<Vector2>();
        }
        return transform.up;
    }

    private void MovePlayer(Vector3 movement)
    {
        if (movementMode == MovementModes.Astroids)
        {
            // Aplica força na direção que a nave está a olhar
            Vector2 force = transform.up * movement.y * Time.deltaTime * moveSpeed;
            myRigidbody.AddForce(force);

            // Rotação manual via teclado/setas (Astroids Style)
            float rotationChange = movement.x * rotationSpeed * Time.deltaTime;
            transform.Rotate(0, 0, -rotationChange);
        }
        else
        {
            if (lockXCoordinate) movement.x = 0;
            if (lockYCoordinate) movement.y = 0;

            // Movimento direto (FreeRoam / Horizontal / Vertical)
            transform.position += movement * Time.deltaTime * moveSpeed;
        }
    }

    private void LookAtPoint(Vector2 lookPoint)
    {
        if (Time.timeScale > 0 && canAimWithMouse)
        {
            // CONVERSÃO CRUCIAL: Transforma a posição do mouse (ecrã) para o mundo
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(lookPoint.x, lookPoint.y, Camera.main.nearClipPlane));
            Vector2 lookDirection = (Vector2)worldPoint - (Vector2)transform.position;

            if (lookDirection.sqrMagnitude > 0.1f)
            {
                transform.up = lookDirection;
            }
        }
    }

    // --- Métodos de PowerUp (Bomba e Escudo) mantidos ---
    public void GanharEscudo(int vidas)
    {
        if (shieldObject != null)
        {
            ShieldController sHealth = shieldObject.GetComponent<ShieldController>();
            if (sHealth != null) sHealth.ActivarEscudo();
            else shieldObject.SetActive(true);
        }
    }

    public void GanharBomba(int quantidade)
    {
        ScreenClearBomb bombScript = GetComponent<ScreenClearBomb>();
        if (bombScript != null) bombScript.AdicionarBomba(quantidade);
    }
}