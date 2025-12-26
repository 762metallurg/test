using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStats", menuName = "Player/Player Stats")]
public class PlayerLocomotionDataSO : ScriptableObject
{
    [Header("--- Movement Stats ---")]
    public float moveSpeed = 8f;
    public float sprintSpeed = 12f;
    public float acceleration = 40f;
    public float rotationSpeed = 360f;

    [Header("--- Floating (Levitation) ---")]
    public float rideHeight = 1.1f;
    public float rideSpringStrength = 2000f;
    public float rideSpringDamper = 100f;
    public float rayLength = 1.5f; // Длина луча для проверки земли

    [Header("--- Layers ---")]
    // LayerMask можно хранить тут, но иногда удобнее на префабе. 
    // Давай вынесем, чтобы конфиг был полным.
    public LayerMask groundLayer; 
}