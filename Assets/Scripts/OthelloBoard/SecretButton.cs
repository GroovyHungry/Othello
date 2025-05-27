using UnityEngine;

/// <summary>
/// 隠し難易度のボタンの挙動を制御するクラス
/// asepriteのファイルはButtonオブジェクトにアニメーションを追加できないため，スクリプトで管理する
/// </summary>
public class SecretButton : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnMouseEnter()
    {
        animator.SetTrigger("Highlighted");
    }

    private void OnMouseExit()
    {
        animator.SetTrigger("Normal");
    }

    private void OnMouseDown()
    {
        DifficultySelect.Instance.OnSecretButtonClicked();
    }
}
