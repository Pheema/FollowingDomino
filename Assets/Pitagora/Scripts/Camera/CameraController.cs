using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour {
    #region Member variables

    // カメラの移動
    [SerializeField]
    float m_smoothTime = 0.7f;

    // カメラ-ターゲット間の初期距離
    [SerializeField]
    float m_initialDistance = 10f;

    // カメラ-ターゲット間の距離制限
    [SerializeField] float m_distanceMin;
    [SerializeField] float m_distanceMax;

    float m_distance;
    float m_targetDistance;
    float m_cameraVelocity;
    CameraRigController m_cameraRigController;

    #endregion

	void Start () {
        m_cameraRigController = transform.parent.GetComponent<CameraRigController>();
        m_distance = m_targetDistance = m_initialDistance;
	}
	
	void FixedUpdate () {
        // カメラから見て最も外側の点のワールド座標を取得
        Vector3 w_outermostPoint = m_cameraRigController.GetOutermostPoint();
        Vector3 target2outermostPoint = w_outermostPoint - transform.parent.position;

        // 目標スクリーン幅の半分
        float halfTargetWidth = Mathf.Max(
                Vector3.Dot(target2outermostPoint, transform.right),
                Vector3.Dot(target2outermostPoint, transform.up) * Camera.main.aspect
            );
        m_targetDistance = halfTargetWidth / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        m_targetDistance = Mathf.Clamp(m_targetDistance, m_distanceMin, m_distanceMax);

        m_distance = Mathf.SmoothDamp(m_distance, m_targetDistance, ref m_cameraVelocity, m_smoothTime);
        transform.localPosition = new Vector3(0, 0, -m_distance);
	}
}
