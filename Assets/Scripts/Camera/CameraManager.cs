using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera mainCamera; // 單一攝影機
    public Camera fixedCamera; 
    private Transform playerTransform;

    public float cameraAngle = 40f; // 俯角40度

    [Header("固定視角設置")]
    public bool usingFixedCamera = false; // 控制是否使用固定視角
    public bool applyLookAtAndAngle = true; // 控制是否應用看向點和俯角
    public Vector3 lookAtPosition = Vector3.zero; // 攝影機看向的固定點（默認為場景原點）

    [Header("滑鼠控制參數 (FixedCam)")]
    public float mouseSensitivity = 150f;     // 靈敏度，依需求微調
    public float pitchClampMin = -80f;        // 俯仰角下限
    public float pitchClampMax =  80f;        // 俯仰角上限

    private float yaw;                         // 左右旋轉量（水平）
    private float pitch;                       // 上下旋轉量（垂直）


    void OnEnable()
    {
        newGameManager.Instance.LoadGameEvent += InitializeCamera;
    }
    void OnDisable()
    {
        newGameManager.Instance.LoadGameEvent -= InitializeCamera;
    }

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
        SetActiveCamera(mainCamera);

        // 除錯：檢查攝影機初始位置（手動設定的位置）
        Debug.Log($"攝影機初始位置（手動設定）：{mainCamera.transform.position}");
    }

    void Update()          // 用 Update 監聽鍵盤即可；不必等到 LateUpdate
    {
        if (usingFixedCamera)
            HandleFixedCameraMouseLook();
    }


    // void LateUpdate()
    // {
    //     // 如果使用固定視角，不在 LateUpdate 中更新攝影機
    //     if (!usingFixedCamera)
    //     {
    //         UpdateCameraPositionFollowPlayer();
    //     }
    // }


    private void SetActiveCamera(Camera camToActivate)
    {
        // 1. 啟用要用的攝影機
        mainCamera.enabled  = (camToActivate == mainCamera);
        fixedCamera.enabled = (camToActivate == fixedCamera);

        // 2. 同步 AudioListener —— Unity 規定場景一次只能有一個啟用
        var mainListener  = mainCamera.GetComponent<AudioListener>();
        var fixedListener = fixedCamera.GetComponent<AudioListener>();

        if (mainListener)  mainListener.enabled  = (camToActivate == mainCamera);
        if (fixedListener) fixedListener.enabled = (camToActivate == fixedCamera);

        // 3. 若切回跟隨模式，讓攝影機立刻重新定位
        if (camToActivate == mainCamera)
        {
            usingFixedCamera = false;
        }
    }


    public void ToggleCamera()
    {
        usingFixedCamera = !usingFixedCamera;            // 反轉狀態旗標
        SetActiveCamera(usingFixedCamera ? fixedCamera : mainCamera);

        if (usingFixedCamera)
        {
            /* === 固定鏡頭模式啟用 === */
            // 取目前鏡頭角度當基準
            Vector3 euler = fixedCamera.transform.eulerAngles;
            yaw = euler.y;
            pitch = euler.x;

            // 鎖定游標／隱藏
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            /* === 回到主鏡頭 === */
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 若你的跟隨鏡頭需要俯角、LookAt 等功能，
            // 透過儲存的 usingFixedCamera 旗標原有程式碼就能判斷要不要執行
    }

    private void HandleFixedCameraMouseLook()
    {
        if (fixedCamera == null) return;   // 防呆

        // 取得滑鼠位移量（Raw 可避免加速曲線）
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;      // 左右旋轉
        pitch -= mouseY;      // 上下旋轉（注意方向相反）
        pitch = Mathf.Clamp(pitch, pitchClampMin, pitchClampMax);

        fixedCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }


    // private void SetupCamera()
    // {
    //     if (usingFixedCamera)
    //     {
    //         Debug.Log($"保留手動設定的攝影機位置與旋轉：位置={mainCamera.transform.position}, 旋轉={mainCamera.transform.eulerAngles}");

    //         // 不做任何修改，保留原始位置與旋轉
    //         return;
    //     }
    //     else
    //     {
    //         UpdateCameraPositionFollowPlayer();
    //     }
    // }

    // private void UpdateCameraPositionFollowPlayer()
    // {
    //     // if (playerTransform == null || mainCamera == null) return;

    //     // // 原有的跟隨邏輯（這裡僅作為備用）
    //     // Vector3 cameraOffset = new Vector3(0f, 300f, -500f);
    //     // Vector3 desiredPosition = playerTransform.position + cameraOffset;
    //     // mainCamera.transform.position = desiredPosition;

    //     // Debug.Log($"玩家位置：{playerTransform.position}, 攝影機位置：{mainCamera.transform.position}");

    //     // Vector3 playerLookAtPosition = playerTransform.position + new Vector3(0f, 1f, 0f);
    //     // mainCamera.transform.LookAt(playerLookAtPosition);

    //     // Vector3 currentEuler = mainCamera.transform.eulerAngles;
    //     // mainCamera.transform.rotation = Quaternion.Euler(cameraAngle, currentEuler.y, currentEuler.z);

    //     // Debug.Log($"攝影機旋轉：{mainCamera.transform.eulerAngles}");
    // }

    // 提供一個方法給其他腳本調用（例如 GameManager）
    public void InitializeCamera()
    {
        // SetupCamera();
    }

    // 提供一個方法，允許動態調整攝影機位置和看向點
    public void SetCameraPositionAndLookAt(Vector3 newPosition, Vector3 newLookAtPosition)
    {
        mainCamera.transform.position = newPosition;
        lookAtPosition = newLookAtPosition;
        usingFixedCamera = true;
        applyLookAtAndAngle = true;
        // SetupCamera();
    }
}