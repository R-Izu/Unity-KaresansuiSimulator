// GameManager.cs - �C����
using UnityEngine;
using UnityEngine.UI;

public enum GameMode
{
    None,
    ComponentPlacement,
    SamanTemplatePlacement,
    SeasonSelection
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Current Mode")]
    [SerializeField] private GameMode currentMode = GameMode.ComponentPlacement;

    [Header("UI Elements (Assign in Inspector)")]
    public Button componentPlacementModeButton;
    public Button samanTemplatePlacementModeButton;

    [Header("Managers (Assign in Inspector)")]
    public GardenBuilder gardenBuilder;
    public TemplanePlacement templanePlacement;

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
        // UI�{�^���̃C�x���g���X�i�[��ݒ�
        if (componentPlacementModeButton != null)
        {
            componentPlacementModeButton.onClick.AddListener(() => SetMode(GameMode.ComponentPlacement));
        }
        if (samanTemplatePlacementModeButton != null)
        {
            samanTemplatePlacementModeButton.onClick.AddListener(() => SetMode(GameMode.SamanTemplatePlacement));
        }

        // ������Ԃ�ݒ�
        UpdateManagerActivity();
        UpdateUIStates();
    }

    void Update()
    {
        // �}�E�X�N���b�N�������ꌳ��
        HandleMouseInput();

        // �f�o�b�O�p�L�[����
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetMode(GameMode.SamanTemplatePlacement);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SetMode(GameMode.ComponentPlacement);
        }
    }

    /// <summary>
    /// �}�E�X���͂��ꌳ�Ǘ����A���݂̃��[�h�ɉ����ēK�؂ȃ}�l�[�W���[�ɏ������Ϗ�
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
    /// �L�[�{�[�h���͂��ꌳ�Ǘ����A���݂̃��[�h�ɉ����ēK�؂ȃ}�l�[�W���[�ɏ������Ϗ�
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
    /// ���݂̃��[�h�ɉ����ă}�l�[�W���[�̗L��/������؂�ւ�
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
    }

    /// <summary>
    /// UI�̏�Ԃ����݂̃��[�h�ɉ����čX�V
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
    }

    /// <summary>
    /// ���݃A�N�e�B�u�ȃ��[�h���ǂ������`�F�b�N�i�e�}�l�[�W���[���g�p�j
    /// </summary>
    /// <param name="mode">�`�F�b�N���郂�[�h</param>
    /// <returns>���݃A�N�e�B�u���ǂ���</returns>
    public bool IsCurrentMode(GameMode mode)
    {
        return currentMode == mode;
    }
}