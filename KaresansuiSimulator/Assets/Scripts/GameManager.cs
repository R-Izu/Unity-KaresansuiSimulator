// GameManager.cs - �V�������j�ɍ��킹���C����
using UnityEngine;
using UnityEngine.UI; // UI�v�f�𑀍삷�邽�߂ɕK�v

public enum GameMode
{
    None,
    // SamanDrawing, // ������폜���邩�A�g��Ȃ��܂܂ɂ��Ă����i����͍폜�����j
    ComponentPlacement, // ���؂Ȃǂ̃R���|�[�l���g��z�u���郂�[�h
    SamanTemplatePlacement, // �V��������e���v���[�g�z�u���[�h
    SeasonSelection // �G�߂�I�����郂�[�h (����ǉ�)
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Current Mode")]
    [SerializeField] private GameMode currentMode = GameMode.ComponentPlacement;

    [Header("UI Elements (Assign in Inspector)")]
    // public Button samanDrawingModeButton; // �폜����
    public Button componentPlacementModeButton;
    public Button samanTemplatePlacementModeButton; // �V��������z�u���[�h�p�̃{�^����ǉ�

    [Header("Managers (Assign in Inspector)")]
    // public SamanApplier samanApplier; // �폜����
    public GardenBuilder gardenBuilder;
    public TemplanePlacement templanePlacement; // �V�����}�l�[�W���[�ւ̎Q��

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
        if (samanTemplatePlacementModeButton != null) // �V�����{�^���̐ݒ�
        {
            samanTemplatePlacementModeButton.onClick.AddListener(() => SetMode(GameMode.SamanTemplatePlacement));
        }

        // �������[�h�Ɋ�Â��āA�e�}�l�[�W���[�̗L��/������؂�ւ���
        UpdateManagerActivity();
        UpdateUIStates(); // UI�̏�����Ԃ��X�V
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
        // SamanApplier�ւ̎Q�Ƃ͍폜�ς݂Ȃ̂ŁA�����ŃR�����g�A�E�g/�폜
        // if (samanApplier != null) { samanApplier.enabled = (currentMode == GameMode.SamanDrawing); }

        if (gardenBuilder != null)
        {
            gardenBuilder.enabled = (currentMode == GameMode.ComponentPlacement);
        }
        if (templanePlacement != null) // �V�����}�l�[�W���[�̗L��/����
        {
            templanePlacement.enabled = (currentMode == GameMode.SamanTemplatePlacement);
        }
    }

    private void UpdateUIStates()
    {
        // SamanDrawingModeButton��SamanTemplatePlacementModeButton�ɒu��������̂ŁA�Â��Q�Ƃ͍폜
        // if (samanDrawingModeButton != null) { samanDrawingModeButton.interactable = (currentMode != GameMode.SamanDrawing); }

        if (componentPlacementModeButton != null)
        {
            componentPlacementModeButton.interactable = (currentMode != GameMode.ComponentPlacement);
            //componentPlacementModeButton.GetComponent<Image>().color = (currentMode == GameMode.ComponentPlacement) ? Color.yellow : Color.white;
        }
        if (samanTemplatePlacementModeButton != null) // �V�����{�^���̏�ԍX�V
        {
            samanTemplatePlacementModeButton.interactable = (currentMode != GameMode.SamanTemplatePlacement);
            //samanTemplatePlacementModeButton.GetComponent<Image>().color = (currentMode == GameMode.SamanTemplatePlacement) ? Color.yellow : Color.white;
        }
    }

    // �f�o�b�O�p�̃L�[���́iUI�쐬�O�̈ꎞ�I�ȃe�X�g�p�j
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetMode(GameMode.SamanTemplatePlacement); // F1�L�[��V�������[�h�Ɋ��蓖�Ă�
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SetMode(GameMode.ComponentPlacement);
        }
    }
}