using UnityEngine;

public class OthelloCell : MonoBehaviour
{
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab; // 🔥 変数名を統一（大文字を修正）

    private bool isOccupied = false; // 🔥 初期配置時に影響しないようにする

    public int x, y;

    private void OnMouseDown()
    {
        if (!isOccupied && OthelloBoard.Instance.IsCellEmpty(x, y))
        {
            GameObject piece = Instantiate(
                OthelloBoard.Instance.IsWhiteTurn() ? whitePiecePrefab : blackPiecePrefab,
                transform.position,
                Quaternion.identity
            );

            isOccupied = true;
            OthelloBoard.Instance.PlacePiece(x, y, piece);
        }
    }
}
