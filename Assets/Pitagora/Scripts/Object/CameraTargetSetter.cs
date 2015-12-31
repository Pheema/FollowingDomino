using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CameraTargetSetter : MonoBehaviour {
    // このドミノが倒れているか
    float m_detectionStartTime = 1f;
    bool m_isFalled = false;
    CameraRigController m_cameraRigController;

    // デバッグ用
    bool isDebug = true;
    Vector3 collPoint;

    void Start()
    {
        m_cameraRigController = Camera.main.transform.parent.GetComponent<CameraRigController>();
        if (isDebug)
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }

    void Update()
    {
        if (isDebug && m_isFalled)
        {
            Debug.DrawRay(collPoint, Vector3.up);
            GetComponent<Renderer>().material.color = Color.red;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        if (Time.time < m_detectionStartTime) return;
        if (!m_isFalled && coll.gameObject.CompareTag("Domino"))
        {
            m_isFalled = true;
            collPoint = coll.contacts[0].point;
            m_cameraRigController.AddCollEvent(transform);
        }
    }
}
