using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraRigController : MonoBehaviour
{

    #region Member variables

    // ターゲットに近づくまでの時間
    [SerializeField, Range(0f, 1f)]
    float m_smoothTime = 0.7f;

    // 極座標
    public float m_phi, m_theta;
    public float m_rotationSpeed = 1f;

    // 初期ターゲット
    [SerializeField]
    Transform m_InitalTarget;

    // ターゲットの座標
    Vector3 m_targetPos;
    public Vector3 targetPos
    {
        get { return m_targetPos; }
    }
    
    // カメラの速度
    Vector3 m_cameraVelocity;

    // 衝突イベントを保持する時間
    [SerializeField]
    float m_detectDuration = 1.0f;

    // 衝突時刻、オブジェクトの履歴を保持するキュー
    Queue<KeyValuePair<float, Transform>> recentCollEvents = new Queue<KeyValuePair<float, Transform>>();

    #endregion

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
            if ((Time.time - recentCollEvents.Peek().Key) > m_detectDuration)
            {
                recentCollEvents.Dequeue();
            }
        }

        // キー操作によるカメラの回転
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        m_phi += h * m_rotationSpeed;
        m_theta += v * m_rotationSpeed;
        m_theta = Mathf.Clamp(m_theta, -90f, 90f);
    }

    void FixedUpdate()
    {
        // 衝突イベントの平均位置を取得
        m_targetPos = Vector3.zero;
        foreach (var val in recentCollEvents)
        {
            m_targetPos += val.Value.position;
        }
        m_targetPos /= recentCollEvents.Count;

        transform.position = Vector3.SmoothDamp(transform.position, m_targetPos, ref m_cameraVelocity, m_smoothTime);
        transform.rotation = Quaternion.Euler(new Vector3(m_theta, -m_phi, 0));
    }

    public void AddCollEvent(Transform newTransform)
    {
        recentCollEvents.Enqueue(new KeyValuePair<float, Transform>(Time.time, newTransform));
    }
}
