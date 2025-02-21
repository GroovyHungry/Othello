using UnityEngine;

public class OthelloCell_forDebug : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log($"{gameObject.name} がクリックされました！", this);
    }
}
