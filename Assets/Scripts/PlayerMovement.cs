using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    public float moveSpeed = 5f; // 移動速度

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
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        if (moveDirection.magnitude > 0)
        {
            CameraManager cameraManager = FindObjectOfType<CameraManager>();
            moveDirection = cameraManager.GetAdjustedMoveDirection(moveDirection);
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);

            // 根據攝影機方向設置角色朝向
            switch (cameraManager.currentDirection)
            {
                case CameraManager.ViewDirection.Front:
                    transform.rotation = Quaternion.LookRotation(Vector3.forward);
                    break;
                case CameraManager.ViewDirection.Back:
                    transform.rotation = Quaternion.LookRotation(Vector3.back);
                    break;
                case CameraManager.ViewDirection.Left:
                    transform.rotation = Quaternion.LookRotation(Vector3.left);
                    break;
                case CameraManager.ViewDirection.Right:
                    transform.rotation = Quaternion.LookRotation(Vector3.right);
                    break;
            }
        }

        if (!controller.isGrounded)
        {
            controller.Move(Vector3.down * 5f * Time.deltaTime);
        }
    }
}