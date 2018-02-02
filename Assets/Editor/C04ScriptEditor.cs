using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(C04Script))]
public class C04ScriptEditor : Editor
{
    Matrix4x4 _matrix = new Matrix4x4();

    float determinant3x3;
    float determinant4x4;

    Vector4 rhs;
    Vector4 result;

    //Inspectorに表示する
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        //Hide Script property
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, new string[]{"m_Script"});
        serializedObject.ApplyModifiedProperties();

        C04Script obj = target as C04Script;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginVertical(GUI.skin.box);

        //matrixをInspectorにMatrixを表示する
        EditorGUILayout.LabelField(new GUIContent("Matrix4x4"));

        _matrix.SetRow(0, RowVector4Field(_matrix.GetRow(0)));
        _matrix.SetRow(1, RowVector4Field(_matrix.GetRow(1)));
        _matrix.SetRow(2, RowVector4Field(_matrix.GetRow(2)));
        _matrix.SetRow(3, RowVector4Field(_matrix.GetRow(3)));

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);

        determinant3x3 = EditorGUILayout.FloatField("Determinant (3x3)", determinant3x3);
        determinant4x4 = EditorGUILayout.FloatField("Determinant (4x4)", determinant4x4);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(GUI.skin.box);
        rhs = EditorGUILayout.Vector4Field("RHS", rhs);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        if(GUILayout.Button("operator *"))
        {
            result = _matrix * rhs;
        }
        if(GUILayout.Button("MultiplyPoint"))
        {
            result = _matrix.MultiplyPoint(rhs);
        }
        if(GUILayout.Button("MultiplyPoint3x4"))
        {
            result = _matrix.MultiplyPoint3x4(rhs);
        }
        if(GUILayout.Button("MultiplyVector"))
        {
            result = _matrix.MultiplyVector(rhs);
        }

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.Vector4Field("Result", result);
        EditorGUILayout.EndVertical();

        if(EditorGUI.EndChangeCheck())
        {
            determinant3x3 = getDeterminant3x3(_matrix);
            determinant4x4 = getDeterminant4x4(_matrix);

            Undo.RecordObject(target, "Chapter04EditorUndo");
            EditorUtility.SetDirty(target);
        }
    }

    public float getDeterminant3x3(Matrix4x4 m)
    {
        return m.m00 * m.m11 * m.m22 - m.m00 * m.m12 * m.m21 - m.m01 * m.m10 * m.m22
             + m.m01 * m.m12 * m.m20 + m.m02 * m.m10 * m.m21 - m.m02 * m.m11 * m.m20 ;
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
             - m.m00 * m.m12 * m.m21 * m.m33 - m.m01 * m.m10 * m.m22 * m.m33 + m.m00 * m.m11 * m.m22 * m.m33 ;
    }

    public static Vector4 RowVector4Field(Vector4 value, params GUILayoutOption[] options)
    {
        Rect position = EditorGUILayout.GetControlRect(true,
                                                       16.0f,
                                                       EditorStyles.numberField,
                                                       options);

        float[] values = new float[] { value.x, value.y, value.z, value.w };

        EditorGUI.BeginChangeCheck();

        EditorGUI.MultiFloatField(position,
                                  new GUIContent[]{new GUIContent(), new GUIContent(), new GUIContent(), new GUIContent()},
                                  values);

        if(EditorGUI.EndChangeCheck())
        {
            value.Set(values[0], values[1], values[2], values[3]);
        }

        return value;
    }

}
