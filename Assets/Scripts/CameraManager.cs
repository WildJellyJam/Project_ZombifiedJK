using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera cameraFront;
    public Camera cameraBack;
    public Camera cameraLeft;
    public Camera cameraRight;

    public enum ViewDirection { Front, Back, Left, Right }
    public ViewDirection currentDirection = ViewDirection.Front;

    private Transform playerTransform;

    void Start()
    {
        playerTransform = FindObjectOfType<PlayerMovement>().transform;
        if (playerTransform == null)
        {
            Debug.LogError("未找到玩家角色，請確保場景中有PlayerMovement組件！");
        }

        InitializeCameras();
        SwitchCamera(ViewDirection.Front);
    }

    void Update()
    {
        UpdateCameraPositions();
    }

    private void InitializeCameras()
    {
        if (cameraFront == null || cameraBack == null || cameraLeft == null || cameraRight == null)
        {
            Debug.LogError("請在Inspector中為所有攝影機賦值！");
            return;
        }

        float distance = 5f;
        float height = 2f;

        // 前方攝影機：應朝向 +Z 方向（從角色的前方看）
        cameraFront.transform.position = playerTransform.position + new Vector3(0, height, distance);
        cameraFront.transform.rotation = Quaternion.LookRotation(-Vector3.forward, Vector3.up);
        Debug.Log($"前方攝影機朝向：{cameraFront.transform.forward}");

        // 後方攝影機：應朝向 -Z 方向（從角色的後方看）
        cameraBack.transform.position = playerTransform.position + new Vector3(0, height, -distance);
        cameraBack.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        Debug.Log($"後方攝影機朝向：{cameraBack.transform.forward}");

        // 左方攝影機：應朝向 -X 方向（從角色的左方看）
        cameraLeft.transform.position = playerTransform.position + new Vector3(-distance, height, 0);
        cameraLeft.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
        Debug.Log($"左方攝影機朝向：{cameraLeft.transform.forward}");

        // 右方攝影機：應朝向 +X 方向（從角色的右方看）
        cameraRight.transform.position = playerTransform.position + new Vector3(distance, height, 0);
        cameraRight.transform.rotation = Quaternion.LookRotation(-Vector3.right, Vector3.up);
        Debug.Log($"右方攝影機朝向：{cameraRight.transform.forward}");

        cameraFront.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
        cameraBack.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
        cameraLeft.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
        cameraRight.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
    }

    private void UpdateCameraPositions()
    {
        float distance = 5f;
        float height = 2f;

        cameraFront.transform.position = playerTransform.position + new Vector3(0, height, distance);
        cameraFront.transform.rotation = Quaternion.LookRotation(playerTransform.position - cameraFront.transform.position, Vector3.up);

        cameraBack.transform.position = playerTransform.position + new Vector3(0, height, -distance);
        cameraBack.transform.rotation = Quaternion.LookRotation(playerTransform.position - cameraBack.transform.position, Vector3.up);

        cameraLeft.transform.position = playerTransform.position + new Vector3(-distance, height, 0);
        cameraLeft.transform.rotation = Quaternion.LookRotation(playerTransform.position - cameraLeft.transform.position, Vector3.up);

        cameraRight.transform.position = playerTransform.position + new Vector3(distance, height, 0);
        cameraRight.transform.rotation = Quaternion.LookRotation(playerTransform.position - cameraRight.transform.position, Vector3.up);
    }

    public void SwitchCamera(ViewDirection direction)
    {
        currentDirection = direction;

        cameraFront.enabled = false;
        cameraBack.enabled = false;
        cameraLeft.enabled = false;
        cameraRight.enabled = false;

        switch (direction)
        {
            case ViewDirection.Front:
                cameraFront.enabled = true;
                break;
            case ViewDirection.Back:
                cameraBack.enabled = true;
                break;
            case ViewDirection.Left:
                cameraLeft.enabled = true;
                break;
            case ViewDirection.Right:
                cameraRight.enabled = true;
                break;
        }
    }

    public void SwitchToLeft()
    {
        switch (currentDirection)
        {
            case ViewDirection.Front:
                SwitchCamera(ViewDirection.Left);
                break;
            case ViewDirection.Left:
                SwitchCamera(ViewDirection.Back);
                break;
            case ViewDirection.Back:
                SwitchCamera(ViewDirection.Right);
                break;
            case ViewDirection.Right:
                SwitchCamera(ViewDirection.Front);
                break;
        }
    }

    public void SwitchToRight()
    {
        switch (currentDirection)
        {
            case ViewDirection.Front:
                SwitchCamera(ViewDirection.Right);
                break;
            case ViewDirection.Right:
                SwitchCamera(ViewDirection.Back);
                break;
            case ViewDirection.Back:
                SwitchCamera(ViewDirection.Left);
                break;
            case ViewDirection.Left:
                SwitchCamera(ViewDirection.Front);
                break;
        }
    }

    public Vector3 GetAdjustedMoveDirection(Vector3 inputDirection)
    {
        Vector3 adjusted = inputDirection;
        switch (currentDirection)
        {
            case ViewDirection.Front:
                //adjusted = inputDirection;
                adjusted = new Vector3(-inputDirection.x, 0, -inputDirection.z);
                break;
            case ViewDirection.Back:
                //adjusted = new Vector3(-inputDirection.x, 0, -inputDirection.z);
                adjusted = inputDirection;
                break;
            case ViewDirection.Left:
                adjusted = new Vector3(inputDirection.z, 0, -inputDirection.x);
                //adjusted = new Vector3(-inputDirection.z, 0, inputDirection.x);
                break;
            case ViewDirection.Right:
                adjusted = new Vector3(-inputDirection.z, 0, inputDirection.x);
                //adjusted = new Vector3(inputDirection.z, 0, -inputDirection.x);
                break;
        }
        Debug.Log($"視角：{currentDirection}, 原始方向：{inputDirection}, 調整後方向：{adjusted}");
        return adjusted;
    }
}