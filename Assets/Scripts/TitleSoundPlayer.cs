using UnityEngine;
using AK.Wwise;

public class TitleSoundPlayer : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event FlipPiece;
    [SerializeField] private AK.Wwise.Event PlacePiece;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlayPlaceSound()
    {
        PlacePiece.Post(gameObject);
    }
    public void PlayFlipSound()
    {
        FlipPiece.Post(gameObject);
    }
}
