using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CustomEditor(typeof(C05Script))]
public class C05ScriptEditor : Editor
{
    Matrix4x4 matrix = Matrix4x4.identity;
    Matrix4x4 projectMatrix = Matrix4x4.identity;

    float determinant3x3;
    float determinant4x4;

    Vector4 rhs;
    Vector4 result;

    Vector3 translation;
    Vector3 rotation;
    Vector3 scale = Vector3.one;

    //3Dモデルの頂点情報
    private MeshFilter mf;

    //実際の頂点情報を格納する配列
    private Vector3[] origVerts;
    private Vector3[] newVerts;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, new string[] { "MyScript" });
        serializedObject.ApplyModifiedProperties();

        C05Script obj = target as C05Script;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField(new GUIContent("Model Transform"));

        matrix.SetRow(0, RowVector4Field(matrix.GetRow(0)));
        matrix.SetRow(1, RowVector4Field(matrix.GetRow(1)));
        matrix.SetRow(2, RowVector4Field(matrix.GetRow(2)));
        matrix.SetRow(3, RowVector4Field(matrix.GetRow(3)));

        if(GUILayout.Button("Reset"))
        {
            matrix = Matrix4x4.identity;
        }

        /*
         * Model Transformのボタンを押した時の挙動
         * matrix変数に入っている行列を座標変換としてorigVertsに
         * 入っている頂点群に順に適用して新しい頂点群を生成し、
         * 元のポリゴンメッシュに格納する
         */
        if (GUILayout.Button("Apply"))
        {
            mf = obj._gameObject.GetComponent<MeshFilter>();
            origVerts = mf.mesh.vertices;
            newVerts = new Vector3[origVerts.Length];

            int i = 0;
            while(i < origVerts.Length)
            {
                newVerts[i] = matrix.MultiplyPoint3x4(origVerts[i]);
                i++;
            }

            mf.mesh.vertices = newVerts;
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);

        determinant3x3 = EditorGUILayout.FloatField("Determinant (3*3)", determinant3x3);
        determinant4x4 = EditorGUILayout.FloatField("Determinant (4*4)", determinant4x4);

        EditorGUILayout.Space();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);

        /*
         * 平行移動行列の作成する
         * setTRSメソッドを用いて行う
         * 平行移動(T)、回転(R)、スケール(S)の各座標変換行列を一気にMatrix4x4に設定する。
         * 今回は平行移動のみで、回転とスケールに関しては無変換に相当する値を設定する。
         */
        translation = EditorGUILayout.Vector3Field("Translation", translation);

        if(GUILayout.Button("Apply"))
        {
            Matrix4x4 m = Matrix4x4.identity;
            m.SetTRS(translation, Quaternion.identity, Vector3.one);
            matrix = m * matrix;
        }

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);

        /*
         * 回転、スケールについても同様に、他の2つのアフィン変換に何もしない変換を設定する
         * 個別にSetTRSで作成する
         */
        rotation = EditorGUILayout.Vector3Field("Rotation", rotation);

        if(GUILayout.Button("Apply"))
        {
            Matrix4x4 m = Matrix4x4.identity;
            m.SetTRS(Vector3.zero, Quaternion.Euler(rotation.x, rotation.y, rotation.z), Vector3.zero);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);

        scale = EditorGUILayout.Vector3Field("Scale", scale);

        if(GUILayout.Button("Apply"))
        {
            Matrix4x4 m = Matrix4x4.identity;
            m.SetTRS(Vector3.zero, Quaternion.identity, scale);
            matrix = m * matrix;
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.LabelField(new GUIContent("Projection Transform"));

        if(GUILayout.Button("Perspective"))
        {
            Camera.main.orthographic = false;
        }

        if (GUILayout.Button("Orthographic"))
        {
            Camera.main.orthographic = true;
        }

        projectMatrix.SetRow(0, RowVector4Field(projectMatrix.GetRow(0)));
        projectMatrix.SetRow(1, RowVector4Field(projectMatrix.GetRow(1)));
        projectMatrix.SetRow(2, RowVector4Field(projectMatrix.GetRow(2)));
        projectMatrix.SetRow(3, RowVector4Field(projectMatrix.GetRow(3)));

        if (GUILayout.Button("Camera.main.projectionMatrix"))
        {
            string graphicsDeviceType = SystemInfo.graphicsDeviceType.ToString();
            Debug.Log(graphicsDeviceType);

            bool dx = graphicsDeviceType.IndexOf("Direct3D") == 0;

            int dxVersion = 11;
            if (dx)
            {
                System.Int32.TryParse(graphicsDeviceType.Substring("Direct3D".Length), out dxVersion);
                Debug.Log("Direct3D version : " + dxVersion);
            }

            Matrix4x4 pm = Camera.main.projectionMatrix;

            if (dx)
            {
                if(dxVersion <  11)
                {
                    for(int i = 0; i < 4; i++)
                    {
                        pm[1, i] = -pm[1, i];
                    }

                    for(int i = 0; i < 4; i++)
                    {
                        pm[2, i] = pm[2, i] * 0.5f + pm[3, i] * 0.5f;
                    }
                }
                else
                {
                    for(int i = 0; i < 4; i++)
                    {
                        pm[1, i] = -pm[1, i];
                    }

                    for(int i = 0; i < 4; i++)
                    {
                        pm[2, i] = pm[2, i] * -0.5f + pm[3, i] * 0.5f;
                    }
                }
            }

            projectMatrix = pm;

            Matrix4x4 gpuProjectionMatrix = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, true);
            if(pm == gpuProjectionMatrix)
            {
                Debug.Log("Camera.main.projectionMatrix matches with GL.GetGPUProjectionMatrix");
            }
            else
            {
                Debug.Log("Camera.main.projectionMatrix doesn't match with GL.GetGPUProjectionMatrix");
            }
        }

        /*
         * DirextXの場合はOpenGLに合うように変換行列に補正を加える。
         * y軸の符号反転とz軸範囲の[-1, 1]から[0, 1]への写像しなおしをする
         */
        if (GUILayout.Button("GL.GetGPUProjectionMatrix"))
        {
            projectMatrix = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, true);
        }

        if (GUILayout.Button("Reset"))
        {
            projectMatrix = Matrix4x4.identity;
            Camera.main.ResetProjectionMatrix();
        }

        if (GUILayout.Button("Set"))
        {
            Camera.main.projectionMatrix = projectMatrix;
        }

        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            determinant3x3 = getDeterminant3x3(matrix);
            determinant4x4 = getDeterminant4x4(matrix);

            Undo.RecordObject(target, "Chapter05EditorUndo");
            EditorUtility.SetDirty(target);
        }
    }

    public float getDeterminant3x3(Matrix4x4 m)
    {
        return m.m00 * m.m11 * m.m22 - m.m00 * m.m12 * m.m21 - m.m01 * m.m10 * m.m22
             + m.m01 * m.m12 * m.m20 + m.m02 * m.m10 * m.m21 - m.m02 * m.m11 * m.m20;
    }

    public float getDeterminant4x4(Matrix4x4 m)
    {
        return m.m03 * m.m12 * m.m21 * m.m30 - m.m02 * m.m13 * m.m21 * m.m30 - m.m03 * m.m11 * m.m22 * m.m30
             + m.m01 * m.m13 * m.m22 * m.m30 + m.m02 * m.m11 * m.m23 * m.m30 - m.m01 * m.m12 * m.m23 * m.m30
             - m.m03 * m.m12 * m.m20 * m.m31 + m.m02 * m.m13 * m.m20 * m.m31 + m.m03 * m.m10 * m.m22 * m.m31
             - m.m00 * m.m13 * m.m22 * m.m31 - m.m02 * m.m10 * m.m23 * m.m31 + m.m00 * m.m12 * m.m23 * m.m31
             + m.m03 * m.m11 * m.m20 * m.m32 - m.m01 * m.m13 * m.m20 * m.m32 - m.m03 * m.m10 * m.m21 * m.m32
             + m.m00 * m.m13 * m.m21 * m.m32 + m.m01 * m.m10 * m.m23 * m.m32 - m.m00 * m.m11 * m.m23 * m.m32
             - m.m02 * m.m11 * m.m20 * m.m33 + m.m01 * m.m12 * m.m20 * m.m33 + m.m02 * m.m10 * m.m21 * m.m33
             - m.m00 * m.m12 * m.m21 * m.m33 - m.m01 * m.m10 * m.m22 * m.m33 + m.m00 * m.m11 * m.m22 * m.m33;
    }

    public static Vector4 RowVector4Field(Vector4 value, params GUILayoutOption[] options)
    {
        Rect position = EditorGUILayout.GetControlRect(true, 16.0f, EditorStyles.numberField, options);
        float[] values = new float[] { value.x, value.y, value.z, value.w };

        EditorGUI.BeginChangeCheck();

        EditorGUI.MultiFloatField(position,
                                  new GUIContent[] { new GUIContent(), new GUIContent(), new GUIContent(), new GUIContent()},
                                  values);

        if(EditorGUI.EndChangeCheck())
        {
            value.Set(values[0], values[1], values[2], values[3]);
        }

        return value;
    }
}