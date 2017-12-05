using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class C01Script : MonoBehaviour
{
    /// <summary>
    /// マウスのいる位置に向くオブジェクト
    /// </summary>
    [SerializeField]
    private GameObject capsule = null;

    /// <summary>
    /// カプセルの角度の保管用
    /// </summary>
    private float targetAngle = 0.0f;

    /// <summary>
    /// カプセルの回転する速さ
    /// </summary>
    [SerializeField]
    private float capsuleRotationSpeed = 4.0f;

    /// <summary>
    /// クリックした位置に生成される球
    /// </summary>
    private GameObject sphere = null;

    /// <summary>
    /// クリックしていた時間を保存する
    /// </summary>
    private float buttonDownTime = 0.0f;

    /// <summary>
    /// クリックして生成された球のxの値の移動量調整用
    /// </summary>
    [SerializeField]
    private float sphereMagnitudeX = 2.0f;

    /// <summary>
    /// クリックして生成された球のyの値の移動量調整用
    /// </summary>
    [SerializeField]
    private float sphereMagnitudeY = 3.0f;

    /// <summary>
    /// 
    /// </summary>
    [SerializeField]
    private float sphereFrequency = 1.0f;
	
	void Start ()
    {
		
	}

    void Update()
    {
        //マウスの右クリックしたかつ、UI上でクリックしていなければ実行
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("posX :" + Input.mousePosition.x + "posY :" + Input.mousePosition.y);

            //クリックした位置に応じたカプセルの傾きを求める
            targetAngle = GetRotationAngleByTargetPosition(Input.mousePosition);

            //球がすでに存在した場合は削除する
            if(sphere != null)
            {
                Destroy(sphere);
                sphere = null;
            }

            //クリックした位置に球を生成する
            sphere = SpawnSphereAt(Input.mousePosition);

            //クリックしていた時間を保存する
            buttonDownTime = Time.time;
        }

        //カプセルをクリックした方向まで線形補間を用いて徐々に動くようにする
        capsule.transform.eulerAngles =
            new Vector3(0, 0, Mathf.LerpAngle(capsule.transform.eulerAngles.z, targetAngle, Time.deltaTime * capsuleRotationSpeed));

        //球をクリックした位置からカプセルの位置までに徐々に波打ちながら寄っていく
        if (sphere != null)
        {
            sphere.transform.position = new Vector3(sphere.transform.position.x + (capsule.transform.position.x - sphere.transform.position.x) * Time.deltaTime * sphereMagnitudeX,
                                                    Mathf.Abs(Mathf.Sin((Time.time - buttonDownTime) * (Mathf.PI * 2) * sphereFrequency) * sphereMagnitudeY),
                                                    0.0f);
        }

    }

    //カプセルを傾ける角度を算出する関数
    private float GetRotationAngleByTargetPosition(Vector3 mousePosition)
    {
        //カプセルの位置をワールド座標からスクリーン座標に変換
        Vector3 selfScreenPoint = Camera.main.WorldToScreenPoint(capsule.transform.position);

        //カプセルの位置とクリックした位置との差分を求める
        Vector3 diff = mousePosition - selfScreenPoint;

        //アークタンジェントを使用して角度を求める、最後にラジアンではなく度に戻す
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        Debug.Log("Angleは" + angle);

        //最終的に今回使用する角度を求めるため90度引く
        float finalAngle = angle - 90.0f;
        Debug.Log("finalAngleは" + finalAngle);

        return finalAngle;
    }

    //クリックした位置に球を生成する関数
    private GameObject SpawnSphereAt(Vector3 mousePosition)
    {
        //球のゲームオブジェクトを生成
        GameObject sp = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        //ワールド座標からスクリーン座標に変換する
        Vector3 selfScreenPoint = Camera.main.WorldToScreenPoint(capsule.transform.position);

        //生成した球のポジションを設定する
        Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, selfScreenPoint.z));
        sp.transform.position = new Vector3(position.x, position.y, 0.0f);

        return sp;
    }

}
