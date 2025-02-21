using UnityEngine;

public class OthelloCell : MonoBehaviour
{
    public GameObject whitePiecePrefab;
    public GameObject BlackPiecePrefab;

    private bool isOccupied = false;
    private static bool isWhiteTurn = true;

    public int x, y;

    private void OnMouseDown()
    {
        if(!isOccupied && OthelloBoard.Instance.IsCellEmpty(x, y))
        {
            GameObject piece = Instantiate(
                isWhiteTurn ? whitePiecePrefab : BlackPiecePrefab,
                transform.position,
                Quaternion.identity
            );

            isOccupied = true;
            OthelloBoard.Instance.PlacePiece(x, y, piece);
            isWhiteTurn = !isWhiteTurn;
        }
    }
}