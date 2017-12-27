using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C03Script : MonoBehaviour
{
    [SerializeField]
    private float rotateSpeed = 90.0f;

    [SerializeField]
    private float scrollSpeed = 200.0f;

    [SerializeField]
    private Transform pivot;

    [Serializable]
    private class SphericalCoordinates
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

        public Vector3 ToCartesian
        {
            get
            {
                //球面の半径Radiusを斜辺とする、内角が球座標の仰角Elevationにあたる直角三角形から、
                //Cosを用いてカメラ位置からx軸z軸の平面へ垂直に引いた線とx軸z軸の平面が接する位置から原点までの距離rangeを求める
                float range = Radius * Mathf.Cos(Elevation);

                //rangeは、方位角Azimuthを内角とする直角三角形の斜辺の長さなので、隣辺にあたるx座標はCos、対辺あたるz座標はSinを掛けて求める。
                //y座標は、rangeを求めたのと同じ、Radiusが斜辺の長さrで内角がElevationの直角三角形でSinを用いて求める。
                return new Vector3(range * Mathf.Cos(Azimuth),
                                   Radius * Mathf.Sin(Elevation),
                                   range * Mathf.Sin(Azimuth));
            }
        }

        public SphericalCoordinates Rotate(float newAzimuth, float newElevatation)
        {
            Azimuth += newAzimuth;
            Elevation += newElevatation;

            return this;
        }

        public SphericalCoordinates TranslateRadius(float x)
        {
            Radius += x;

            return this;
        }
    }

    [SerializeField]
    private SphericalCoordinates sphericalCoordinates;

    //対象のオブジェクトの頂点座標を保存するための変数
    private List<Vector3> triangleVertices = new List<Vector3>();

    void Start ()
    {
        //スクリプトを初期化
        sphericalCoordinates = new SphericalCoordinates(transform.position);

        //アタッチしたGameObjectのpositionを設定する
        transform.position = sphericalCoordinates.ToCartesian + pivot.position;

        //対象のオブジェクトから適当にMesh内から3つの持ってくる
        Mesh _mesh = pivot.gameObject.GetComponent<MeshFilter>().mesh;
        for(int i = 0; i < _mesh.vertices.Length; i++)
        {
            if(triangleVertices.Count < 3)
            {
                triangleVertices.Add(_mesh.vertices[i]);
            }
        }
    }
	
	void Update ()
    {
        DrawCameraLine();
        float kh, kv, mh, mv, h, v;
        kh = Input.GetAxis("Horizontal");
        kv = Input.GetAxis("Vertical");

        bool anyMouseButton = Input.GetMouseButton(0) | Input.GetMouseButton(1) | Input.GetMouseButton(2);
        mh = anyMouseButton ? Input.GetAxis("Mouse X") : 0f;
        mv = anyMouseButton ? Input.GetAxis("Mouse Y") : 0f;

        h = kh * kh > mh * mh ? kh : mh;
        v = kv * kv > mv * mv ? kv : mv;

        if (h * h > Mathf.Epsilon || v * v > Mathf.Epsilon)
        {
            transform.position
                = sphericalCoordinates.Rotate(h * rotateSpeed * Time.deltaTime, v * rotateSpeed * Time.deltaTime).ToCartesian + pivot.position;
        }

        float sw = -Input.GetAxis("Mouse ScrollWheel");
        if (sw * sw > Mathf.Epsilon)
        {
            transform.position = sphericalCoordinates.TranslateRadius(sw * Time.deltaTime * scrollSpeed).ToCartesian + pivot.position;
        }

        //pivotに設定したGameObjectの方へ、このスクリプトをアタッチされたGameObjectを向かせる。
        transform.LookAt(pivot.position);

    }

    private void DrawCameraLine()
    {
        //対象のオブジェクトから線を描画する
        Debug.DrawLine(pivot.position, pivot.transform.forward * 2.0f, Color.blue);

        //cameraから出ている線の終点を求める
        Vector3 cameraPoint = transform.position + transform.forward * 5.0f;

        //triangleVerticesに入って３つの頂点座標を使って、三角形の各辺に相当するベクトルと、
        //カメラから出ている線の先端と三角形の各頂点とを結ぶベクトルとを３頂点分求める。
        Vector3 edge1 = triangleVertices[1] - triangleVertices[0];
        Vector3 edge2 = cameraPoint - triangleVertices[1];

        Vector3 edge3 = triangleVertices[2] - triangleVertices[1];
        Vector3 edge4 = cameraPoint - triangleVertices[2];

        Vector3 edge5 = triangleVertices[0] - triangleVertices[2];
        Vector3 edge6 = cameraPoint - triangleVertices[0];

        //双方のベクトルの外積を３頂点分求める。
        Vector3 cp1 = Vector3.Cross(edge1, edge2);
        Vector3 cp2 = Vector3.Cross(edge3, edge4);
        Vector3 cp3 = Vector3.Cross(edge5, edge6);

        //cp1と他の外積cp2、cp3各々との内積を取って符号を見る。
        //内積が正の値であるということは、法線ベクトルが反対向きになっていないことを意味するため、
        //他の法線ベクトル双方と反対向きでないならば、全ての頂点上の法線ベクトルが同じ方向を向いていることになり、
        //カメラから出ている線の先端が、triangleVerticesにセットした三角形上に乗っていることを示す。
        if (Vector3.Dot(cp1, cp2) > 0 && Vector3.Dot(cp1, cp3) > 0)
        {
            Debug.DrawLine(transform.position, cameraPoint, Color.red);
        }
        else
        {
            Debug.DrawLine(transform.position, cameraPoint, Color.green);
        }
    }
}
