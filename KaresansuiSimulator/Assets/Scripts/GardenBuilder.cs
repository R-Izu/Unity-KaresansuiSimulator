// GardenBuilder.cs - 修正版
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
        // 初期選択コンポーネントを設定
        if (gardenComponents.Count > 0)
        {
            SetSelectedComponent(0);
        }

        // 親オブジェクトが設定されていない場合、新しく作成する
        if (parentObjectForComponents == null)
        {
            GameObject parentGO = new GameObject("GardenElements");
            parentObjectForComponents = parentGO.transform;
            UnityEngine.Debug.Log("Parent object 'GardenElements' created for components.");
        }
    }

    void Update()
    {
        // GameManagerがアクティブでない場合は処理を停止
        if (!IsActiveMode()) return;

        // キーボード入力のみをここで処理（マウス入力はGameManagerが管理）
        HandleKeyboardInput();
    }

    /// <summary>
    /// 現在このマネージャーがアクティブかどうかをチェック
    /// </summary>
    /// <returns>アクティブかどうか</returns>
    private bool IsActiveMode()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.IsCurrentMode(GameMode.ComponentPlacement);
    }

    /// <summary>
    /// キーボード入力を処理（GameManagerから呼び出される）
    /// </summary>
    public void HandleKeyboardInput()
    {
        // コンポーネント切り替え用のキー入力
        for (int i = 0; i < gardenComponents.Count && i < 9; i++) // 最大9個まで
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetSelectedComponent(i);
                UnityEngine.Debug.Log($"Selected component: {_selectedComponent.name}");
            }
        }
    }

    /// <summary>
    /// マウスクリック処理（GameManagerから呼び出される）
    /// </summary>
    public void HandleMouseClick()
    {
        if (!IsActiveMode()) return;
        PlaceSelectedComponent();
    }

    /// <summary>
    /// 指定されたインデックスのコンポーネントを選択します。
    /// UIから呼び出すことを想定しています。
    /// </summary>
    /// <param name="index">選択するコンポーネントのインデックス</param>
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
    /// 現在選択されているコンポーネントをマウスの位置に配置します。
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
            // ヒットした位置にプレハブをインスタンス化
            GameObject newComponent = Instantiate(_selectedComponent.prefab, hit.point, Quaternion.identity);

            // 親オブジェクトを設定
            newComponent.transform.SetParent(parentObjectForComponents);

            // 配置されたオブジェクトの数を更新
            _selectedComponent.currentCount++;

            UnityEngine.Debug.Log($"Placed {_selectedComponent.name} at {hit.point}. Current count: {_selectedComponent.currentCount}/{_selectedComponent.maxCount}");
        }
        else
        {
            UnityEngine.Debug.Log("No ground hit for placement.");
        }
    }

    /// <summary>
    /// 配置されたコンポーネントを削除します
    /// </summary>
    /// <param name="componentToRemove">削除するオブジェクト</param>
    public void RemoveComponent(GameObject componentToRemove)
    {
        // 実装時は適切なコンポーネント特定ロジックが必要
        // 簡略化のため、直接削除のみ実行
        Destroy(componentToRemove);
        UnityEngine.Debug.Log($"Removed {componentToRemove.name}.");
    }

    /// <summary>
    /// 全コンポーネントの配置数をリセット
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
    /// 現在選択されているコンポーネントの情報を取得
    /// </summary>
    /// <returns>選択中のコンポーネント</returns>
    public GardenComponent GetSelectedComponent()
    {
        return _selectedComponent;
    }

    /// <summary>
    /// 指定したコンポーネントの配置可能数を取得
    /// </summary>
    /// <param name="index">コンポーネントのインデックス</param>
    /// <returns>配置可能数（現在数/最大数）</returns>
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