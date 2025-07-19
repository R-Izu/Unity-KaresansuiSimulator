// GardenBuilder.cs
using UnityEngine;
using System.Collections.Generic; // List���g�p���邽�߂ɕK�v

public class GardenBuilder : MonoBehaviour
{
    [System.Serializable]
    public class GardenComponent
    {
        public string name; // UI�Ȃǂŕ\������R���|�[�l���g��
        public GameObject prefab; // �z�u����v���n�u
        public int maxCount = 10; // �z�u�\�ȍő吔 (Inspector�Őݒ�)
        [HideInInspector] public int currentCount = 0; // ���ݔz�u����Ă��鐔 (�����p)
    }

    [Header("Garden Components")]
    public List<GardenComponent> gardenComponents; // �z�u�\�ȃR���|�[�l���g�̃��X�g

    [Header("Placement Settings")]
    public LayerMask placementLayer; // �z�u�\�Ȓn�ʂ̃��C���[ (��: "Ground")
    public Transform parentObjectForComponents; // �z�u���ꂽ�R���|�[�l���g�̐e�ƂȂ�I�u�W�F�N�g

    private GardenComponent _selectedComponent; // ���ݑI������Ă���R���|�[�l���g

    void Start()
    {
        // �����I���R���|�[�l���g��ݒ� (��: ���X�g�̍ŏ��̗v�f)
        if (gardenComponents.Count > 0)
        {
            SetSelectedComponent(0);
        }

        // �e�I�u�W�F�N�g���ݒ肳��Ă��Ȃ��ꍇ�A�V�����쐬����
        if (parentObjectForComponents == null)
        {
            GameObject parentGO = new GameObject("GardenElements");
            parentObjectForComponents = parentGO.transform;
            Debug.Log("Parent object 'GardenElements' created for components.");
        }
    }

    void Update()
    {
        // �}�E�X�̍��N���b�N�ŃI�u�W�F�N�g��z�u
        if (Input.GetMouseButtonDown(0))
        {
            PlaceSelectedComponent();
        }

        // �L�[�{�[�h���͂ŃR���|�[�l���g��؂�ւ���� (UI�����O�̈ꎞ�I�ȃe�X�g�p)
        // 1�L�[�ōŏ��̃R���|�[�l���g�A2�L�[��2�Ԗڂ̃R���|�[�l���g...
        for (int i = 0; i < gardenComponents.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) // Alpha1�̓L�[�{�[�h��1
            {
                SetSelectedComponent(i);
                Debug.Log($"Selected component: {_selectedComponent.name}");
            }
        }
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
            Debug.Log($"Selected component set to: {_selectedComponent.name}");
        }
        else
        {
            Debug.LogWarning("Invalid component index selected.");
        }
    }

    /// <summary>
    /// ���ݑI������Ă���R���|�[�l���g���}�E�X�̈ʒu�ɔz�u���܂��B
    /// </summary>
    private void PlaceSelectedComponent()
    {
        if (_selectedComponent == null || _selectedComponent.prefab == null)
        {
            Debug.LogWarning("No component or prefab selected for placement.");
            return;
        }

        if (_selectedComponent.currentCount >= _selectedComponent.maxCount)
        {
            Debug.LogWarning($"Max count ({_selectedComponent.maxCount}) reached for {_selectedComponent.name}. Cannot place more.");
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

            Debug.Log($"Placed {_selectedComponent.name} at {hit.point}. Current count: {_selectedComponent.currentCount}/{_selectedComponent.maxCount}");
        }
        else
        {
            Debug.Log("No ground hit for placement.");
        }
    }

    // �����I�ɔz�u���ꂽ�I�u�W�F�N�g���폜����@�\�Ȃǂ�ǉ�����ꍇ�ɔ�����
    public void RemoveComponent(GameObject componentToRemove)
    {
        // �ǂ̃R���|�[�l���g�^�C�v�����ʂ��AcurrentCount�����炷���W�b�N���K�v�ɂȂ�
        // ��: componentToRemove.name ���猳�̃v���n�u���𐄑����AgardenComponents ���X�g������
        // ���̗�ł͊ȗ����̂��߁A���� currentCount �����炳�Ȃ�
        Destroy(componentToRemove);
        Debug.Log($"Removed {componentToRemove.name}.");
    }

    // UI����Ăяo�����߂̃w���p�[���\�b�h (��: �z�u�������Z�b�g�������ꍇ)
    public void ResetComponentCounts()
    {
        foreach (var comp in gardenComponents)
        {
            comp.currentCount = 0;
        }
        Debug.Log("All component counts reset.");
    }
}