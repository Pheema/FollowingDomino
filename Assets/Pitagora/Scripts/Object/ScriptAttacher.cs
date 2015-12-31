using UnityEngine;
using System.Collections;

public class ScriptAttacher : MonoBehaviour
{

    void Awake()
    {
        foreach(Transform child in transform)
        {
            if (child.CompareTag("Domino"))
            {
                child.gameObject.AddComponent<CameraTargetSetter>();
            }
        }
    }
}
