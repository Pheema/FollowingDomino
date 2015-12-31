using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraRigController : MonoBehaviour {
    public float smoothTime = 0.7f;
    // カメラとオブジェクト間の距離
    public float distance = 10f;
    // 極座標
    public float phi, theta;
    public float rotationSpeed = 5f;

    public Transform m_InitalTarget;

    Vector3 m_targetPos = Vector3.zero;
    Vector3 m_cameraVelocity;

    public float detectDuration = 0.1f;
    // 衝突時刻、オブジェクトの履歴を保持するキュー
    Queue<KeyValuePair<float, Transform>> recentCollEvents = new Queue<KeyValuePair<float, Transform>>();

    void Start()
    {
        recentCollEvents.Enqueue(new KeyValuePair<float, Transform>(Time.time, m_InitalTarget));
    }

    void Update()
    {
        // 最新の衝突イベントは残す
        // それ以外は時間が経過したら消去する
        if (recentCollEvents.Count > 1)
        {
            if (Time.time - recentCollEvents.Peek().Key > detectDuration)
            {
                recentCollEvents.Dequeue();
            }
        }

        // カメラの回転
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        phi += h * rotationSpeed;
        theta += v * rotationSpeed;
        theta = Mathf.Clamp(theta, -90f, 90f);
    }

    void FixedUpdate()
    {
        // カメラの回転操作
        transform.rotation = Quaternion.Euler(new Vector3(theta, -phi, 0));
        //transform.Rotate(new Vector3(theta, phi, 0), Space.Self);

        // 衝突イベントの平均位置を取得
        m_targetPos = Vector3.zero;
        foreach (var val in recentCollEvents)
        {
            m_targetPos += val.Value.position;
        }
        m_targetPos /= recentCollEvents.Count;
        
        transform.position = Vector3.SmoothDamp(transform.position, m_targetPos, ref m_cameraVelocity, smoothTime);
    }

    public void AddCollEvent(Transform newTransform)
    {
        recentCollEvents.Enqueue(new KeyValuePair<float, Transform>(Time.time, newTransform));
    }
}
