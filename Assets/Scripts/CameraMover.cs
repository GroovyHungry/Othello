using UnityEngine;
using Cysharp.Threading.Tasks;
public class CameraMover : MonoBehaviour
{
    public Vector3 startPos;  // メニュー表示位置
    public Vector3 targetPos; // ボード表示位置
    public float duration = 2.0f;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        cam.transform.position = startPos;
    }

    public async UniTask MoveToGameView()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            cam.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            await UniTask.Yield(); // 1フレーム待つ
        }

        cam.transform.position = targetPos; // 最終的にピッタリ合わせる
    }
}
