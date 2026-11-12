using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Mobile Joystick UI")]
    [SerializeField] private RectTransform joystickBackground;
    [SerializeField] private RectTransform joystickHandle;
    [SerializeField] private float handleRange = 75f;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 joystickInput;
    private Canvas parentCanvas;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (joystickBackground != null)
        {
            parentCanvas = joystickBackground.GetComponentInParent<Canvas>();
            joystickBackground.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        Vector2 keyboardInput = Vector2.zero;
        var keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) keyboardInput.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) keyboardInput.y -= 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) keyboardInput.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) keyboardInput.x += 1f;
        }

        // Prioriza o joystick se estiver em uso; caso contrário, usa o teclado
        if (joystickInput.sqrMagnitude > 0.01f)
            movementInput = joystickInput;
        else
            movementInput = keyboardInput.normalized;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);
    }

    // --- Métodos chamados pelos eventos de toque na tela ---

    public void OnTouchDown(BaseEventData data)
    {
        if (joystickBackground == null) return;

        PointerEventData pointerData = (PointerEventData)data;
        joystickBackground.gameObject.SetActive(true);
        joystickBackground.position = pointerData.position;
        joystickHandle.anchoredPosition = Vector2.zero;
    }

    public void OnTouchDrag(BaseEventData data)
    {
        if (joystickBackground == null) return;

        PointerEventData pointerData = (PointerEventData)data;
        float scaleFactor = parentCanvas != null ? parentCanvas.scaleFactor : 1f;

        Vector2 direction = (pointerData.position - (Vector2)joystickBackground.position) / scaleFactor;
        joystickInput = Vector2.ClampMagnitude(direction / handleRange, 1f);
        joystickHandle.anchoredPosition = joystickInput * handleRange;
    }

    public void OnTouchUp(BaseEventData data)
    {
        joystickInput = Vector2.zero;
        if (joystickHandle != null) joystickHandle.anchoredPosition = Vector2.zero;
        if (joystickBackground != null) joystickBackground.gameObject.SetActive(false);
    }
}