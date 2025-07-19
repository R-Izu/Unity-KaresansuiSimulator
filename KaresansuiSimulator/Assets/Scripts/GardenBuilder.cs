// GardenBuilder.cs - �C����
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class GardenBuilder : MonoBehaviour
{
    [System.Serializable]
    public class GardenComponent
    {
        public string name;
        public GameObject prefab;
        public int maxCount = 10;
        [HideInInspector] public int currentCount = 0;
    }

    [Header("Garden Components")]
    public List<GardenComponent> gardenComponents;

    [Header("Placement Settings")]
    public LayerMask placementLayer;
    public Transform parentObjectForComponents;

    private GardenComponent _selectedComponent;

    void Start()
    {
        // �����I���R���|�[�l���g��ݒ�
        if (gardenComponents.Count > 0)
        {
            SetSelectedComponent(0);
        }

        // �e�I�u�W�F�N�g���ݒ肳��Ă��Ȃ��ꍇ�A�V�����쐬����
        if (parentObjectForComponents == null)
        {
            GameObject parentGO = new GameObject("GardenElements");
            parentObjectForComponents = parentGO.transform;
            UnityEngine.Debug.Log("Parent object 'GardenElements' created for components.");
        }
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
               GameManager.Instance.IsCurrentMode(GameMode.ComponentPlacement);
    }

    /// <summary>
    /// �L�[�{�[�h���͂������iGameManager����Ăяo�����j
    /// </summary>
    public void HandleKeyboardInput()
    {
        // �R���|�[�l���g�؂�ւ��p�̃L�[����
        for (int i = 0; i < gardenComponents.Count && i < 9; i++) // �ő�9�܂�
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetSelectedComponent(i);
                UnityEngine.Debug.Log($"Selected component: {_selectedComponent.name}");
            }
        }
    }

    /// <summary>
    /// �}�E�X�N���b�N�����iGameManager����Ăяo�����j
    /// </summary>
    public void HandleMouseClick()
    {
        if (!IsActiveMode()) return;
        PlaceSelectedComponent();
    }

    /// <summary>
    /// �w�肳�ꂽ�C���f�b�N�X�̃R���|�[�l���g��I�����܂��B
    /// UI����Ăяo�����Ƃ�z�肵�Ă��܂��B
    /// </summary>
    /// <param name="index">�I������R���|�[�l���g�̃C���f�b�N�X</param>
    public void SetSelectedComponent(int index)
    {
        if (index >= 0 && index < gardenComponents.Count)
        {
            _selectedComponent = gardenComponents[index];
            UnityEngine.Debug.Log($"Selected component set to: {_selectedComponent.name}");
        }
        else
        {
            UnityEngine.Debug.LogWarning($"Invalid component index selected: {index}");
        }
    }

    /// <summary>
    /// ���ݑI������Ă���R���|�[�l���g���}�E�X�̈ʒu�ɔz�u���܂��B
    /// </summary>
    private void PlaceSelectedComponent()
    {
        if (_selectedComponent == null || _selectedComponent.prefab == null)
        {
            UnityEngine.Debug.LogWarning("No component or prefab selected for placement.");
            return;
        }

        if (_selectedComponent.currentCount >= _selectedComponent.maxCount)
        {
            UnityEngine.Debug.LogWarning($"Max count ({_selectedComponent.maxCount}) reached for {_selectedComponent.name}. Cannot place more.");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, placementLayer))
        {
            // �q�b�g�����ʒu�Ƀv���n�u���C���X�^���X��
            GameObject newComponent = Instantiate(_selectedComponent.prefab, hit.point, Quaternion.identity);

            // �e�I�u�W�F�N�g��ݒ�
            newComponent.transform.SetParent(parentObjectForComponents);

            // �z�u���ꂽ�I�u�W�F�N�g�̐����X�V
            _selectedComponent.currentCount++;

            UnityEngine.Debug.Log($"Placed {_selectedComponent.name} at {hit.point}. Current count: {_selectedComponent.currentCount}/{_selectedComponent.maxCount}");
        }
        else
        {
            UnityEngine.Debug.Log("No ground hit for placement.");
        }
    }

    /// <summary>
    /// �z�u���ꂽ�R���|�[�l���g���폜���܂�
    /// </summary>
    /// <param name="componentToRemove">�폜����I�u�W�F�N�g</param>
    public void RemoveComponent(GameObject componentToRemove)
    {
        // �������͓K�؂ȃR���|�[�l���g���胍�W�b�N���K�v
        // �ȗ����̂��߁A���ڍ폜�̂ݎ��s
        Destroy(componentToRemove);
        UnityEngine.Debug.Log($"Removed {componentToRemove.name}.");
    }

    /// <summary>
    /// �S�R���|�[�l���g�̔z�u�������Z�b�g
    /// </summary>
    public void ResetComponentCounts()
    {
        foreach (var comp in gardenComponents)
        {
            comp.currentCount = 0;
        }
        UnityEngine.Debug.Log("All component counts reset.");
    }

    /// <summary>
    /// ���ݑI������Ă���R���|�[�l���g�̏����擾
    /// </summary>
    /// <returns>�I�𒆂̃R���|�[�l���g</returns>
    public GardenComponent GetSelectedComponent()
    {
        return _selectedComponent;
    }

    /// <summary>
    /// �w�肵���R���|�[�l���g�̔z�u�\�����擾
    /// </summary>
    /// <param name="index">�R���|�[�l���g�̃C���f�b�N�X</param>
    /// <returns>�z�u�\���i���ݐ�/�ő吔�j</returns>
    public (int current, int max) GetComponentCount(int index)
    {
        if (index >= 0 && index < gardenComponents.Count)
        {
            var comp = gardenComponents[index];
            return (comp.currentCount, comp.maxCount);
        }
        return (0, 0);
    }
}