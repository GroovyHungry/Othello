using UnityEngine;
using AK.Wwise;

public class TitleSoundPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlayPlaceSound()
    {
        AkSoundEngine.PostEvent("PlacePiece", gameObject);
    }
    public void PlayFlipSound()
    {
        AkSoundEngine.PostEvent("FlipPiece", gameObject);
    }
}
