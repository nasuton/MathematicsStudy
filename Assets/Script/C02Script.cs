using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C02Script : MonoBehaviour
{
    /// <summary>
    /// 回転速度
    /// </summary>
    [SerializeField]
    private float rotateSpeed = 1.0f;

    /// <summary>
    /// 
    /// </summary>
    [SerializeField]
    private float scrollSpeed = 200.0f;

    /// <summary>
    /// 球面座標の原点
    /// </summary>
    [SerializeField]
    private Transform pivot;

    [System.Serializable]
    public class SphericalCoordinates
    {
        /// <summary>
        /// 球面座標の半径
        /// </summary>
        private float _radius = 0.0f;

        /// <summary>
        /// _radiusのゲッターとセッター作成
        /// </summary>
        public float Radius
        {
            get { return _radius; }

            //値を制限する
            private set
            {
                _radius = Mathf.Clamp(value, _minRadius, _maxRadius);
            }
        }

        /// <summary>
        /// 方位角
        /// </summary>
        private float _azimuth = 0.0f;

        /// <summary>
        /// _azimuthのゲッターとセッターの作成
        /// </summary>
        public float Azimuth
        {
            get { return _azimuth; }

            //最大値と最小値の値の間を繰り返す
            private set
            {
                _azimuth = Mathf.Repeat(value, _maxAzimuth - minAzimuth);
            }
        }

        /// <summary>
        /// 仰角
        /// </summary>
        private float _elevation = 0.0f;

        /// <summary>
        /// _elevationのゲッターとセッターを作成
        /// </summary>
        public float Elevation
        {
            get { return _elevation; }

            //値を制限する
            private set
            {
                _elevation = Mathf.Clamp(value, _minElevation, _maxElevation);
            }
        }

        /// <summary>
        /// _radiusの最大値と最小値の設定
        /// </summary>
        [SerializeField]
        private float _minRadius = 3.0f;
        [SerializeField]
        private float _maxRadius = 20.0f;

        /// <summary>
        /// _azimuthの最大値と最小値の設定
        /// </summary>
        [SerializeField]
        private float minAzimuth = 0.0f;
        private float _minAzimuth = 0.0f;
        [SerializeField]
        private float maxAzimuth = 360.0f;
        private float _maxAzimuth = 0.0f;

        /// <summary>
        /// _azimuthの最大値と最小値の設定
        /// </summary>
        [SerializeField]
        private float minElevation = 0.0f;
        private float _minElevation = 0.0f;
        [SerializeField]
        private float maxElevation = 90.0f;
        private float _maxElevation = 0.0f;

        public SphericalCoordinates()
        {

        }

        /// <summary>
        /// このクラスのコンストラクタで引数にデカルト座標の数値を受け取り、
        /// 球面座標の初期値を設定する
        /// </summary>
        /// <param name="cartesianCoordinate"></param>
        public SphericalCoordinates(Vector3 cartesianCoordinate)
        {
            //度数をラジアンに変換
            _minAzimuth = Mathf.Deg2Rad * minAzimuth;
            _maxAzimuth = Mathf.Deg2Rad * maxAzimuth;

            //度数をラジアンに変換
            _minElevation = Mathf.Deg2Rad * minElevation;
            _maxElevation = Mathf.Deg2Rad * maxElevation;

            //カメラと原点(今回はキューブ)の距離
            Radius = cartesianCoordinate.magnitude;

            //x座標とz座標からなる直角三角形の内角としての方位角をアークタンジェントで求める
            Azimuth = Mathf.Atan2(cartesianCoordinate.z, cartesianCoordinate.x);

            //y座標と半径rからなる直角三角形の内角として仰角をアークサインで求める
            Elevation = Mathf.Asin(cartesianCoordinate.y / Radius);
        }


    }

    private SphericalCoordinates sphericalCoordinates;

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

}
