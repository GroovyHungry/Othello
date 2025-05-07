using UnityEngine;

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
        DifficultySelectManager.Instance.OnSecretButtonClicked();
    }
}
