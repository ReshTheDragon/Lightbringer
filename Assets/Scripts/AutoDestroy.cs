using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float destroyDelay = 0.5f;

    void Start()
    {
        Destroy(gameObject, destroyDelay);
    }
}
