// GardenBuilder.cs
using UnityEngine;
using System.Collections.Generic; // Listを使用するために必要

public class GardenBuilder : MonoBehaviour
{
    [System.Serializable]
    public class GardenComponent
    {
        public string name; // UIなどで表示するコンポーネント名
        public GameObject prefab; // 配置するプレハブ
        public int maxCount = 10; // 配置可能な最大数 (Inspectorで設定)
        [HideInInspector] public int currentCount = 0; // 現在配置されている数 (内部用)
    }

    [Header("Garden Components")]
    public List<GardenComponent> gardenComponents; // 配置可能なコンポーネントのリスト

    [Header("Placement Settings")]
    public LayerMask placementLayer; // 配置可能な地面のレイヤー (例: "Ground")
    public Transform parentObjectForComponents; // 配置されたコンポーネントの親となるオブジェクト

    private GardenComponent _selectedComponent; // 現在選択されているコンポーネント

    void Start()
    {
        // 初期選択コンポーネントを設定 (例: リストの最初の要素)
        if (gardenComponents.Count > 0)
        {
            SetSelectedComponent(0);
        }

        // 親オブジェクトが設定されていない場合、新しく作成する
        if (parentObjectForComponents == null)
        {
            GameObject parentGO = new GameObject("GardenElements");
            parentObjectForComponents = parentGO.transform;
            Debug.Log("Parent object 'GardenElements' created for components.");
        }
    }

    void Update()
    {
        // マウスの左クリックでオブジェクトを配置
        if (Input.GetMouseButtonDown(0))
        {
            PlaceSelectedComponent();
        }

        // キーボード入力でコンポーネントを切り替える例 (UI実装前の一時的なテスト用)
        // 1キーで最初のコンポーネント、2キーで2番目のコンポーネント...
        for (int i = 0; i < gardenComponents.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) // Alpha1はキーボードの1
            {
                SetSelectedComponent(i);
                Debug.Log($"Selected component: {_selectedComponent.name}");
            }
        }
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
            Debug.Log($"Selected component set to: {_selectedComponent.name}");
        }
        else
        {
            Debug.LogWarning("Invalid component index selected.");
        }
    }

    /// <summary>
    /// 現在選択されているコンポーネントをマウスの位置に配置します。
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
            // ヒットした位置にプレハブをインスタンス化
            GameObject newComponent = Instantiate(_selectedComponent.prefab, hit.point, Quaternion.identity);

            // 親オブジェクトを設定
            newComponent.transform.SetParent(parentObjectForComponents);

            // 配置されたオブジェクトの数を更新
            _selectedComponent.currentCount++;

            Debug.Log($"Placed {_selectedComponent.name} at {hit.point}. Current count: {_selectedComponent.currentCount}/{_selectedComponent.maxCount}");
        }
        else
        {
            Debug.Log("No ground hit for placement.");
        }
    }

    // 将来的に配置されたオブジェクトを削除する機能などを追加する場合に備えて
    public void RemoveComponent(GameObject componentToRemove)
    {
        // どのコンポーネントタイプか判別し、currentCountを減らすロジックが必要になる
        // 例: componentToRemove.name から元のプレハブ名を推測し、gardenComponents リストを検索
        // この例では簡略化のため、直接 currentCount を減らさない
        Destroy(componentToRemove);
        Debug.Log($"Removed {componentToRemove.name}.");
    }

    // UIから呼び出すためのヘルパーメソッド (例: 配置数をリセットしたい場合)
    public void ResetComponentCounts()
    {
        foreach (var comp in gardenComponents)
        {
            comp.currentCount = 0;
        }
        Debug.Log("All component counts reset.");
    }
}