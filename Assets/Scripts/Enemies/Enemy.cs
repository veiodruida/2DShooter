using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which controls enemy behaviour
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The speed at which the enemy moves.")]
    public float moveSpeed = 5.0f;
    [Tooltip("The score value for defeating this enemy")]
    public int scoreValue = 5;

    [Header("Following Settings")]
    [Tooltip("The transform of the object that this enemy should follow.")]
    public Transform followTarget = null;
    [Tooltip("The distance at which the enemy begins following the follow target.")]
    public float followRange = 10.0f;

    [Header("Shooting")]
    [Tooltip("The enemy's gun components")]
    public List<ShootingController> guns = new List<ShootingController>();

    /// <summary>
    /// Enum to help with shooting modes
    /// </summary>
    public enum ShootMode { None, ShootAll };

    [Tooltip("The way the enemy shoots:\n" +
        "None: Enemy does not shoot.\n" +
        "ShootAll: Enemy fires all guns whenever it can.")]
    public ShootMode shootMode = ShootMode.ShootAll;

    /// <summary>
    /// Enum to help wih different movement modes
    /// </summary>
    public enum MovementModes { NoMovement, FollowTarget, Scroll };

    [Tooltip("The way this enemy will move\n" +
        "NoMovement: This enemy will not move.\n" +
        "FollowTarget: This enemy will follow the assigned target.\n" +
        "Scroll: This enemy will move in one horizontal direction only.")]
    public MovementModes movementMode = MovementModes.FollowTarget;

    //The direction that this enemy will try to scroll if it is set as a scrolling enemy.
    [SerializeField] private Vector3 scrollDirection = Vector3.right;

    /// <summary>
    /// Description:
    /// Standard Unity function called after update every frame
    /// Inputs: 
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    private void LateUpdate()
    {
        HandleBehaviour();       
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once before the first call to Update
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void Start()
    {
        // Se o alvo sumiu ou está vazio, ele encontra o player sozinho pela Tag
        if (followTarget == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) followTarget = p.transform;
        }

        // Mantém a tua lógica de Scroll
        if (movementMode == MovementModes.Scroll)
        {
            originalPosition = transform.position;
            turnPosition = originalPosition + scrollDirection;
        }

        ConfigurarDificuldade();
    }

    void ConfigurarDificuldade()
    {
        // Se ainda não carregou, tentamos carregar manualmente agora
        if (GameSettings.instance == null || GameSettings.instance.configAtual == null)
        {
            // Tenta encontrar o objeto na cena caso a instância não tenha sido definida
            GameSettings settings = FindFirstObjectByType<GameSettings>();

            if (settings != null && settings.configAtual != null)
            {
                AplicarConfiguracoes(settings.configAtual);
            }
            else
            {
                Debug.LogWarning($"<color=orange>Aviso:</color> {name} usará valores manuais pois o Config não está pronto.");
            }
        }
        else
        {
            AplicarConfiguracoes(GameSettings.instance.configAtual);
        }
    }

    void AplicarConfiguracoes(DifficultyData config)
    {
        moveSpeed = config.velocidadeInimigoComum;

        if (guns.Count == 0) guns.AddRange(GetComponentsInChildren<ShootingController>());

        foreach (ShootingController gun in guns)
        {
            if (gun != null)
            {
                gun.fireRateBase = config.intervaloTiroInimigo;
                Debug.Log($"<color=green>Sucesso:</color> {name} configurado via arquivo.");
            }
        }
    }

    /// <summary>
    /// Description:
    /// Handles moving and shooting in accordance with the enemy's set behaviour
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    private void HandleBehaviour()
    {
        MoveEnemy();
        // Attempt to shoot, according to this enemy's shooting mode
        TryToShoot();
    }

    /// <summary>
    /// Description:
    /// This is meant to be called before destroying the gameobject associated with this script
    /// It can not be replaced with OnDestroy() because of Unity's inability to distiguish between unloading a scene
    /// and destroying the gameobject from the Destroy function
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    public void DoBeforeDestroy()
    {
        AddToScore();
        IncrementEnemiesDefeated();
    }

    /// <summary>
    /// Description:
    /// Adds to the game manager's score the score associated with this enemy if one exists
    /// Input:
    /// None
    /// Returns:
    /// void (no return)
    /// </summary>
    private void AddToScore()
    {
        if (GameManager.instance != null && !GameManager.instance.gameIsOver)
        {
            GameManager.AddScore(scoreValue);
        }
    }

    /// <summary>
    /// Description:
    /// Increments the game manager's number of defeated enemies
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void IncrementEnemiesDefeated()
    {
        if (GameManager.instance != null && !GameManager.instance.gameIsOver)
        {
            GameManager.instance.IncrementEnemiesDefeated();
        }       
    }

    /// <summary>
    /// Description:
    /// Moves the enemy and rotates it according to it's movement mode
    /// Inputs: none
    /// Returns: 
    /// void (no return)
    /// </summary>
    private void MoveEnemy()
    {
        // Determine correct movement
        Vector3 movement = GetDesiredMovement();

        // Determine correct rotation
        Quaternion rotationToTarget = GetDesiredRotation();

        // Move and rotate the enemy
        transform.position = transform.position + movement;
        transform.rotation = rotationToTarget;
    }

    /// <summary>
    /// Description:
    /// Calculates the movement of this enemy
    /// Inputs: 
    /// none
    /// Returns: 
    /// Vector3
    /// </summary>
    /// <returns>Vector3: The movement of this enemy</returns>
    protected virtual Vector3 GetDesiredMovement()
    {
        Vector3 movement;
        switch(movementMode)
        {
            case MovementModes.FollowTarget:
                movement = GetFollowPlayerMovement();
                break;
            case MovementModes.Scroll:
                movement = GetScrollingMovement();
                break;
            default:
                movement = Vector3.zero;
                break;
        }
        return movement;
    }

    /// <summary>
    /// Description:
    /// Calculates and returns the desired rotation of this enemy
    /// Inputs: 
    /// none
    /// Returns: 
    /// Quaternion
    /// </summary>
    /// <returns>Quaternion: The desired rotation</returns>
    protected virtual Quaternion GetDesiredRotation()
    {
        Quaternion rotation;
        switch (movementMode)
        {
            case MovementModes.FollowTarget:
                rotation = GetFollowPlayerRotation();
                break;
            case MovementModes.Scroll:
                rotation = GetScrollingRotation();
                break;
            default:
                rotation = transform.rotation;
                break;
        }
        return rotation;
    }

    /// <summary>
    /// Description:
    /// Tries to fire all referenced ShootingController scripts
    /// depends on shootMode variable
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    private void TryToShoot()
    {
        switch (shootMode)
        {
            case ShootMode.None:
                break;
            case ShootMode.ShootAll:
                foreach (ShootingController gun in guns)
                {
                    gun.Fire();
                }
                break;
        }
    }

    /// <summary>
    /// Description:
    /// The direction and magnitude of the enemy's desired movement in follow mode
    /// Inputs: 
    /// none
    /// Returns: 
    /// Vector3
    /// </summary>
    /// <returns>Vector3: The movement to be used in follow movement mode.</returns>
    private Vector3 GetFollowPlayerMovement()
    {
        // Check if the target is in range, then move
        if (followTarget != null && (followTarget.position - transform.position).magnitude < followRange)
        {
            Vector3 moveDirection = (followTarget.position - transform.position).normalized;
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            return movement;
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Description
    /// The desired rotation of follow movement mode
    /// Inputs: 
    /// none
    /// Returns: 
    /// Quaternion
    /// </summary>
    /// <returns>Quaternion: The rotation to be used in follow movement mode.</returns>
    private Quaternion GetFollowPlayerRotation()
    {
        if (followTarget == null) return transform.rotation;

        // Se o tiro sai "de costas", mude Vector3.down para Vector3.up aqui:
        Vector3 direction = (followTarget.position - transform.position).normalized;
        float angle = Vector3.SignedAngle(Vector3.down, direction, Vector3.forward);

        return Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Description:
    /// The direction and magnitude of the enemy's desired movement in scrolling mode
    /// Inputs: 
    /// none
    /// Returns: 
    /// Vector3
    /// </summary>
    /// <returns>Vector3: The movement to be used in scrolling movement mode.</returns>
    private Vector3 GetScrollingMovement()
    {
        scrollDirection = GetScrollDirection();
        Vector3 movement = scrollDirection.normalized * moveSpeed * Time.deltaTime;
        return movement;
    }

    /// <summary>
    /// Description
    /// The desired rotation of scrolling movement mode
    /// Inputs: 
    /// none
    /// Returns: 
    /// Quaternion
    /// </summary>
    /// <returns>Quaternion: The rotation to be used in scrolling movement mode</returns>
    private Quaternion GetScrollingRotation()
    {
        return Quaternion.identity;
    }

    private Vector3 originalPosition;
    private Vector3 turnPosition;
    /// <summary>
    /// Description:
    /// Determines the direction to move in with scrolling movement mode
    /// Inputs: 
    /// none
    /// Returns: 
    /// Vector3
    /// </summary>
    /// <returns>Vector3: The desired scroll direction</returns>
    private Vector3 GetScrollDirection()
    {
        bool overX = false;
        bool overY = false;
        bool overZ = false;

        Vector3 directionFromCurrentPositionToTarget = turnPosition - transform.position;

        if ((directionFromCurrentPositionToTarget.x <= 0.0001 && directionFromCurrentPositionToTarget.x >= -0.0001) || Mathf.Sign(directionFromCurrentPositionToTarget.x) != Mathf.Sign(scrollDirection.x))
        {
            overX = true;
            transform.position = new Vector3(turnPosition.x, transform.position.y, transform.position.z);
        }
        if ((directionFromCurrentPositionToTarget.y <= 0.0001 && directionFromCurrentPositionToTarget.y >= -0.0001) || Mathf.Sign(directionFromCurrentPositionToTarget.y) != Mathf.Sign(scrollDirection.y))
        {
            overY = true;
            transform.position = new Vector3(transform.position.x, turnPosition.y, transform.position.z);
        }
        if ((directionFromCurrentPositionToTarget.z <= 0.0001 && directionFromCurrentPositionToTarget.z >= -0.0001) || Mathf.Sign(directionFromCurrentPositionToTarget.z) != Mathf.Sign(scrollDirection.z))
        {
            overZ = true;
            transform.position = new Vector3(transform.position.x, transform.position.y, turnPosition.z);
        }

        if (overX && overY && overZ)
        {
            turnPosition = originalPosition - scrollDirection;
            return scrollDirection * -1;
        }
        return scrollDirection;
    }

   

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Se eu (Inimigo/Bomba) bater no Escudo
        if (other.CompareTag("Shield"))
        {
            // 1. Dou dano no Escudo
            Health shieldHealth = other.GetComponent<Health>();
            if (shieldHealth != null) shieldHealth.TakeDamage(1);

            // 2. Eu recebo dano por ter batido no escudo
            Health myHealth = GetComponent<Health>();
            if (myHealth != null) 
            {
                // Se o inimigo tiver 1 de vida, ele morre aqui e o Health.cs 
                // vai criar a animação de destruição (Die) automaticamente.
                myHealth.TakeDamage(1); 
            }
        }
    }

    private void OnDestroy()
    {
        // Se o jogo não estiver a fechar, avisa quem o destruiu
        if (!gameObject.scene.isLoaded) return;
        //Debug.Log("<color=yellow>INIMIGO DESTRUÍDO!</color> Verifique se foi o Health ou outro script.");


    }

}
