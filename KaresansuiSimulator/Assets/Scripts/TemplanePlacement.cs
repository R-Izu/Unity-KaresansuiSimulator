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
        public float prefabWorldSize = 10f; // �v���n�u�̎��ۂ̃��[���h�T�C�Y�i�f�t�H���g�v���[����10x10�j
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
    public float[] allowedScales = { 1f, 2f, 3f }; // ���ۂ̃��[���h�T�C�Y�icellSize�Ɉˑ����Ȃ��j
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
        if (Input.GetMouseButtonDown(0))
        {
            PlaceTemplane();
        }

        for (int i = 0; i < samanTemplates.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetCurrentTemplate(i);
            }
        }

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

    void PlaceTemplane()
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

            if (placedTemplanes.ContainsKey(gridCoords) && placedTemplanes[gridCoords] != null)
            {
                Destroy(placedTemplanes[gridCoords]);
                placedTemplanes.Remove(gridCoords);
                UnityEngine.Debug.Log($"TemplanePlacement: Replaced existing templane at {gridCoords}.");
            }

            GameObject newTemplane = Instantiate(samanTemplates[currentTemplateIndex].prefab);
            // �v���[���T�C�Y���Z���T�C�Y�ȉ��ɐ���
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

        // �O���b�h�Z���̍����p�̍��W
        float cellMinX = groundBottomLeft.x + (gridCoords.x * cellSize);
        float cellMinZ = groundBottomLeft.z + (gridCoords.y * cellSize);

        // �v���[�����Z�����Ɏ��܂�悤�ɔz�u
        if (templateActualWorldScale <= cellSize)
        {
            // �v���[�����Z����菬�����ꍇ�F�Z���̒��S�ɔz�u
            float centerX = cellMinX + (cellSize / 2f);
            float centerZ = cellMinZ + (cellSize / 2f);
            return new Vector3(centerX, groundPlaneRenderer.transform.position.y + 0.001f, centerZ);
        }
        else
        {
            // �v���[�����Z�����傫���ꍇ�F�Z���̍����p����ɔz�u
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
            // ��]����T�C�Y�i�Z���T�C�Y�ȉ��ɐ����j
            float newScale = Mathf.Min(allowedScales[currentScaleIndex], cellSize);

            // �v���n�u�̌��̃T�C�Y���l�������X�P�[���v�Z
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
    /// �C���ŁF�v���n�u�̌��̃T�C�Y���l�������X�P�[���K�p�icellSize�Ɉˑ����Ȃ��A�Z�����Ɏ��܂�悤�ɐ����j
    /// </summary>
    void ApplyCurrentRotationAndScale(GameObject targetTemplane)
    {
        if (targetTemplane == null) return;

        targetTemplane.transform.rotation = Quaternion.identity;

        // ��]���郏�[���h�T�C�Y�i�Z���T�C�Y�ȉ��ɐ����j
        float desiredWorldScale = Mathf.Min(allowedScales[currentScaleIndex], cellSize);

        // �v���n�u�̌��̃��[���h�T�C�Y���擾
        float currentPrefabSize = samanTemplates[currentTemplateIndex].prefabWorldSize;

        // �X�P�[���䗦���v�Z�F��]�T�C�Y / ���̃T�C�Y
        float scaleRatio = desiredWorldScale / currentPrefabSize;

        // �v�Z���ꂽ�X�P�[���䗦��K�p
        targetTemplane.transform.localScale = new Vector3(scaleRatio, 1, scaleRatio);

        // �ʒu���O���b�h�ɍ��킹����
        Vector2Int currentGridCoords = WorldToGrid(targetTemplane.transform.position);
        targetTemplane.transform.position = GridToWorld(currentGridCoords, desiredWorldScale);

        UnityEngine.Debug.Log($"TemplanePlacement: Applied scale - Desired: {desiredWorldScale}, Prefab size: {currentPrefabSize}, Scale ratio: {scaleRatio}, Cell size: {cellSize}");
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