using UnityEngine;

/// <summary>
/// 8x8 のオセロ盤を動的に生成するクラス
/// </summary>
public class OthelloGrid : MonoBehaviour
{
    /// <summary>
    /// セルプレハブ
    /// </summary>
    public GameObject cellPrefab; // `Cell` のプレハブ

    /// <summary>
    /// 盤面のサイズ (8×8)
    /// </summary>
    private int gridSize = 8; // 8×8 のオセロ盤

    /// <summary>
    /// ゲーム開始時にグリッドを生成する
    /// </summary>
    private void Start()
    {
        GenerateGrid();
    }

    /// <summary>
    /// セルの座標と名前を設定して盤面に配置する
    /// </summary>
    private void GenerateGrid()
    {
        float offset = (gridSize - 1) / 2.0f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 position = new Vector3(x - offset, y - offset, -5);
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                cell.name = $"Cell ({x},{y})";

                OthelloCell cellScript = cell.GetComponent<OthelloCell>();
                if (cellScript != null)
                {
                    cellScript.x = x;
                    cellScript.y = y;
                }
            }
        }
    }
}
