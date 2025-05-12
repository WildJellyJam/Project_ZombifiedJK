using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    public float moveSpeed = 10f; // 移動速度

    void Start()
    {
        // 獲取CharacterController組件
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController組件未找到，請在角色上添加CharacterController！");
        }
    }

    void Update()
    {
        // 獲取WASD輸入
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D鍵：左/右
        float moveZ = Input.GetAxisRaw("Vertical");   // W/S鍵：前/後

        // 計算移動方向（基於世界空間）
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        // 如果有移動輸入，移動角色並調整朝向
        if (moveDirection.magnitude > 0)
        {
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);

            // 讓角色朝向移動方向
            transform.LookAt(transform.position + moveDirection);
        }

        // 簡單的重力處理，確保角色不會浮空
        if (!controller.isGrounded)
        {
            controller.Move(Vector3.down * 5f * Time.deltaTime);
        }
    }
}