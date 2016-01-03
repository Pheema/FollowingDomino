using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraRigController : MonoBehaviour
{
    enum FOLLOWING_TYPE
    {
        NORMAL,
        WEIGHTING
    };

    #region Member variables

    // 衝突イベントの重み付け
    // NORMAL: 均一, WEIGHTING: 線形に重み付け
    [SerializeField]
    FOLLOWING_TYPE m_followingType = FOLLOWING_TYPE.NORMAL;

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

    // 実行開始から衝突を無視する時間
    [SerializeField]
    float m_detectionStartTime = 1f;
    public float detectionStartTime
    {
        get { return m_detectionStartTime; }
    }

    // 衝突イベントを保持する時間
    [SerializeField]
    float m_detectDuration = 1f;
    
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
        
        switch(m_followingType)
        {
            case FOLLOWING_TYPE.NORMAL:
                foreach (var val in recentCollEvents)
                {
                    m_targetPos += val.Value.position;
                }
                m_targetPos /= recentCollEvents.Count;
                break;

            case FOLLOWING_TYPE.WEIGHTING:
                // 最新のデータは重み1
                m_targetPos += recentCollEvents.Peek().Value.position;
                
                if (recentCollEvents.Count > 1)
                {
                    float weightSum = 1f;
                    foreach (var val in recentCollEvents)
                    {
                        // 最新の衝突データは重み付け平均に含めない
                        // if (Mathf.Approximately(val.Key, recentCollEvents.Peek().Key)) continue;
                        float elapsedTime = Time.time - val.Key;
                        float weight = Mathf.Max(0f, 
                                Mathf.Min(
                                    2f * elapsedTime / m_detectDuration, 
                                    2f * (1f - elapsedTime / m_detectDuration)
                                )
                            );
                        m_targetPos += weight * val.Value.position;
                        weightSum += weight;
                    }
                    m_targetPos /= weightSum;
                }
                break;
        }

        transform.position = Vector3.SmoothDamp(transform.position, m_targetPos, ref m_cameraVelocity, m_smoothTime);
        transform.rotation = Quaternion.Euler(m_theta, -m_phi, 0f);
    }

    public void AddCollEvent(Transform newTransform)
    {
        recentCollEvents.Enqueue(new KeyValuePair<float, Transform>(Time.time, newTransform));
    }

    // メインカメラから見て最も外側にある点のワールド座標を取得
    // w_: ワールド座標, s_: スクリーン座標
    public Vector3 GetOutermostPoint()
    {
        Vector3 w_outermostPoint = Camera.main.transform.position + transform.forward;
        foreach (var collEvent in recentCollEvents)
        {
            Vector3 s_point = Camera.main.WorldToScreenPoint(collEvent.Value.position);
            Vector3 s_outermostPoint = Camera.main.WorldToScreenPoint(w_outermostPoint);
            if (s_point.magnitude > s_outermostPoint.magnitude)
            {
                w_outermostPoint = collEvent.Value.position;
            }

        }
        return w_outermostPoint;
    }
}
