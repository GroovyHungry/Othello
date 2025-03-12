using UnityEngine;

public class OthelloGrid : MonoBehaviour
{
    public GameObject cellPrefab; // `Cell` のプレハブ
    private int gridSize = 8; // 8×8 のオセロ盤

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
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
                if(cellScript != null)
                {
                    cellScript.x = x;
                    cellScript.y = y;
                }
            }
        }
    }
}
