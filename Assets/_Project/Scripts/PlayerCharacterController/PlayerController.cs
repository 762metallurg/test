using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("--- Configuration ---")]
    public PlayerLocomotionDataSO stats;
    
    [Header("--- References ---")]
    public Transform cameraTransform;

    [Header("--- Debug Info ---")]
    public bool isGrounded;
    public bool useFloating;   
    public bool lockMovement; 

    [Header("--- State Flags ---")]
    public float lastJumpTime; // Вернул таймер прыжка
    public float lastDodgeTime;

    // === ПРОКСИ-СВОЙСТВА ===
    public float MoveSpeed => stats.moveSpeed;
    public float SprintSpeed => stats.sprintSpeed;
    public float WalkingSpeed => stats.walkingSpeed; // Вернул для ходьбы
    public float Acceleration => stats.acceleration;
    public float RotationSpeed => stats.rotationSpeed;
    
    // Левитация
    public float RideHeight => stats.rideHeight;
    public float RideSpringStrength => stats.rideSpringStrength;
    public float RideSpringDamper => stats.rideSpringDamper;
    public float RayLength => stats.rayLength;
    public LayerMask GroundLayer => stats.groundLayer;

    // Прыжки
    public float JumpHeight => stats.jumpHeight;
    public float AirControl => stats.airControl;

    // === КОМПОНЕНТЫ ===
    public Rigidbody RB { get; private set; }
    public InputActions Input { get; private set; }
    //public PlayerAnimationManager AnimationManager { get; private set; }
    
    // === STATE MACHINES ===
    public PlayerStateMachine LocomotionSM { get; private set; }
    public PlayerStateMachine ActionSM { get; private set; }

    // === STATES ===
    public PlayerLocomotionIdleState LocoIdle { get; private set; }
    public PlayerLocomotionMoveState LocoMove { get; private set; }
    public PlayerLocomotionSprintState LocoSprint { get; private set; }
    public PlayerLocomotionJumpState LocoJump { get; private set; } // Вернул
    public PlayerLocomotionAirState LocoAir { get; private set; }   // Вернул
    public PlayerActionNoneState ActionNone { get; private set; }
    public PlayerLocomotionDodgeState LocoDodge { get; private set; }

    // === INPUT ===
    public Vector2 MoveInput { get; private set; }
    public bool IsSprintingInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool DodgeInput { get; private set; }
    public bool IsWalking { get; private set; } // Вернул флаг ходьбы
    public float MoveAmount => Mathf.Clamp01(MoveInput.magnitude); // Вернул для аналогового стика

    private void Awake()
    {
        RB = GetComponent<Rigidbody>();
        RB.freezeRotation = true; 
        RB.useGravity = true;
        RB.interpolation = RigidbodyInterpolation.Interpolate;
        
        //AnimationManager = GetComponent<PlayerAnimationManager>();

        if (cameraTransform == null && Camera.main != null) 
            cameraTransform = Camera.main.transform;

        if (stats == null) 
            Debug.LogError("PLAYER CONTROLLER: Не назначен файл Stats (ScriptableObject)!");

        Input = new InputActions();
        
        // --- ПОДПИСКИ НА INPUT ---
        Input.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        Input.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        Input.Player.Sprint.performed += ctx => IsSprintingInput = true;
        Input.Player.Sprint.canceled += ctx => IsSprintingInput = false;
        
        Input.Player.Jump.performed += ctx => JumpInput = true;
        Input.Player.Jump.canceled += ctx => JumpInput = false;
        
        // Walk Toggle (Interactions: Press Only или через код)
        Input.Player.WalkToggle.performed += ctx => IsWalking = !IsWalking;

        // Dodge
        Input.Player.Dodge.performed += ctx => DodgeInput = true;
        Input.Player.Dodge.canceled += ctx => DodgeInput = false;

        LocomotionSM = new PlayerStateMachine();
        ActionSM = new PlayerStateMachine();

        // === ИНИЦИАЛИЗАЦИЯ ВСЕХ СОСТОЯНИЙ (ПРОВЕРЬ ЭТОТ БЛОК!) ===
        
        // 1. Базовые (ты мог их случайно удалить)
        LocoIdle = new PlayerLocomotionIdleState(this, LocomotionSM);
        LocoMove = new PlayerLocomotionMoveState(this, LocomotionSM);
        LocoSprint = new PlayerLocomotionSprintState(this, LocomotionSM);
        
        // 2. Воздушные
        LocoJump = new PlayerLocomotionJumpState(this, LocomotionSM);
        LocoAir = new PlayerLocomotionAirState(this, LocomotionSM);
        
        // 3. Додж
        LocoDodge = new PlayerLocomotionDodgeState(this, LocomotionSM);
        
        // 4. Экшен (пока пустышка)
        ActionNone = new PlayerActionNoneState(this, ActionSM);
    }

    private void OnEnable() => Input.Enable();
    private void OnDisable() => Input.Disable();

    private void Start()
    {
        LocomotionSM.Initialize(LocoIdle);
        ActionSM.Initialize(ActionNone);
    }

    private void Update()
    {
        LocomotionSM.CurrentState.LogicUpdate();
        ActionSM.CurrentState.LogicUpdate();


        // ВРЕМЕННЫЙ ДЕБАГ
        if (JumpInput) Debug.Log("Кнопка нажата!");
        if (JumpInput && isGrounded) Debug.Log("Готов к прыжку (Input + Ground)!");
    }

    private void FixedUpdate()
    {
        RB.angularVelocity = Vector3.zero;

        LocomotionSM.CurrentState.PhysicsUpdate();
        ActionSM.CurrentState.PhysicsUpdate();
        CheckGround();

        // ВАЖНО: Добавил проверку jumpCooldown.
        // Без неё левитация включится мгновенно и не даст взлететь.
        if (useFloating && Time.time > lastJumpTime + stats.jumpCooldown) 
        {
            ApplyFloatingForce();
        }
    }

    public void HandleMovement(float targetSpeed)
    {
        if (lockMovement) return;

        Vector3 camFwd = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camFwd.y = 0;
        camRight.y = 0;
        camFwd.Normalize();
        camRight.Normalize();

        Vector3 targetDirection = (camFwd * MoveInput.y + camRight * MoveInput.x);

        if (targetDirection.magnitude > 0.01f)
        {
            targetDirection.Normalize();

            // Умный поворот
            float angle = Vector3.Angle(transform.forward, targetDirection);
            // Если угол большой - вращаемся быстрее (RotationSpeed * 2)
            float currentRotSpeed = (angle > 90f) ? RotationSpeed * 2f : RotationSpeed;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion nextRotation = Quaternion.RotateTowards(
                transform.rotation, 
                targetRotation, 
                currentRotSpeed * Time.fixedDeltaTime
            );
            RB.MoveRotation(nextRotation);

            Vector3 targetVel = targetDirection * targetSpeed;
            Vector3 currentVel = RB.linearVelocity;
            Vector3 velDiff = targetVel - currentVel;
            velDiff.y = 0; 

            RB.AddForce(velDiff * Acceleration, ForceMode.Acceleration);
        }
        else
        {
            // Торможение
            Vector3 currentVel = RB.linearVelocity;
            Vector3 antiSlide = new Vector3(-currentVel.x * 10f, 0, -currentVel.z * 10f); 
            RB.AddForce(antiSlide, ForceMode.Acceleration);
        }
    }

    // Твоя версия физики (оставил без изменений, как ты просил)
    private void ApplyFloatingForce()
    {
        Vector3 rayOrigin = transform.position + Vector3.up; 
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, RayLength + 1f, GroundLayer))
        {
            float distanceToGround = hit.distance - 1f; 
            
            float dir = RideHeight - distanceToGround;
            
            float rayDirVel = Vector3.Dot(Vector3.down, RB.linearVelocity);
            float otherDirVel = (hit.rigidbody != null) ? Vector3.Dot(Vector3.down, hit.rigidbody.linearVelocity) : 0f;
            float relVel = rayDirVel - otherDirVel;

            float force = (dir * RideSpringStrength) - (relVel * RideSpringDamper);
            
            RB.AddForce(Vector3.down * force);
        }
    }

    private void CheckGround() 
    {
        isGrounded = Physics.Raycast(transform.position + Vector3.up, Vector3.down, RayLength + 1f, GroundLayer);
    }

    public void ApplyFriction(float frictionAmount)
    {
        Vector3 vel = RB.linearVelocity; 
        RB.linearVelocity = new Vector3(
            vel.x * (1 - frictionAmount), 
            vel.y, 
            vel.z * (1 - frictionAmount)
        );
    }

    // Метод для прыжка
    public void HandleJump()
    {
        lastJumpTime = Time.time;
        float jumpVelocity = Mathf.Sqrt(JumpHeight * -2f * Physics.gravity.y);
        
        Vector3 playerVelocity = RB.linearVelocity;
        playerVelocity.y = jumpVelocity;
        RB.linearVelocity = playerVelocity;
    }
}