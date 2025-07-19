// GameManager.cs - 新しい方針に合わせた修正版
using UnityEngine;
using UnityEngine.UI; // UI要素を操作するために必要

public enum GameMode
{
    None,
    // SamanDrawing, // これを削除するか、使わないままにしておく（今回は削除推奨）
    ComponentPlacement, // 岩や木などのコンポーネントを配置するモード
    SamanTemplatePlacement, // 新しい砂紋テンプレート配置モード
    SeasonSelection // 季節を選択するモード (今後追加)
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Current Mode")]
    [SerializeField] private GameMode currentMode = GameMode.ComponentPlacement;

    [Header("UI Elements (Assign in Inspector)")]
    // public Button samanDrawingModeButton; // 削除する
    public Button componentPlacementModeButton;
    public Button samanTemplatePlacementModeButton; // 新しい砂紋配置モード用のボタンを追加

    [Header("Managers (Assign in Inspector)")]
    // public SamanApplier samanApplier; // 削除する
    public GardenBuilder gardenBuilder;
    public TemplanePlacement templanePlacement; // 新しいマネージャーへの参照

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
        if (samanTemplatePlacementModeButton != null) // 新しいボタンの設定
        {
            samanTemplatePlacementModeButton.onClick.AddListener(() => SetMode(GameMode.SamanTemplatePlacement));
        }

        // 初期モードに基づいて、各マネージャーの有効/無効を切り替える
        UpdateManagerActivity();
        UpdateUIStates(); // UIの初期状態を更新
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

    private void UpdateManagerActivity()
    {
        // SamanApplierへの参照は削除済みなので、ここでコメントアウト/削除
        // if (samanApplier != null) { samanApplier.enabled = (currentMode == GameMode.SamanDrawing); }

        if (gardenBuilder != null)
        {
            gardenBuilder.enabled = (currentMode == GameMode.ComponentPlacement);
        }
        if (templanePlacement != null) // 新しいマネージャーの有効/無効
        {
            templanePlacement.enabled = (currentMode == GameMode.SamanTemplatePlacement);
        }
    }

    private void UpdateUIStates()
    {
        // SamanDrawingModeButtonをSamanTemplatePlacementModeButtonに置き換えるので、古い参照は削除
        // if (samanDrawingModeButton != null) { samanDrawingModeButton.interactable = (currentMode != GameMode.SamanDrawing); }

        if (componentPlacementModeButton != null)
        {
            componentPlacementModeButton.interactable = (currentMode != GameMode.ComponentPlacement);
            //componentPlacementModeButton.GetComponent<Image>().color = (currentMode == GameMode.ComponentPlacement) ? Color.yellow : Color.white;
        }
        if (samanTemplatePlacementModeButton != null) // 新しいボタンの状態更新
        {
            samanTemplatePlacementModeButton.interactable = (currentMode != GameMode.SamanTemplatePlacement);
            //samanTemplatePlacementModeButton.GetComponent<Image>().color = (currentMode == GameMode.SamanTemplatePlacement) ? Color.yellow : Color.white;
        }
    }

    // デバッグ用のキー入力（UI作成前の一時的なテスト用）
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetMode(GameMode.SamanTemplatePlacement); // F1キーを新しいモードに割り当てる
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SetMode(GameMode.ComponentPlacement);
        }
    }
}