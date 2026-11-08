using UnityEngine;
using UnityEngine.InputSystem; // Necessário para a nova API

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // Reseta o input a cada frame
        movementInput = Vector2.zero;

        // Verifica se o teclado está conectado e lê as setas / WASD usando o novo Input System
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            movementInput.y += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            movementInput.y -= 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            movementInput.x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            movementInput.x += 1f;

        // Normaliza para evitar velocidade diagonal maior
        movementInput = movementInput.normalized;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);
    }
}