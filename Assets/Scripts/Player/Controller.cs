using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Terresquall;

public class Controller : MonoBehaviour
{
    [Header("GameObject/Component References")]
    public Rigidbody2D myRigidbody = null;

    [Header("Mobile Controls")]
    public VirtualJoystick mobileMoveJoystick;
    public VirtualJoystick mobileLookJoystick;

    [Header("Movement Variables")]
    public float moveSpeed = 10.0f;
    public float rotationSpeed = 60f;

    [Header("Smooth Movement")]
    public float smoothTime = 0.15f;
    private Vector2 currentInputVector;
    private Vector2 smoothInputVelocity;

    [Header("Animation")]
    public Animator turbineAnimator;

    [Header("Input Actions & Controls")]
    public InputAction moveAction;
    public InputAction lookAction;

    public enum AimModes { AimTowardsMouse, AimForwards, DualStickMobile };
    [Header("Modo de Mira (Automático no Start)")]
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

    private ShootingController shootingController;

    void OnEnable() { moveAction.Enable(); lookAction.Enable(); }
    void OnDisable() { moveAction.Disable(); lookAction.Disable(); }

    private void Start()
    {
        if (myRigidbody == null) myRigidbody = GetComponent<Rigidbody2D>();
        shootingController = GetComponent<ShootingController>();

        // --- LÓGICA DE PLATAFORMA REAL ---
#if UNITY_IOS || UNITY_ANDROID || UNITY_TVOS
        aimMode = AimModes.DualStickMobile;
        ConfigurarVisibilidadeJoysticks(true);
        Debug.Log("Controller: Mobile detectado. Ativando Joysticks.");
#else
        aimMode = AimModes.AimTowardsMouse;
        ConfigurarVisibilidadeJoysticks(false);
        Debug.Log("Controller: PC/Desktop detectado. Joysticks ocultados.");
#endif

        if (GameSettings.instance != null && GameSettings.instance.configAtual != null)
        {
            moveSpeed = GameSettings.instance.configAtual.velocidadePlayer;
        }

        // FÚRIA: O jogador inicia com o Shield no máximo
        GanharEscudo(3);
    }

    private void ConfigurarVisibilidadeJoysticks(bool mostrar)
    {
        if (mobileMoveJoystick != null) mobileMoveJoystick.gameObject.SetActive(mostrar);
        if (mobileLookJoystick != null) mobileLookJoystick.gameObject.SetActive(mostrar);
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        Vector2 moveInput = Vector2.zero;
        Vector2 lookInputVector = Vector2.zero;

        // 1. Coleta Input de Movimento (Prioriza Joystick se estiver no modo Mobile)
        if (aimMode == AimModes.DualStickMobile && mobileMoveJoystick != null && mobileMoveJoystick.GetAxis().sqrMagnitude > 0.01f)
        {
            moveInput = mobileMoveJoystick.GetAxis();
            moveInput = Vector2.ClampMagnitude(moveInput, 1f);
        }
        else
        {
            // No PC ou se o joystick mobile não estiver sendo tocado, usa o WASD/Teclado
            moveInput = moveAction.ReadValue<Vector2>();
        }

        // 2. Coleta Input de Mira/Ataque
        if (aimMode == AimModes.DualStickMobile && mobileLookJoystick != null)
        {
            lookInputVector = mobileLookJoystick.GetAxis();
            
            if (lookInputVector.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(lookInputVector.y, lookInputVector.x) * Mathf.Rad2Deg - 90f;
                Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed * 0.2f);
                
                if (shootingController != null) shootingController.Fire();
            }
        }
        else if (aimMode == AimModes.AimTowardsMouse)
        {
            // PC: Vira sempre para o curso do rato
            Vector2 mousePos = lookAction.ReadValue<Vector2>();
            LookAtPoint(mousePos);
        }

        // 3. Suavização do Movimento
        currentInputVector = Vector2.SmoothDamp(
            currentInputVector,
            moveInput,
            ref smoothInputVelocity,
            smoothTime
        );

        Vector3 movementVector = new Vector3(currentInputVector.x, currentInputVector.y, 0);

        // 4. Executa Movimento
        MovePlayer(movementVector);

        // 5. Animação e Som
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

    private void MovePlayer(Vector3 movement)
    {
        if (movementMode == MovementModes.Astroids)
        {
            Vector2 force = transform.up * movement.y * Time.deltaTime * moveSpeed;
            myRigidbody.AddForce(force);
            float rotationChange = movement.x * rotationSpeed * Time.deltaTime;
            transform.Rotate(0, 0, -rotationChange);
        }
        else
        {
            if (lockXCoordinate) movement.x = 0;
            if (lockYCoordinate) movement.y = 0;
            transform.position += movement * Time.deltaTime * moveSpeed;
        }
    }

    private void LookAtPoint(Vector2 lookPoint)
    {
        if (Time.timeScale > 0 && Camera.main != null)
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(lookPoint.x, lookPoint.y, Camera.main.nearClipPlane));
            Vector2 lookDirection = (Vector2)worldPoint - (Vector2)transform.position;
            if (lookDirection.sqrMagnitude > 0.1f) transform.up = lookDirection;
        }
    }

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