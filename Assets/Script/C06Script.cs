using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class C06Script : MonoBehaviour
{
    [SerializeField]
    private GameObject targetObject;

    private float targetRotationTime = 0.0f;

    private Quaternion targetRotationFrom = Quaternion.identity;
    private Quaternion targetRotationTo = Quaternion.identity;

    private bool spinnig = true;
    private bool rotating = false;

	void Start ()
    {
        
	}
	
	void Update ()
    {
        /*
         * 元の位置から指定したペースで
         * 回転していくようにする
         */
        if (spinnig)
        {
            Quaternion targetSpinRotation = Quaternion.AngleAxis(-180.0f, Vector3.up);
            targetObject.transform.rotation = Quaternion.Slerp(targetObject.transform.rotation, targetSpinRotation, 0.05f);
        }

        /*
         * カメラの制御を行ってる。
         */
        Quaternion cameraRotaion = Quaternion.LookRotation(targetObject.transform.position + new Vector3(0.0f, 0.5f, 0.0f) - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, cameraRotaion, Time.deltaTime);
        transform.Translate(0.02f, 0.005f, 0.5f * Time.deltaTime);

        /*
         * 該当するキー入力されるとそれにあった回転軸を設定する
         * その後、回転をしていく
         */
        if(rotating)
        {
            targetRotationTime += Time.deltaTime / 0.5f;
            targetObject.transform.rotation = Quaternion.Slerp(targetRotationFrom, targetRotationTo, targetRotationTime);

            if(targetRotationTime >= 1.0f)
            {
                rotating = false;
                targetRotationTime = 0.0f;
            }
        }
        else
        {
            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                ResetTargetObjectRotation(Vector3.right);
                rotating = true;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ResetTargetObjectRotation(Vector3.left);
                rotating = true;
            }
            else if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                ResetTargetObjectRotation(Vector3.forward);
                rotating = true;
            }
            else if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ResetTargetObjectRotation(Vector3.back);
                rotating = true;
            }
        }
	}

    public class QuaternionComparer : IEqualityComparer<Quaternion>
    {
        public bool Equals(Quaternion lhs, Quaternion rhs)
        {
            return lhs == rhs;
        }

        public int GetHashCode(Quaternion obj)
        {
            return obj.GetHashCode();
        }
    }

    /*
     * targetRotationFromに現在の回転角度を代入する
     * targetRotationToには現在の回転角度に変異するクォータニオンを合成した回転後のクォータニオンを設定する
     * 
     */
    void ResetTargetObjectRotation(Vector3 axis)
    {
        spinnig = false;
        targetRotationFrom = targetObject.transform.rotation;

        Quaternion q = Quaternion.AngleAxis(90.0f, Quaternion.Inverse(targetRotationFrom) * axis);
        targetRotationTo = targetRotationFrom * q;

        Assert.IsTrue(Quaternion.Inverse(targetRotationFrom) * axis == targetObject.transform.InverseTransformVector(axis));
        Assert.AreEqual<Quaternion>(targetRotationFrom * q, Quaternion.AngleAxis(90.0f, axis) * targetRotationFrom, null, new QuaternionComparer());
    }

}
