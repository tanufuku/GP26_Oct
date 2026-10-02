using UnityEngine;

/// <summary>
/// プレイヤーを追いかけるカメラ。
/// 角度は固定で、位置だけ滑らかに追従し、常にプレイヤーの方を向く（LookAt）。
/// 「カメラを子にする」方法だとキャラの回転にカメラも巻き込まれて酔いやすいので、
/// スクリプトで位置だけを追いかける。
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;                          // 追いかける相手（プレイヤー）
    [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -7f); // 相手から見たカメラの位置
    [SerializeField] private float smoothSpeed = 8f;                     // 追従の速さ（大きいほどピタッと付く）
    [SerializeField] private float lookHeight = 1f;                      // 見る高さ（足元ではなく胸あたり）

    // プレイヤーが Update で動いた「あと」にカメラを動かしたいので LateUpdate を使う。
    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}
