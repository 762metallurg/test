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

    // === ПРОКСИ-СВОЙСТВА (Для совместимости со StateMachine) ===
    // Твои стейты (LocoMove и т.д.) будут обращаться к этим свойствам как раньше,
    // но данные теперь берутся из файла stats.
    public float MoveSpeed => stats.moveSpeed;
    public float SprintSpeed => stats.sprintSpeed;
    public float Acceleration => stats.acceleration;
    public float RotationSpeed => stats.rotationSpeed;
    
    // Левитация
    public float RideHeight => stats.rideHeight;
    public float RideSpringStrength => stats.rideSpringStrength;
    public float RideSpringDamper => stats.rideSpringDamper;
    public float RayLength => stats.rayLength;
    public LayerMask GroundLayer => stats.groundLayer;

    // === КОМПОНЕНТЫ ===
    public Rigidbody RB { get; private set; }
    public InputActions Input { get; private set; }
    
    // === STATE MACHINES ===
    public PlayerStateMachine LocomotionSM { get; private set; }
    public PlayerStateMachine ActionSM { get; private set; }

    // === STATES ===
    public PlayerLocomotionIdleState LocoIdle { get; private set; }
    public PlayerLocomotionMoveState LocoMove { get; private set; }
    public PlayerLocomotionSprintState LocoSprint { get; private set; }
    public PlayerActionNoneState ActionNone { get; private set; }

    // === INPUT ===
    public Vector2 MoveInput { get; private set; }
    public bool IsSprintingInput { get; private set; }

    private void Awake()
    {
        RB = GetComponent<Rigidbody>();
        RB.freezeRotation = true; 
        RB.useGravity = true;
        RB.interpolation = RigidbodyInterpolation.Interpolate;

        if (cameraTransform == null && Camera.main != null) 
            cameraTransform = Camera.main.transform;

        // Валидация: если забыл вставить Stats, ругаемся в консоль
        if (stats == null) 
            Debug.LogError("PLAYER CONTROLLER: Не назначен файл Stats (ScriptableObject)!");

        Input = new InputActions();
        Input.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        Input.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        Input.Player.DodgeSprint.performed += ctx => IsSprintingInput = true;
        Input.Player.DodgeSprint.canceled += ctx => IsSprintingInput = false;

        LocomotionSM = new PlayerStateMachine();
        ActionSM = new PlayerStateMachine();

        LocoIdle = new PlayerLocomotionIdleState(this, LocomotionSM);
        LocoMove = new PlayerLocomotionMoveState(this, LocomotionSM);
        LocoSprint = new PlayerLocomotionSprintState(this, LocomotionSM);
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
    }

    private void FixedUpdate()
    {
        RB.angularVelocity = Vector3.zero;

        LocomotionSM.CurrentState.PhysicsUpdate();
        ActionSM.CurrentState.PhysicsUpdate();
        CheckGround();

        if (useFloating) ApplyFloatingForce();
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

            // Используем свойство-прокси RotationSpeed (которое берет из SO)
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion nextRotation = Quaternion.RotateTowards(
                transform.rotation, 
                targetRotation, 
                RotationSpeed * Time.fixedDeltaTime
            );
            RB.MoveRotation(nextRotation);

            Vector3 targetVel = targetDirection * targetSpeed;
            Vector3 currentVel = RB.linearVelocity;
            Vector3 velDiff = targetVel - currentVel;
            velDiff.y = 0; 

            // Используем свойство Acceleration
            RB.AddForce(velDiff * Acceleration, ForceMode.Acceleration);
        }
        else
        {
            Vector3 currentVel = RB.linearVelocity;
            Vector3 antiSlide = new Vector3(-currentVel.x * 10f, 0, -currentVel.z * 10f); 
            RB.AddForce(antiSlide, ForceMode.Acceleration);
        }
    }

    private void ApplyFloatingForce()
    {
        // Используем RayLength из SO
        Vector3 rayOrigin = transform.position + Vector3.up; 
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, RayLength + 1f, GroundLayer))
        {
            float distanceToGround = hit.distance - 1f; 
            
            // Используем RideHeight из SO
            float dir = RideHeight - distanceToGround;
            
            float rayDirVel = Vector3.Dot(Vector3.down, RB.linearVelocity);
            float otherDirVel = (hit.rigidbody != null) ? Vector3.Dot(Vector3.down, hit.rigidbody.linearVelocity) : 0f;
            float relVel = rayDirVel - otherDirVel;

            // Используем RideSpringStrength и Damper из SO
            float force = (dir * RideSpringStrength) - (relVel * RideSpringDamper);
            
            RB.AddForce(Vector3.down * force);
        }
    }

    private void CheckGround() 
    {
        // Используем RayLength и GroundLayer из SO
        isGrounded = Physics.Raycast(transform.position + Vector3.up, Vector3.down, RayLength + 1f, GroundLayer);
    }

    // === ФИЗИКА ТРЕНИЯ (Чтобы не скользил в Idle) ===
    public void ApplyFriction(float frictionAmount)
    {
        // Если используешь Unity 6, то linearVelocity, если старую - velocity
        Vector3 vel = RB.linearVelocity; 
        
        // Гасим инерцию только по X и Z, высоту (Y) не трогаем
        RB.linearVelocity = new Vector3(
            vel.x * (1 - frictionAmount), 
            vel.y, 
            vel.z * (1 - frictionAmount)
        );
    }
}