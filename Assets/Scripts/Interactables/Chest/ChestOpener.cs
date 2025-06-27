using UnityEngine;

public class ChestOpener : MonoBehaviour
{
    public Animator animator;

    public void OpenChest()
    {
        animator.SetTrigger("Open");
    }
}
