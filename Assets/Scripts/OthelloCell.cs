using UnityEngine;

public class OthelloCell : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log($"{gameObject.name} がクリックされました！", this);
    }
}
