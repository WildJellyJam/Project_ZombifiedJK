using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    public float moveSpeed = 10f; // 移動速度

    private CameraManager camMgr;

    void Start()
    {
        // 獲取CharacterController組件
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController組件未找到，請在角色上添加CharacterController！");
        }
        camMgr = FindObjectOfType<CameraManager>();
        if (camMgr == null)
            Debug.LogError("找不到 CameraManager，請確認場景中已放置！");
    }

    void Update()
    {
        // 獲取WASD輸入
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D鍵：左/右
        float moveZ = Input.GetAxisRaw("Vertical");   // W/S鍵：前/後

        bool usingFixed = camMgr != null && camMgr.usingFixedCamera;

        // 計算移動方向（基於世界空間）
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        if (!usingFixed)
        {
            /* ---- A. 普通模式：以世界座標移動（維持你原本的寫法） ---- */
            moveDirection = new Vector3(moveX, 0, moveZ).normalized;
            // 如果有移動輸入，移動角色並調整朝向
            if (moveDirection.magnitude > 0)
            {
                controller.Move(moveDirection * moveSpeed * Time.deltaTime);

                // 讓角色朝向移動方向
                transform.LookAt(transform.position + moveDirection);
            }

        }
        else
        {
            /* ---- B. 固定鏡頭模式：以鏡頭前 / 右 為基準 ---- */
            Camera cam = camMgr.fixedCamera;
            if (cam == null) return;                     // 防呆

            Vector3 camFwd = cam.transform.forward;     // 鏡頭前
            Vector3 camRight = cam.transform.right;     // 鏡頭右
            camFwd.y = camRight.y = 0f;                 // 投影到水平面
            camFwd.Normalize();
            camRight.Normalize();

            moveDirection = (camFwd * moveZ + camRight * moveX).normalized;

            /* 讓角色朝向鏡頭視角（或改成朝向 moveDir 亦可） */
            transform.rotation = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f);
            if (moveDirection.magnitude > 0)
                controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

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