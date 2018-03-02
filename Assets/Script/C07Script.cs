using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

//この属性を付与すると再生ボタンを押さずともこのスクリプトが実行される
[ExecuteInEditMode]
public class C07Script : MonoBehaviour
{
    private LineRenderer lineRenderer;

    public GameObject[] controlPoints = new GameObject[3];

    [SerializeField]
    private Color color = Color.red;

    [SerializeField]
    private float width = 0.1f;

    [SerializeField]
    private int numberOfPoints = 20;

    public enum CurveType
    {
        Bezier,
        CatmllRom,
        BSpline,
    }

    public CurveType curveType = CurveType.Bezier;

    private CurveType oldCurveType = CurveType.Bezier;

    private int vertexCountDiff = 0;

    /// <summary>
    /// 関数オブジェクトを設定する
    /// Funcは戻り値あり
    /// Actionは戻り値なし
    /// </summary>
    private Func<int, bool> CheckBoundary = null;
    private Action<int> SetupPoints = null;
    private Func<float, Vector3> ParametricTransformWithGeometryMatrix = null;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        if(controlPoints == null || controlPoints.Length < 3)
        {
            Debug.LogError("Control Points are not found");
            return;
        }

        if(numberOfPoints < 2)
        {
            numberOfPoints = 2;
        }

        if(oldCurveType != curveType)
        {
            CheckBoundary = null;
            SetupPoints = null;
            ParametricTransformWithGeometryMatrix = null;

            oldCurveType = curveType;
        }

        //２次ベジェスプラインの実装
        if(CheckBoundary == null)
        {
            Matrix4x4 matrix = new Matrix4x4();

            if (curveType == CurveType.Bezier)
            {
                vertexCountDiff = 2;

                CheckBoundary = (i) => {
                    return !(controlPoints[i] == null
                        || controlPoints[i + 1] == null
                        || controlPoints[i + 2] == null);
                };

                Matrix4x4 m = new Matrix4x4();
                m.SetRow(0, new Vector3(1f, -2f, 1f));
                m.SetRow(1, new Vector3(-2f, 2f, 0f));
                m.SetRow(2, new Vector3(1f, 0f, 0f));
                m = m.transpose;

                /*
                 * ベジェ曲線をつなぎ合わせただけでは節点がうまくつながらないため、
                 * 制御点と制御点の間を補間する点を追加しクッションのように挟むことによって
                 * 曲線の滑らかさを担保している
                 */
                SetupPoints = (i) => {
                    matrix.SetColumn(0, 0.5f * (controlPoints[i].transform.position + controlPoints[i + 1].transform.position));
                    matrix.SetColumn(1, controlPoints[i + 1].transform.position);
                    matrix.SetColumn(2, 0.5f * (controlPoints[i + 1].transform.position + controlPoints[i + 2].transform.position));
                    matrix *= m;
                };

                ParametricTransformWithGeometryMatrix = (t) => {
                    return matrix * new Vector3(t * t, t, 1);
                };
            }
            //３次Catmull Romスプラインの実装
            else if (curveType == CurveType.CatmllRom)
            {
                vertexCountDiff = 1;

                CheckBoundary = (i) => {
                    return !(controlPoints[i] == null
                        || controlPoints[i + 1] == null
                        || (i > 0 && controlPoints[i - 1] == null)
                        || (i < controlPoints.Length - 2 && controlPoints[i + 2] == null));
                };

                Matrix4x4 m = new Matrix4x4();
                m.SetRow(0, new Vector4(1f, 0f, 0f, 0f));
                m.SetRow(1, new Vector4(0f, 0f, 1f, 0f));
                m.SetRow(2, new Vector4(-3f, 3f, -2f, -1f));
                m.SetRow(3, new Vector4(2f, -2f, 1f, 1f));
                m = m.transpose;

                /*
                 * 基本的にはベジェスプラインと同じだが
                 * ジオメトリー行列の３番目・４番目の列には始点・終点での接線のベクトルを格納する
                 */
                SetupPoints = (i) => {
                    matrix.SetColumn(0, controlPoints[i].transform.position);
                    matrix.SetColumn(1, controlPoints[i + 1].transform.position);

                    if (i > 0)
                    {
                        matrix.SetColumn(2, 0.5f * (controlPoints[i + 1].transform.position - controlPoints[i - 1].transform.position));
                    }
                    else
                    {
                        matrix.SetColumn(2, controlPoints[i + 1].transform.position - controlPoints[i].transform.position);
                    }

                    if (i < controlPoints.Length - 2)
                    {
                        matrix.SetColumn(3, 0.5f * (controlPoints[i + 2].transform.position - controlPoints[i].transform.position));
                    }
                    else
                    {
                        matrix.SetColumn(3, controlPoints[i + 1].transform.position - controlPoints[i].transform.position);
                    }

                    matrix *= m;
                };

                ParametricTransformWithGeometryMatrix = (t) => {
                    return matrix * new Vector4(1, t, t * t, t * t * t);
                };
            }
            //３次Bスプラインの実装
            else if (curveType == CurveType.BSpline)
            {
                vertexCountDiff = 3;

                CheckBoundary = (i) => {
                    return !(controlPoints[i] == null
                        || controlPoints[i + 1] == null
                        || controlPoints[i + 2] == null
                        || controlPoints[i + 3] == null);
                };

                Matrix4x4 m = new Matrix4x4();
                m.SetRow(0, new Vector4(-1f, 3f, -3f, 1f) * 1 / 6);
                m.SetRow(1, new Vector4(3f, -6f, 3f, 0f) * 1 / 6);
                m.SetRow(2, new Vector4(-3f, 0f, 3f, 0f) * 1 / 6);
                m.SetRow(3, new Vector4(1f, 4f, 1f, 0f) * 1 / 6);
                m = m.transpose;

                SetupPoints = (i) => {
                    matrix.SetColumn(0, controlPoints[i].transform.position);
                    matrix.SetColumn(1, controlPoints[i + 1].transform.position);
                    matrix.SetColumn(2, controlPoints[i + 2].transform.position);
                    matrix.SetColumn(3, controlPoints[i + 3].transform.position);
                    matrix *= m;
                };

                ParametricTransformWithGeometryMatrix = (t) => {
                    return matrix * new Vector4(t * t * t, t * t, t, 1);
                };
            }
        }

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.positionCount = numberOfPoints * (controlPoints.Length - vertexCountDiff);

        for (int j = 0; j < controlPoints.Length - vertexCountDiff; j++)
        {
            if (!CheckBoundary(j))
            {
                return;
            }

            SetupPoints(j);

            float pointStep = j == controlPoints.Length - (vertexCountDiff + 1)
                ? 1.0f / (numberOfPoints - 1.0f) : 1.0f / numberOfPoints;

            //線の描画と球の追加/削除
            for (int i = 0; i < numberOfPoints; i++)
            {
                Vector3 position = ParametricTransformWithGeometryMatrix(i * pointStep);
                lineRenderer.SetPosition(i + j * numberOfPoints, position);
            }
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            List<GameObject> controlPointsNew = new List<GameObject>();
            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObj = hit.transform.gameObject;
                foreach (GameObject obj in controlPoints)
                {
                    bool used = true;
                    if (controlPoints.Length > 4 && obj.Equals(hitObj))
                    {
                        if (obj.Equals(controlPoints[controlPoints.Length - 1]))
                        {
                            if (controlPointsNew.Count > 0)
                            {
                                controlPointsNew.Add(controlPointsNew[controlPointsNew.Count - 1]);
                                Destroy(obj);
                            }

                            break;
                        }

                        if (!obj.Equals(controlPoints[0]))
                        {
                            used = false;
                            Destroy(obj);
                        }
                    }

                    if (used)
                    {
                        controlPointsNew.Add(obj);
                    }
                }

                if (controlPointsNew.Count != 0 && controlPointsNew.Count != controlPoints.Length)
                {
                    controlPoints = controlPointsNew.ToArray();
                }
            }
            else
            {
                GameObject sp = SpawnSphereAt(Input.mousePosition);
                foreach (GameObject obj in controlPoints)
                {
                    controlPointsNew.Add(obj);
                    if (controlPointsNew.Count == controlPoints.Length - 2)
                    {
                        controlPointsNew.Add(sp);
                    }
                }

                controlPoints = controlPointsNew.ToArray();
            }
        }
    }

    GameObject SpawnSphereAt(Vector3 mousePosition)
    {
        GameObject sp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0.0f));
        sp.transform.position = position;
        sp.name += sp.GetInstanceID().ToString();
        return sp;
    }
}