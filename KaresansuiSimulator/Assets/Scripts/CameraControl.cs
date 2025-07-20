using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeedMultiplier = 2f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 3f;
    public float minY = -60f; // Look up/down limit
    public float maxY = 90f;  // Look up/down limit

    private float _rotationX = 0f;
    private float _rotationY = 0f;

    void Start()
    {
        // 初期回転を現在のカメラの回転に合わせる
        Vector3 currentEuler = transform.rotation.eulerAngles;
        _rotationY = currentEuler.y;
        _rotationX = currentEuler.x;

        // マウスカーソルをロックして非表示にする
        // マウス操作時にのみロックするよう GameManager で制御することを推奨
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    void Update()
    {
        // GameManager がアクティブでない場合は処理を停止
        if (!IsActiveMode()) return;

        HandleMovement();
        HandleRotation();

        // デバッグ用: Escキーでカーソルを解除
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// 現在このマネージャーがアクティブかどうかをチェック
    /// </summary>
    /// <returns>アクティブかどうか</returns>
    private bool IsActiveMode()
    {
        // GameManager が存在し、現在のモードが CameraControl である場合にアクティブ
        return GameManager.Instance != null && GameManager.Instance.IsCurrentMode(GameMode.CameraControl);
    }

    private void HandleMovement()
    {
        float currentMoveSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentMoveSpeed *= sprintSpeedMultiplier;
        }

        float horizontal = Input.GetAxis("Horizontal"); // A/D keys
        float vertical = Input.GetAxis("Vertical");     // W/S keys

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        transform.position += moveDirection * currentMoveSpeed * Time.deltaTime;

        // Spaceキーで上昇、LeftControlキーで下降
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += Vector3.up * currentMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.position += Vector3.down * currentMoveSpeed * Time.deltaTime;
        }
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        _rotationY += mouseX; // Y軸（左右）の回転
        _rotationX -= mouseY; // X軸（上下）の回転

        // X軸の回転を制限
        _rotationX = Mathf.Clamp(_rotationX, minY, maxY);

        // カメラの回転を適用
        transform.rotation = Quaternion.Euler(_rotationX, _rotationY, 0f);
    }

    /// <summary>
    /// カメラコントロールモードがアクティブになったときに呼び出される
    /// </summary>
    public void EnableCameraControl()
    {
        this.enabled = true; // スクリプトを有効化
        Cursor.lockState = CursorLockMode.Locked; // カーソルをロック
        Cursor.visible = false; // カーソルを非表示
        UnityEngine.Debug.Log("CameraControl: Enabled.");
    }

    /// <summary>
    /// カメラコントロールモードが非アクティブになったときに呼び出される
    /// </summary>
    public void DisableCameraControl()
    {
        this.enabled = false; // スクリプトを無効化
        Cursor.lockState = CursorLockMode.None; // カーソルを解除
        Cursor.visible = true; // カーソルを表示
        UnityEngine.Debug.Log("CameraControl: Disabled.");
    }
}