// TemplanePlacement.cs - UI��]�@�\�ǉ���
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class TemplanePlacement : MonoBehaviour
{
    [System.Serializable]
    public class SamanTemplate
    {
        public string name;
        public GameObject prefab;
        [Header("Prefab Size Settings")]
        public float prefabWorldSize = 10f;
        [Header("UI Display")]
        public Sprite previewImage; // �v���r���[�p�摜
        [TextArea(2, 4)]
        public string description = ""; // �e���v���[�g�̐���
    }

    [Header("Placement Settings")]
    public LayerMask groundLayer;
    public MeshRenderer groundPlaneRenderer;
    public float cellSize = 1f;

    [Header("Saman Templates")]
    public List<SamanTemplate> samanTemplates;
    public int currentTemplateIndex = 0;

    [Header("Rotation & Scale Settings")]
    public GameObject currentlySelectedTemplaneInScene;
    public float[] allowedScales = { 1f, 2f, 3f };
    private int currentScaleIndex = 0;
    private float currentRotationY = 0f; // ���݂̉�]�p�x��ǐ�

    [Header("UI Elements")]
    [SerializeField] private Button rotateLeftButton;
    [SerializeField] private Button rotateRightButton;
    [SerializeField] private Button scaleButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Image currentTemplatePreview;
    [SerializeField] private TextMeshProUGUI currentTemplateNameText;         // Text����TextMeshProUGUI�ɕύX
    [SerializeField] private TextMeshProUGUI currentTemplateDescriptionText;  // Text����TextMeshProUGUI�ɕύX
    [SerializeField] private TextMeshProUGUI rotationAngleText;             // Text����TextMeshProUGUI�ɕύX
    [SerializeField] private TextMeshProUGUI scaleText;                     // Text����TextMeshProUGUI�ɕύX

    [Header("Template Selection UI")]
    public List<Button> templateSelectionButtons; // �e�e���v���[�g�I���{�^���̃��X�g
    public List<Text> templateNameTexts;          // �e�e���v���[�g�I���{�^���̎q�̃e�L�X�g

    private Dictionary<Vector2Int, GameObject> placedTemplanes = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        InitializeUI();
    }

    void OnEnable()
    {
        UnityEngine.Debug.Log("TemplanePlacement: OnEnable called.");
        if (samanTemplates.Count > 0)
        {
            SetCurrentTemplate(0);
        }
        UpdateUI();
    }

    void OnDisable()
    {
        UnityEngine.Debug.Log("TemplanePlacement: OnDisable called.");
    }

    void Update()
    {
        // GameManager���A�N�e�B�u�łȂ��ꍇ�͏������~
        if (!IsActiveMode()) return;

        // �L�[�{�[�h���݂͂̂͂����ŏ����i�}�E�X���͂�GameManager�������j
        HandleKeyboardInput();
    }

    /// <summary>
    /// UI�v�f�̏������ƃC�x���g���X�i�[�̐ݒ�
    /// </summary>
    private void InitializeUI()
    {
        // ��]�{�^���̃C�x���g�ݒ�
        if (rotateLeftButton != null)
            rotateLeftButton.onClick.AddListener(() => RotateTemplate(-90f));

        if (rotateRightButton != null)
            rotateRightButton.onClick.AddListener(() => RotateTemplate(90f));

        // �X�P�[���{�^���̃C�x���g�ݒ�
        if (scaleButton != null)
            scaleButton.onClick.AddListener(ScaleSelectedTemplane);

        // �폜�{�^���̃C�x���g�ݒ�
        if (deleteButton != null)
            deleteButton.onClick.AddListener(DeleteSelectedTemplane);

        // �e���v���[�g�I���{�^���̃C�x���g�ݒ�ƃe�L�X�g������
        for (int i = 0; i < templateSelectionButtons.Count; i++)
        {
            // �N���[�W���̖�������邽�߂Ƀ��[�J���ϐ��ɃR�s�[
            int index = i;

            if (templateSelectionButtons[index] != null)
            {
                // �{�^�����N���b�N���ꂽ�� SetCurrentTemplate ���\�b�h���Ăяo��
                templateSelectionButtons[index].onClick.AddListener(() => SetCurrentTemplate(index));

                // �Ή�����e���v���[�g�����݂��A�e�L�X�g�v�f�����蓖�Ă��Ă���ꍇ�A�{�^���̃e�L�X�g��ݒ�
                if (index < samanTemplates.Count && templateNameTexts.Count > index && templateNameTexts[index] != null)
                {
                    templateNameTexts[index].text = samanTemplates[index].name;
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"TemplanePlacement: Template button or text element not properly assigned or template missing for index {index}.");
                }
            }
        }

        UpdateUI();
    }

    /// <summary>
    /// UI�\�����X�V
    /// </summary>
    private void UpdateUI()
    {
        if (samanTemplates.Count == 0 || currentTemplateIndex < 0 || currentTemplateIndex >= samanTemplates.Count)
            return;

        var currentTemplate = samanTemplates[currentTemplateIndex];

        // �v���r���[�摜�̍X�V
        if (currentTemplatePreview != null)
        {
            if (currentTemplate.previewImage != null)
            {
                currentTemplatePreview.sprite = currentTemplate.previewImage;
                currentTemplatePreview.gameObject.SetActive(true);
            }
            else
            {
                currentTemplatePreview.gameObject.SetActive(false);
            }
        }

        // �e���v���[�g���̍X�V
        if (currentTemplateNameText != null)
            currentTemplateNameText.text = currentTemplate.name;

        // �e���v���[�g�����̍X�V
        if (currentTemplateDescriptionText != null)
            currentTemplateDescriptionText.text = currentTemplate.description;

        // ��]�p�x�̕\���X�V
        if (rotationAngleText != null)
            rotationAngleText.text = $"Rotation: {currentRotationY:F0}��";

        // �X�P�[���̕\���X�V
        if (scaleText != null)
            scaleText.text = $"Scale: {allowedScales[currentScaleIndex]:F1}x";

        // �{�^���̗L��/������Ԃ��X�V
        bool hasSelectedTemplate = currentlySelectedTemplaneInScene != null;
        if (rotateLeftButton != null) rotateLeftButton.interactable = hasSelectedTemplate;
        if (rotateRightButton != null) rotateRightButton.interactable = hasSelectedTemplate;
        if (scaleButton != null) scaleButton.interactable = hasSelectedTemplate;
        if (deleteButton != null) deleteButton.interactable = hasSelectedTemplate;
    }

    /// <summary>
    /// ���݂��̃}�l�[�W���[���A�N�e�B�u���ǂ������`�F�b�N
    /// </summary>
    /// <returns>�A�N�e�B�u���ǂ���</returns>
    private bool IsActiveMode()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.IsCurrentMode(GameMode.SamanTemplatePlacement);
    }

    /// <summary>
    /// �L�[�{�[�h���͂������iGameManager����Ăяo�����j
    /// </summary>
    public void HandleKeyboardInput()
    {
        // �e���v���[�g�I��
        for (int i = 0; i < samanTemplates.Count && i < 9; i++) // �ő�9�܂�
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetCurrentTemplate(i);
            }
        }

        // ��]�E�X�P�[���E�폜�i�����̋@�\���ێ��j
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateTemplate(90f); // �����̋@�\�Ɠ���
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ScaleSelectedTemplane();
        }
        if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
        {
            DeleteSelectedTemplane();
        }
    }

    /// <summary>
    /// �}�E�X�N���b�N�����iGameManager����Ăяo�����j
    /// </summary>
    public void HandleMouseClick()
    {
        if (!IsActiveMode()) return;
        PlaceTemplane();
    }

    /// <summary>
    /// �e���v���[�g���w��p�x��]�iUI�p�j
    /// </summary>
    /// <param name="angle">��]�p�x�i�x�j</param>
    public void RotateTemplate(float angle)
    {
        currentRotationY += angle;
        currentRotationY = currentRotationY % 360f; // 0-360�x�͈̔͂ɐ��K��

        if (currentlySelectedTemplaneInScene != null)
        {
            currentlySelectedTemplaneInScene.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);
            UnityEngine.Debug.Log($"TemplanePlacement: Rotated template to {currentRotationY}��");
        }

        UpdateUI();
    }

    /// <summary>
    /// �����̉�]�@�\�i90�x��]�j- ���ʌ݊����̂��߂Ɉێ�
    /// </summary>
    public void RotateSelectedTemplane()
    {
        RotateTemplate(90f);
    }

    /// <summary>
    /// �e���v���[�g��z�u
    /// </summary>
    private void PlaceTemplane()
    {
        if (samanTemplates.Count == 0 || samanTemplates[currentTemplateIndex].prefab == null)
        {
            UnityEngine.Debug.LogWarning("TemplanePlacement: No template selected or prefab is null.");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            if (hit.collider.gameObject != groundPlaneRenderer.gameObject)
            {
                UnityEngine.Debug.LogWarning($"TemplanePlacement: Ray hit {hit.collider.name} but expected {groundPlaneRenderer.gameObject.name}. Not placing.");
                return;
            }

            Vector3 worldPos = hit.point;
            Vector2Int gridCoords = WorldToGrid(worldPos);

            // �����̃e���v���[�g������ꍇ�͍폜
            if (placedTemplanes.ContainsKey(gridCoords) && placedTemplanes[gridCoords] != null)
            {
                Destroy(placedTemplanes[gridCoords]);
                placedTemplanes.Remove(gridCoords);
                UnityEngine.Debug.Log($"TemplanePlacement: Replaced existing templane at {gridCoords}.");
            }

            // �V�����e���v���[�g��z�u
            GameObject newTemplane = Instantiate(samanTemplates[currentTemplateIndex].prefab);
            float desiredWorldScale = Mathf.Min(allowedScales[currentScaleIndex], cellSize);
            newTemplane.transform.position = GridToWorld(gridCoords, desiredWorldScale);
            newTemplane.transform.SetParent(groundPlaneRenderer.transform);

            currentlySelectedTemplaneInScene = newTemplane;
            ApplyCurrentRotationAndScale(newTemplane);

            placedTemplanes.Add(gridCoords, newTemplane);
            UnityEngine.Debug.Log($"TemplanePlacement: Placed '{samanTemplates[currentTemplateIndex].name}' at grid {gridCoords}.");

            UpdateUI(); // UI��Ԃ��X�V
        }
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 groundBottomLeft = groundPlaneRenderer.transform.position - groundPlaneRenderer.bounds.extents;
        Vector3 relativePos = worldPos - groundBottomLeft;
        int gridX = Mathf.FloorToInt(relativePos.x / cellSize);
        int gridZ = Mathf.FloorToInt(relativePos.z / cellSize);
        return new Vector2Int(gridX, gridZ);
    }

    /// <summary>
    /// �O���b�h���W�����[���h���W�i�O���b�h�Z�����Ɏ��܂�ʒu�j�ɕϊ����܂��B
    /// </summary>
    /// <param name="templateActualWorldScale">�z�u����e���v���[�g�̎��ۂ̃��[���h�X�P�[��</param>
    Vector3 GridToWorld(Vector2Int gridCoords, float templateActualWorldScale)
    {
        Vector3 groundBottomLeft = groundPlaneRenderer.transform.position - groundPlaneRenderer.bounds.extents;

        float cellMinX = groundBottomLeft.x + (gridCoords.x * cellSize);
        float cellMinZ = groundBottomLeft.z + (gridCoords.y * cellSize);

        if (templateActualWorldScale <= cellSize)
        {
            float centerX = cellMinX + (cellSize / 2f);
            float centerZ = cellMinZ + (cellSize / 2f);
            return new Vector3(centerX, groundPlaneRenderer.transform.position.y + 0.001f, centerZ);
        }
        else
        {
            float centerX = cellMinX + (templateActualWorldScale / 2f);
            float centerZ = cellMinZ + (templateActualWorldScale / 2f);
            return new Vector3(centerX, groundPlaneRenderer.transform.position.y + 0.001f, centerZ);
        }
    }

    public void SetCurrentTemplate(int index)
    {
        if (index >= 0 && index < samanTemplates.Count)
        {
            currentTemplateIndex = index;
            // �V�����e���v���[�g��I���������]�����Z�b�g
            currentRotationY = 0f;
            currentScaleIndex = 0;

            UnityEngine.Debug.Log($"TemplanePlacement: Selected template: {samanTemplates[index].name}");
            UpdateUI();
        }
        else
        {
            UnityEngine.Debug.LogWarning($"TemplanePlacement: Invalid template index {index}.");
        }
    }

    public void ScaleSelectedTemplane()
    {
        if (currentlySelectedTemplaneInScene != null)
        {
            currentScaleIndex = (currentScaleIndex + 1) % allowedScales.Length;
            ApplyCurrentRotationAndScale(currentlySelectedTemplaneInScene);

            UnityEngine.Debug.Log($"TemplanePlacement: Scaled to {allowedScales[currentScaleIndex]}x");
            UpdateUI();
        }
        else
        {
            UnityEngine.Debug.LogWarning("TemplanePlacement: No templane selected in scene for scaling.");
        }
    }

    public void DeleteSelectedTemplane()
    {
        if (currentlySelectedTemplaneInScene != null)
        {
            Vector2Int gridCoords = WorldToGrid(currentlySelectedTemplaneInScene.transform.position);
            if (placedTemplanes.ContainsKey(gridCoords))
            {
                placedTemplanes.Remove(gridCoords);
            }
            Destroy(currentlySelectedTemplaneInScene);
            currentlySelectedTemplaneInScene = null;
            UnityEngine.Debug.Log("TemplanePlacement: Deleted selected templane.");

            UpdateUI(); // UI��Ԃ��X�V
        }
        else
        {
            UnityEngine.Debug.LogWarning("TemplanePlacement: No templane selected in scene for deletion.");
        }
    }

    /// <summary>
    /// �v���n�u�̌��̃T�C�Y���l�������X�P�[���K�p
    /// </summary>
    private void ApplyCurrentRotationAndScale(GameObject targetTemplane)
    {
        if (targetTemplane == null) return;

        // ��]��K�p
        targetTemplane.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);

        // �X�P�[����K�p
        float desiredWorldScale = Mathf.Min(allowedScales[currentScaleIndex], cellSize);
        float currentPrefabSize = samanTemplates[currentTemplateIndex].prefabWorldSize;
        float scaleRatio = desiredWorldScale / currentPrefabSize;

        targetTemplane.transform.localScale = new Vector3(scaleRatio, 1, scaleRatio);

        Vector2Int currentGridCoords = WorldToGrid(targetTemplane.transform.position);
        targetTemplane.transform.position = GridToWorld(currentGridCoords, desiredWorldScale);

        UnityEngine.Debug.Log($"TemplanePlacement: Applied rotation {currentRotationY}�� and scale {scaleRatio}");
    }

    /// <summary>
    /// ���ݑI������Ă���e���v���[�g�̏����擾
    /// </summary>
    /// <returns>�I�𒆂̃e���v���[�g</returns>
    public SamanTemplate GetCurrentTemplate()
    {
        if (currentTemplateIndex >= 0 && currentTemplateIndex < samanTemplates.Count)
        {
            return samanTemplates[currentTemplateIndex];
        }
        return null;
    }

    void OnDrawGizmos()
    {
        if (groundPlaneRenderer == null) return;

        Gizmos.color = Color.yellow;
        Vector3 groundCenter = groundPlaneRenderer.transform.position;
        Vector3 groundSize = groundPlaneRenderer.bounds.size;

        float halfX = groundSize.x / 2f;
        float halfZ = groundSize.z / 2f;

        Vector3 startPos = groundCenter - new Vector3(halfX, groundSize.y / 2f, halfZ);

        for (int x = 0; x <= (int)groundSize.x; x++)
        {
            Vector3 p1 = startPos + new Vector3(x * cellSize, 0.005f, 0);
            Vector3 p2 = startPos + new Vector3(x * cellSize, 0.005f, groundSize.z);
            Gizmos.DrawLine(p1, p2);
        }
        for (int z = 0; z <= (int)groundSize.z; z++)
        {
            Vector3 p1 = startPos + new Vector3(0, 0.005f, z * cellSize);
            Vector3 p2 = startPos + new Vector3(groundSize.x, 0.005f, z * cellSize);
            Gizmos.DrawLine(p1, p2);
        }
    }
}