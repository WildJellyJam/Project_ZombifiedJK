using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera mainCamera; // 單一攝影機
    private Transform playerTransform;

    public float cameraAngle = 40f; // 俯角40度

    [Header("固定視角設置")]
    public bool useFixedCamera = true; // 控制是否使用固定視角
    public bool applyLookAtAndAngle = true; // 控制是否應用看向點和俯角
    public Vector3 lookAtPosition = Vector3.zero; // 攝影機看向的固定點（默認為場景原點）

    void Start()
    {
        // 找到玩家角色（如果需要）
        playerTransform = FindObjectOfType<PlayerMovement>()?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("未找到玩家角色，請確保場景中有PlayerMovement組件！");
            return;
        }

        // 確保場景中有主攝影機
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("未找到主攝影機，請在場景中添加攝影機並設為MainCamera！");
                return;
            }
        }

        // 強制設置攝影機標籤為 MainCamera
        mainCamera.tag = "MainCamera";

        // 除錯：檢查攝影機初始位置（手動設定的位置）
        Debug.Log($"攝影機初始位置（手動設定）：{mainCamera.transform.position}");

        // 初始化攝影機（僅設置視角，不修改位置）
        SetupCamera();
    }

    void LateUpdate()
    {
        // 如果使用固定視角，不在 LateUpdate 中更新攝影機
        if (!useFixedCamera)
        {
            UpdateCameraPositionFollowPlayer();
        }
    }

    private void SetupCamera()
    {
        if (useFixedCamera)
        {
            Debug.Log($"保留手動設定的攝影機位置與旋轉：位置={mainCamera.transform.position}, 旋轉={mainCamera.transform.eulerAngles}");

            // 不做任何修改，保留原始位置與旋轉
            return;
        }
        else
        {
            UpdateCameraPositionFollowPlayer();
        }
    }

    private void UpdateCameraPositionFollowPlayer()
    {
        if (playerTransform == null || mainCamera == null) return;

        // 原有的跟隨邏輯（這裡僅作為備用）
        Vector3 cameraOffset = new Vector3(0f, 300f, -500f);
        Vector3 desiredPosition = playerTransform.position + cameraOffset;
        mainCamera.transform.position = desiredPosition;

        Debug.Log($"玩家位置：{playerTransform.position}, 攝影機位置：{mainCamera.transform.position}");

        Vector3 playerLookAtPosition = playerTransform.position + new Vector3(0f, 1f, 0f);
        mainCamera.transform.LookAt(playerLookAtPosition);

        Vector3 currentEuler = mainCamera.transform.eulerAngles;
        mainCamera.transform.rotation = Quaternion.Euler(cameraAngle, currentEuler.y, currentEuler.z);

        Debug.Log($"攝影機旋轉：{mainCamera.transform.eulerAngles}");
    }

    // 提供一個方法給其他腳本調用（例如 GameManager）
    public void InitializeCamera()
    {
        SetupCamera();
    }

    // 提供一個方法，允許動態調整攝影機位置和看向點
    public void SetCameraPositionAndLookAt(Vector3 newPosition, Vector3 newLookAtPosition)
    {
        mainCamera.transform.position = newPosition;
        lookAtPosition = newLookAtPosition;
        useFixedCamera = true;
        applyLookAtAndAngle = true;
        SetupCamera();
    }
}