using UnityEngine;

/// <summary>
/// 玩家跟随摄像机脚本 - 方案一（18x10瓦片显示）
/// 实现4瓦片水平死区和2瓦片垂直死区的平滑跟随效果
/// </summary>
public class PlayerFollowCamera : MonoBehaviour
{
    [Header("目标设置")]
    [Tooltip("跟随的目标对象（通常是玩家）")]
    public Transform target;
    
    [Tooltip("跟随的速度（数值越大响应越快）")]
    [Range(0.1f, 20f)]
    public float followSpeed = 8f;
    
    [Header("死区设置")]
    [Tooltip("水平死区大小")]
    [Range(0, 10)]
    public float horizontalDeadZoneTiles = 4f;
    
    [Tooltip("垂直死区大小")]
    [Range(0, 6)]
    public float verticalDeadZoneTiles = 2f;
    
    [Tooltip("是否启用死区功能")]
    public bool enableDeadZone = true;
    
    [Header("边界限制")]
    [Tooltip("世界边界最小值")]
    public Vector2 worldBoundsMin = new Vector2(-50f, -50f);
    
    [Tooltip("世界边界最大值")]
    public Vector2 worldBoundsMax = new Vector2(50f, 50f);
    
    [Header("缩放控制")]
    [Tooltip("是否允许鼠标滚轮缩放")]
    public bool allowZoom = false;
    
    [Tooltip("缩放速度")]
    [Range(0.1f, 5f)]
    public float zoomSpeed = 2f;
    
    [Tooltip("最小缩放值")]
    public float minZoom = 3f;
    
    [Tooltip("最大缩放值")]
    public float maxZoom = 10f;
    
    [Header("前瞻跟随")]
    [Tooltip("是否启用前瞻跟随")]
    public bool enableLookAhead = true;
    
    [Tooltip("前瞻比例（相对于移动速度）")]
    [Range(0f, 5f)]
    public float lookAheadRatio = 1.5f;
    
    [Tooltip("最大前瞻偏移（单位）")]
    public float maxLookAheadOffset = 3f;
    
    [Tooltip("前瞻跟随的响应速度")]
    [Range(0.1f, 10f)]
    public float lookAheadResponseSpeed = 5f;
    
    [Tooltip("垂直方向的前瞻增强倍数")]
    [Range(0.5f, 3f)]
    public float verticalLookAheadMultiplier = 1.5f;
    
    // 内部状态
    private Camera cam;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    private Vector3 deadZoneCenter;
    private Vector3 deadZoneSize;
    private bool hasTargetPosition = false;
    
    // 前瞻跟随状态
    private Vector3 previousPlayerPosition;
    private Vector3 lookAheadTarget;
    private Vector3 currentLookAhead;
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("PlayerFollowCamera 需要在Camera对象上使用");
            enabled = false;
            return;
        }
        
        // 保存原始摄像机z轴位置
        float originalZ = transform.position.z;
        
        // 设置正交相机
        cam.orthographic = true;
        
        // 初始化目标位置（保持原始z轴位置）
        if (target != null)
        {
            targetPosition = new Vector3(target.position.x, target.position.y, originalZ);
            deadZoneCenter = targetPosition;
            hasTargetPosition = true;
            transform.position = targetPosition;
            
            // 初始化前瞻跟随状态
            previousPlayerPosition = target.position;
            currentLookAhead = Vector3.zero;
            lookAheadTarget = targetPosition;
        }
        
        CalculateDeadZone();
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // 处理缩放输入
        if (allowZoom)
        {
            HandleZoomInput();
        }
        
        // 更新死区大小（如果正交大小改变）
        if (cam.orthographicSize != deadZoneSize.y / 2f)
        {
            CalculateDeadZone();
        }
        
        // 更新摄像机位置
        UpdateCameraPosition();
    }
    
    /// <summary>
    /// 处理缩放输入
    /// </summary>
    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            
            // 重新计算死区大小
            CalculateDeadZone();
        }
    }
    
    /// <summary>
    /// 计算死区大小
    /// </summary>
    private void CalculateDeadZone()
    {
        float aspect = (float)Screen.width / Screen.height;
        float cameraHeight = 2f * cam.orthographicSize;
        float cameraWidth = cameraHeight * aspect;
        
        // 计算死区大小（基于18x10瓦片基准）
        float tileSizeX = cameraWidth / 18f;
        float tileSizeY = cameraHeight / 10f;
        
        deadZoneSize = new Vector3(
            horizontalDeadZoneTiles * tileSizeX,
            verticalDeadZoneTiles * tileSizeY,
            0
        );
    }
    
    /// <summary>
    /// 更新摄像机位置
    /// </summary>
    private void UpdateCameraPosition()
    {
        Vector3 playerPos = target.position;
        
        // 计算玩家移动方向和速度
        Vector3 playerVelocity = (playerPos - previousPlayerPosition) / Time.deltaTime;
        previousPlayerPosition = playerPos;
        
        // 初始化目标位置
        Vector3 baseTargetPosition;
        
        if (enableDeadZone)
        {
            // 检查玩家是否超出死区
            if (IsPlayerOutsideDeadZone(playerPos))
            {
                // 更新死区中心到玩家位置
                deadZoneCenter = new Vector3(
                    Mathf.Clamp(playerPos.x, worldBoundsMin.x + deadZoneSize.x / 2, worldBoundsMax.x - deadZoneSize.x / 2),
                    Mathf.Clamp(playerPos.y, worldBoundsMin.y + deadZoneSize.y / 2, worldBoundsMax.y - deadZoneSize.y / 2),
                    transform.position.z
                );
            }
            
            baseTargetPosition = deadZoneCenter;
        }
        else
        {
            baseTargetPosition = new Vector3(
                Mathf.Clamp(playerPos.x, worldBoundsMin.x, worldBoundsMax.x),
                Mathf.Clamp(playerPos.y, worldBoundsMin.y, worldBoundsMax.y),
                transform.position.z
            );
        }
        
        // 应用前瞻跟随（如果启用）
        if (enableLookAhead && playerVelocity.magnitude > 0.1f)
        {
            // 计算前瞻目标位置
            Vector3 normalizedVelocity = playerVelocity.normalized;
            Vector3 lookAheadOffset = normalizedVelocity * playerVelocity.magnitude * lookAheadRatio * 0.1f; // 缩放因子
            
            // 垂直方向增强
            lookAheadOffset.y *= verticalLookAheadMultiplier;
            
            // 限制最大前瞻偏移
            lookAheadOffset.x = Mathf.Clamp(lookAheadOffset.x, -maxLookAheadOffset, maxLookAheadOffset);
            lookAheadOffset.y = Mathf.Clamp(lookAheadOffset.y, -maxLookAheadOffset, maxLookAheadOffset);
            
            // 设置前瞻目标
            lookAheadTarget = baseTargetPosition + lookAheadOffset;
            
            // 平滑跟随前瞻偏移
            currentLookAhead = Vector3.Lerp(currentLookAhead, lookAheadOffset, Time.deltaTime * lookAheadResponseSpeed);
            
            // 应用前瞻偏移到目标位置
            targetPosition = baseTargetPosition + currentLookAhead;
        }
        else
        {
            // 没有移动时逐渐减少前瞻偏移
            currentLookAhead = Vector3.Lerp(currentLookAhead, Vector3.zero, Time.deltaTime * lookAheadResponseSpeed * 0.5f);
            targetPosition = baseTargetPosition + currentLookAhead;
        }
        
        // 使用SmoothDamp进行平滑移动
        Vector3 newPosition = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref currentVelocity, 
            1f / followSpeed
        );
        
        // 应用世界边界限制
        newPosition.x = Mathf.Clamp(newPosition.x, worldBoundsMin.x, worldBoundsMax.x);
        newPosition.y = Mathf.Clamp(newPosition.y, worldBoundsMin.y, worldBoundsMax.y);
        
        transform.position = newPosition;
    }
    
    /// <summary>
    /// 检查玩家是否在死区外
    /// </summary>
    private bool IsPlayerOutsideDeadZone(Vector3 playerPosition)
    {
        Vector3 deadZoneMin = deadZoneCenter - deadZoneSize * 0.5f;
        Vector3 deadZoneMax = deadZoneCenter + deadZoneSize * 0.5f;
        
        return playerPosition.x < deadZoneMin.x || playerPosition.x > deadZoneMax.x ||
               playerPosition.y < deadZoneMin.y || playerPosition.y > deadZoneMax.y;
    }
    
    /// <summary>
    /// 设置跟随目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            float originalZ = transform.position.z;
            targetPosition = new Vector3(target.position.x, target.position.y, originalZ);
            deadZoneCenter = targetPosition;
            hasTargetPosition = true;
            
            // 重新初始化前瞻跟随状态
            previousPlayerPosition = target.position;
            currentLookAhead = Vector3.zero;
            lookAheadTarget = targetPosition;
        }
    }
    
    /// <summary>
    /// 设置世界边界
    /// </summary>
    public void SetWorldBounds(Vector2 min, Vector2 max)
    {
        worldBoundsMin = min;
        worldBoundsMax = max;
    }
    
    /// <summary>
    /// 可视化编辑器中的死区
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!Application.isEditor || target == null) return;
        
        Camera editorCam = GetComponent<Camera>();
        if (editorCam == null) return;
        
        // 计算当前死区大小
        float aspect = (float)Screen.width / Screen.height;
        float cameraHeight = 2f * editorCam.orthographicSize;
        float cameraWidth = cameraHeight * aspect;
        
        float tileSizeX = cameraWidth / 18f;
        float tileSizeY = cameraHeight / 10f;
        
        Vector3 currentDeadZoneSize = new Vector3(
            horizontalDeadZoneTiles * tileSizeX,
            verticalDeadZoneTiles * tileSizeY,
            0
        );
        
        // 绘制死区
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Vector3 center = hasTargetPosition ? deadZoneCenter : target.position;
        Gizmos.DrawWireCube(center, currentDeadZoneSize);
        
        // 绘制摄像机视野
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireCube(transform.position, new Vector3(cameraWidth, cameraHeight, 0.1f));
        
        // 绘制世界边界
        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Vector3 boundsCenter = (Vector3)(worldBoundsMin + worldBoundsMax) / 2f;
        Vector3 boundsSize = (Vector3)(worldBoundsMax - worldBoundsMin);
        Gizmos.DrawWireCube(boundsCenter, new Vector3(boundsSize.x, boundsSize.y, 0.1f));
    }
}