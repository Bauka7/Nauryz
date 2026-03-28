using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform playerCamera;
    
    public float speed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public float mouseSensitivity = 100f;

    Vector3 velocity;
    bool isGrounded;
    float xRotation = 0f;

    void Start()
    {
        // Блокируем курсор мыши в центре экрана и скрываем его
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- 1. ПРОВЕРКА ЗЕМЛИ ---
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // --- 2. ВРАЩЕНИЕ КАМЕРЫ (МЫШЬ) ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Ограничиваем, чтобы не сломать шею

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // Камера смотрит вверх/вниз
        transform.Rotate(Vector3.up * mouseX); // Персонаж поворачивается влево/вправо

        // --- 3. ДВИЖЕНИЕ (WASD) ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // --- 4. ПРЫЖОК (Пробел) ---
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Применяем гравитацию
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}