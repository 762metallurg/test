using UnityEngine;

[CreateAssetMenu(fileName = "PlayerLocomotionData", menuName = "ScriptableObjects/PlayerLocomotionData", order = 1)]
public class PlayerLocomotionDataSO : ScriptableObject
{
    [Header("--- Movement Stats ---")]
    public float walkingSpeed = 3f;  // <--- ДОБАВЬ ЭТУ СТРОКУ
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float acceleration = 80f;
    public float rotationSpeed = 720f;

    [Header("--- Floating (Levitation) ---")]
    public float rideHeight = 1.1f;
    public float rideSpringStrength = 2000f;
    public float rideSpringDamper = 100f;
    public float rayLength = 1.5f;
    public LayerMask groundLayer;

    [Header("--- Jump & Air ---")]
    public float jumpHeight = 2f;
    public float airControl = 0.8f;
    public float jumpCooldown = 0.2f;
}