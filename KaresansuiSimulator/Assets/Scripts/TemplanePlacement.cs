// TemplanePlacement.cs - �C����
using UnityEngine;
using System.Collections.Generic;

public class TemplanePlacement : MonoBehaviour
{
    [System.Serializable]
    public class SamanTemplate
    {
        public string name;
        public GameObject prefab;
        [Header("Prefab Size Settings")]
        public float prefabWorldSize = 10f;
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

    private Dictionary<Vector2Int, GameObject> placedTemplanes = new Dictionary<Vector2Int, GameObject>();

    void OnEnable()
    {
        UnityEngine.Debug.Log("TemplanePlacement: OnEnable called.");
        if (samanTemplates.Count > 0)
        {
            SetCurrentTemplate(0);
        }
    }

    void OnDisable()
    {
        UnityEngine.Debug.Log("TemplanePlacement: OnDisable called.");
    }

    void Update()
    {
        // GameManager���A�N�e�B�u�łȂ��ꍇ�͏������~
        if (!IsActiveMode()) return;

        // �L�[�{�[�h���݂͂̂������ŏ����i�}�E�X���͂�GameManager���Ǘ��j
        HandleKeyboardInput();
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

        // ��]�E�X�P�[���E�폜
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateSelectedTemplane();
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
            UnityEngine.Debug.Log($"TemplanePlacement: Selected template: {samanTemplates[index].name}");
        }
        else
        {
            UnityEngine.Debug.LogWarning($"TemplanePlacement: Invalid template index {index}.");
        }
    }

    public void RotateSelectedTemplane()
    {
        if (currentlySelectedTemplaneInScene != null)
        {
            currentlySelectedTemplaneInScene.transform.Rotate(0, 90, 0);
            UnityEngine.Debug.Log($"TemplanePlacement: Rotated selected templane. New rotation: {currentlySelectedTemplaneInScene.transform.localEulerAngles.y}");
        }
        else
        {
            UnityEngine.Debug.LogWarning("TemplanePlacement: No templane selected in scene for rotation.");
        }
    }

    public void ScaleSelectedTemplane()
    {
        if (currentlySelectedTemplaneInScene != null)
        {
            currentScaleIndex = (currentScaleIndex + 1) % allowedScales.Length;
            float newScale = Mathf.Min(allowedScales[currentScaleIndex], cellSize);

            float currentPrefabSize = samanTemplates[currentTemplateIndex].prefabWorldSize;
            float scaleRatio = newScale / currentPrefabSize;
            currentlySelectedTemplaneInScene.transform.localScale = new Vector3(scaleRatio, 1, scaleRatio);

            Vector2Int currentGridCoords = WorldToGrid(currentlySelectedTemplaneInScene.transform.position);
            currentlySelectedTemplaneInScene.transform.position = GridToWorld(currentGridCoords, newScale);

            UnityEngine.Debug.Log($"TemplanePlacement: Scaled selected templane to {newScale}x{newScale} (scale ratio: {scaleRatio}), Cell size: {cellSize}");
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

        targetTemplane.transform.rotation = Quaternion.identity;

        float desiredWorldScale = Mathf.Min(allowedScales[currentScaleIndex], cellSize);
        float currentPrefabSize = samanTemplates[currentTemplateIndex].prefabWorldSize;
        float scaleRatio = desiredWorldScale / currentPrefabSize;

        targetTemplane.transform.localScale = new Vector3(scaleRatio, 1, scaleRatio);

        Vector2Int currentGridCoords = WorldToGrid(targetTemplane.transform.position);
        targetTemplane.transform.position = GridToWorld(currentGridCoords, desiredWorldScale);

        UnityEngine.Debug.Log($"TemplanePlacement: Applied scale - Desired: {desiredWorldScale}, Prefab size: {currentPrefabSize}, Scale ratio: {scaleRatio}, Cell size: {cellSize}");
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