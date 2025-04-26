using UnityEngine;
using Cysharp.Threading.Tasks;
using AK.Wwise;

public class OthelloCell : MonoBehaviour
{
    public SpriteRenderer hoverFrame;
    public int x, y;
    private void Update()
    {
        if (OthelloManager.initializing || OthelloManager.Waiting || OthelloManager.isAIPlaying)
        {
            hoverFrame.enabled = false;
        }
    }

    private void OnMouseEnter()
    {
        if (OthelloManager.initializing) return;
        if (OthelloManager.Waiting) return;
        if (OthelloManager.isAIPlaying) return;

        string currentTag = OthelloManager.Instance.IsWhiteTurn() ? "White" : "Black";
        if (OthelloManager.Instance.GetBoard().IsCellEmpty(x, y) && OthelloManager.Instance.IsValidMove(x, y, currentTag))
        {
            hoverFrame.enabled = true;
            AkSoundEngine.PostEvent("OnSelect", gameObject);

        }
    }
    private void OnMouseExit()
    {
        hoverFrame.enabled = false;
    }
    private void OnMouseDown()
    {
        if (OthelloManager.initializing) return;
        if (OthelloManager.Waiting) return;
        if (OthelloManager.isAIPlaying) return;

        string currentTag = OthelloManager.Instance.IsWhiteTurn() ? "White" : "Black";

        if (OthelloManager.Instance.GetBoard().IsCellEmpty(x, y) && OthelloManager.Instance.IsValidMove(x, y, currentTag))
        {
            Vector3 pos = transform.position;
            _ = OthelloManager.Instance.PlacePieces(x, y, currentTag, pos);
        }
    }
}
