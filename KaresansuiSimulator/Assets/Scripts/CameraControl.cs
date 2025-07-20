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
        // ������]�����݂̃J�����̉�]�ɍ��킹��
        Vector3 currentEuler = transform.rotation.eulerAngles;
        _rotationY = currentEuler.y;
        _rotationX = currentEuler.x;

        // �}�E�X�J�[�\�������b�N���Ĕ�\���ɂ���
        // �}�E�X���쎞�ɂ̂݃��b�N����悤 GameManager �Ő��䂷�邱�Ƃ𐄏�
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    void Update()
    {
        // GameManager ���A�N�e�B�u�łȂ��ꍇ�͏������~
        if (!IsActiveMode()) return;

        HandleMovement();
        HandleRotation();

        // �f�o�b�O�p: Esc�L�[�ŃJ�[�\��������
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// ���݂��̃}�l�[�W���[���A�N�e�B�u���ǂ������`�F�b�N
    /// </summary>
    /// <returns>�A�N�e�B�u���ǂ���</returns>
    private bool IsActiveMode()
    {
        // GameManager �����݂��A���݂̃��[�h�� CameraControl �ł���ꍇ�ɃA�N�e�B�u
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

        // Space�L�[�ŏ㏸�ALeftControl�L�[�ŉ��~
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

        _rotationY += mouseX; // Y���i���E�j�̉�]
        _rotationX -= mouseY; // X���i�㉺�j�̉�]

        // X���̉�]�𐧌�
        _rotationX = Mathf.Clamp(_rotationX, minY, maxY);

        // �J�����̉�]��K�p
        transform.rotation = Quaternion.Euler(_rotationX, _rotationY, 0f);
    }

    /// <summary>
    /// �J�����R���g���[�����[�h���A�N�e�B�u�ɂȂ����Ƃ��ɌĂяo�����
    /// </summary>
    public void EnableCameraControl()
    {
        this.enabled = true; // �X�N���v�g��L����
        Cursor.lockState = CursorLockMode.Locked; // �J�[�\�������b�N
        Cursor.visible = false; // �J�[�\�����\��
        UnityEngine.Debug.Log("CameraControl: Enabled.");
    }

    /// <summary>
    /// �J�����R���g���[�����[�h����A�N�e�B�u�ɂȂ����Ƃ��ɌĂяo�����
    /// </summary>
    public void DisableCameraControl()
    {
        this.enabled = false; // �X�N���v�g�𖳌���
        Cursor.lockState = CursorLockMode.None; // �J�[�\��������
        Cursor.visible = true; // �J�[�\����\��
        UnityEngine.Debug.Log("CameraControl: Disabled.");
    }
}