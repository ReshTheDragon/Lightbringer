using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float destroyDelay = 1f;

    void Start()
    {
        Destroy(gameObject, destroyDelay);
    }
}
