// GameManager.cs - 修正版
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GameMode
{
    None,
    ComponentPlacement,
    SamanTemplatePlacement,
    SeasonSelection,
    CameraControl
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Current Mode")]
    [SerializeField] private GameMode currentMode = GameMode.ComponentPlacement;

    [Header("UI Elements (Assign in Inspector)")]
    public Button componentPlacementModeButton;
    public Button samanTemplatePlacementModeButton;
    public Button cameraControlModeButton;

    [Header("Managers (Assign in Inspector)")]
    public GardenBuilder gardenBuilder;
    public TemplanePlacement templanePlacement;
    public CameraControl cameraControl;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        // UIボタンのイベントリスナーを設定
        if (componentPlacementModeButton != null)
        {
            componentPlacementModeButton.onClick.AddListener(() => SetMode(GameMode.ComponentPlacement));
        }
        if (samanTemplatePlacementModeButton != null)
        {
            samanTemplatePlacementModeButton.onClick.AddListener(() => SetMode(GameMode.SamanTemplatePlacement));
        }
        if (cameraControlModeButton != null)
        {
            cameraControlModeButton.onClick.AddListener(() => SetMode(GameMode.CameraControl));
        }


        // 初期状態を設定
        UpdateManagerActivity();
        UpdateUIStates();
    }

    void Update()
    {
        // マウスクリック処理を一元化
        HandleMouseInput();

        // キーボード入力を一元管理
        HandleKeyboardInput();

        // デバッグ用キー入力
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetMode(GameMode.SamanTemplatePlacement);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SetMode(GameMode.ComponentPlacement);
        }
        if (Input.GetKeyDown(KeyCode.F3)) 
        {
            SetMode(GameMode.CameraControl);
        }
    }

    /// <summary>
    /// マウス入力を一元管理し、現在のモードに応じて適切なマネージャーに処理を委譲
    /// </summary>
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            switch (currentMode)
            {
                case GameMode.ComponentPlacement:
                    if (gardenBuilder != null && gardenBuilder.enabled)
                    {
                        gardenBuilder.HandleMouseClick();
                    }
                    break;

                case GameMode.SamanTemplatePlacement:
                    if (templanePlacement != null && templanePlacement.enabled)
                    {
                        templanePlacement.HandleMouseClick();
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// キーボード入力を一元管理し、現在のモードに応じて適切なマネージャーに処理を委譲
    /// </summary>
    private void HandleKeyboardInput()
    {
        switch (currentMode)
        {
            case GameMode.ComponentPlacement:
                if (gardenBuilder != null && gardenBuilder.enabled)
                {
                    gardenBuilder.HandleKeyboardInput();
                }
                break;

            case GameMode.SamanTemplatePlacement:
                if (templanePlacement != null && templanePlacement.enabled)
                {
                    templanePlacement.HandleKeyboardInput();
                }
                break;
        }
    }

    public void SetMode(GameMode newMode)
    {
        if (currentMode == newMode) return;

        currentMode = newMode;
        Debug.Log($"Game Mode changed to: {currentMode}");

        UpdateManagerActivity();
        UpdateUIStates();
    }

    public GameMode GetCurrentMode()
    {
        return currentMode;
    }

    /// <summary>
    /// 現在のモードに応じてマネージャーの有効/無効を切り替え
    /// </summary>
    private void UpdateManagerActivity()
    {
        if (gardenBuilder != null)
        {
            gardenBuilder.enabled = (currentMode == GameMode.ComponentPlacement);
        }
        if (templanePlacement != null)
        {
            templanePlacement.enabled = (currentMode == GameMode.SamanTemplatePlacement);
        }
        if (cameraControl != null)
        {
            // cameraControl.enabled = (currentMode == GameMode.CameraControl); // 直接 enabled を操作する代わりにカスタムメソッドを使用
            if (currentMode == GameMode.CameraControl)
            {
                cameraControl.EnableCameraControl();
            }
            else
            {
                cameraControl.DisableCameraControl();
            }
        }

    }

    /// <summary>
    /// UIの状態を現在のモードに応じて更新
    /// </summary>
    private void UpdateUIStates()
    {
        if (componentPlacementModeButton != null)
        {
            componentPlacementModeButton.interactable = (currentMode != GameMode.ComponentPlacement);
        }
        if (samanTemplatePlacementModeButton != null)
        {
            samanTemplatePlacementModeButton.interactable = (currentMode != GameMode.SamanTemplatePlacement);
        }
        if (cameraControlModeButton != null)
        {
            cameraControlModeButton.interactable = (currentMode != GameMode.CameraControl);
        }

    }

    /// <summary>
    /// 現在アクティブなモードかどうかをチェック（各マネージャーが使用）
    /// </summary>
    /// <param name="mode">チェックするモード</param>
    /// <returns>現在アクティブかどうか</returns>
    public bool IsCurrentMode(GameMode mode)
    {
        return currentMode == mode;
    }
}