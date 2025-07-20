// TemplanePlacement.cs - UI回転機能追加版
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
        public Sprite previewImage; // プレビュー用画像
        [TextArea(2, 4)]
        public string description = ""; // テンプレートの説明
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
    private float currentRotationY = 0f; // 現在の回転角度を追跡

    [Header("UI Elements")]
    [SerializeField] private Button rotateLeftButton;
    [SerializeField] private Button rotateRightButton;
    [SerializeField] private Button scaleButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Image currentTemplatePreview;
    [SerializeField] private TextMeshProUGUI currentTemplateNameText;         // TextからTextMeshProUGUIに変更
    [SerializeField] private TextMeshProUGUI currentTemplateDescriptionText;  // TextからTextMeshProUGUIに変更
    [SerializeField] private TextMeshProUGUI rotationAngleText;             // TextからTextMeshProUGUIに変更
    [SerializeField] private TextMeshProUGUI scaleText;                     // TextからTextMeshProUGUIに変更

    [Header("Template Selection UI")]
    public List<Button> templateSelectionButtons; // 各テンプレート選択ボタンのリスト
    public List<Text> templateNameTexts;          // 各テンプレート選択ボタンの子のテキスト

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
        // GameManagerがアクティブでない場合は処理を停止
        if (!IsActiveMode()) return;

        // キーボード入力のみはここで処理（マウス入力はGameManagerが処理）
        HandleKeyboardInput();
    }

    /// <summary>
    /// UI要素の初期化とイベントリスナーの設定
    /// </summary>
    private void InitializeUI()
    {
        // 回転ボタンのイベント設定
        if (rotateLeftButton != null)
            rotateLeftButton.onClick.AddListener(() => RotateTemplate(-90f));

        if (rotateRightButton != null)
            rotateRightButton.onClick.AddListener(() => RotateTemplate(90f));

        // スケールボタンのイベント設定
        if (scaleButton != null)
            scaleButton.onClick.AddListener(ScaleSelectedTemplane);

        // 削除ボタンのイベント設定
        if (deleteButton != null)
            deleteButton.onClick.AddListener(DeleteSelectedTemplane);

        // テンプレート選択ボタンのイベント設定とテキスト初期化
        for (int i = 0; i < templateSelectionButtons.Count; i++)
        {
            // クロージャの問題を避けるためにローカル変数にコピー
            int index = i;

            if (templateSelectionButtons[index] != null)
            {
                // ボタンがクリックされたら SetCurrentTemplate メソッドを呼び出す
                templateSelectionButtons[index].onClick.AddListener(() => SetCurrentTemplate(index));

                // 対応するテンプレートが存在し、テキスト要素が割り当てられている場合、ボタンのテキストを設定
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
    /// UI表示を更新
    /// </summary>
    private void UpdateUI()
    {
        if (samanTemplates.Count == 0 || currentTemplateIndex < 0 || currentTemplateIndex >= samanTemplates.Count)
            return;

        var currentTemplate = samanTemplates[currentTemplateIndex];

        // プレビュー画像の更新
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

        // テンプレート名の更新
        if (currentTemplateNameText != null)
            currentTemplateNameText.text = currentTemplate.name;

        // テンプレート説明の更新
        if (currentTemplateDescriptionText != null)
            currentTemplateDescriptionText.text = currentTemplate.description;

        // 回転角度の表示更新
        if (rotationAngleText != null)
            rotationAngleText.text = $"Rotation: {currentRotationY:F0}°";

        // スケールの表示更新
        if (scaleText != null)
            scaleText.text = $"Scale: {allowedScales[currentScaleIndex]:F1}x";

        // ボタンの有効/無効状態を更新
        bool hasSelectedTemplate = currentlySelectedTemplaneInScene != null;
        if (rotateLeftButton != null) rotateLeftButton.interactable = hasSelectedTemplate;
        if (rotateRightButton != null) rotateRightButton.interactable = hasSelectedTemplate;
        if (scaleButton != null) scaleButton.interactable = hasSelectedTemplate;
        if (deleteButton != null) deleteButton.interactable = hasSelectedTemplate;
    }

    /// <summary>
    /// 現在このマネージャーがアクティブかどうかをチェック
    /// </summary>
    /// <returns>アクティブかどうか</returns>
    private bool IsActiveMode()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.IsCurrentMode(GameMode.SamanTemplatePlacement);
    }

    /// <summary>
    /// キーボード入力を処理（GameManagerから呼び出される）
    /// </summary>
    public void HandleKeyboardInput()
    {
        // テンプレート選択
        for (int i = 0; i < samanTemplates.Count && i < 9; i++) // 最大9個まで
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetCurrentTemplate(i);
            }
        }

        // 回転・スケール・削除（既存の機能を維持）
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateTemplate(90f); // 既存の機能と統合
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
    /// マウスクリック処理（GameManagerから呼び出される）
    /// </summary>
    public void HandleMouseClick()
    {
        if (!IsActiveMode()) return;
        PlaceTemplane();
    }

    /// <summary>
    /// テンプレートを指定角度回転（UI用）
    /// </summary>
    /// <param name="angle">回転角度（度）</param>
    public void RotateTemplate(float angle)
    {
        currentRotationY += angle;
        currentRotationY = currentRotationY % 360f; // 0-360度の範囲に正規化

        if (currentlySelectedTemplaneInScene != null)
        {
            currentlySelectedTemplaneInScene.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);
            UnityEngine.Debug.Log($"TemplanePlacement: Rotated template to {currentRotationY}°");
        }

        UpdateUI();
    }

    /// <summary>
    /// 既存の回転機能（90度回転）- 下位互換性のために維持
    /// </summary>
    public void RotateSelectedTemplane()
    {
        RotateTemplate(90f);
    }

    /// <summary>
    /// テンプレートを配置
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

            // 既存のテンプレートがある場合は削除
            if (placedTemplanes.ContainsKey(gridCoords) && placedTemplanes[gridCoords] != null)
            {
                Destroy(placedTemplanes[gridCoords]);
                placedTemplanes.Remove(gridCoords);
                UnityEngine.Debug.Log($"TemplanePlacement: Replaced existing templane at {gridCoords}.");
            }

            // 新しいテンプレートを配置
            GameObject newTemplane = Instantiate(samanTemplates[currentTemplateIndex].prefab);
            float desiredWorldScale = Mathf.Min(allowedScales[currentScaleIndex], cellSize);
            newTemplane.transform.position = GridToWorld(gridCoords, desiredWorldScale);
            newTemplane.transform.SetParent(groundPlaneRenderer.transform);

            currentlySelectedTemplaneInScene = newTemplane;
            ApplyCurrentRotationAndScale(newTemplane);

            placedTemplanes.Add(gridCoords, newTemplane);
            UnityEngine.Debug.Log($"TemplanePlacement: Placed '{samanTemplates[currentTemplateIndex].name}' at grid {gridCoords}.");

            UpdateUI(); // UI状態を更新
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
    /// グリッド座標をワールド座標（グリッドセル内に収まる位置）に変換します。
    /// </summary>
    /// <param name="templateActualWorldScale">配置するテンプレートの実際のワールドスケール</param>
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
            // 新しいテンプレートを選択したら回転をリセット
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

            UpdateUI(); // UI状態を更新
        }
        else
        {
            UnityEngine.Debug.LogWarning("TemplanePlacement: No templane selected in scene for deletion.");
        }
    }

    /// <summary>
    /// プレハブの元のサイズを考慮したスケール適用
    /// </summary>
    private void ApplyCurrentRotationAndScale(GameObject targetTemplane)
    {
        if (targetTemplane == null) return;

        // 回転を適用
        targetTemplane.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);

        // スケールを適用
        float desiredWorldScale = Mathf.Min(allowedScales[currentScaleIndex], cellSize);
        float currentPrefabSize = samanTemplates[currentTemplateIndex].prefabWorldSize;
        float scaleRatio = desiredWorldScale / currentPrefabSize;

        targetTemplane.transform.localScale = new Vector3(scaleRatio, 1, scaleRatio);

        Vector2Int currentGridCoords = WorldToGrid(targetTemplane.transform.position);
        targetTemplane.transform.position = GridToWorld(currentGridCoords, desiredWorldScale);

        UnityEngine.Debug.Log($"TemplanePlacement: Applied rotation {currentRotationY}° and scale {scaleRatio}");
    }

    /// <summary>
    /// 現在選択されているテンプレートの情報を取得
    /// </summary>
    /// <returns>選択中のテンプレート</returns>
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