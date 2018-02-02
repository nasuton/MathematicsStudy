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

	void Start () {
		
	}

	void Update () {
		
	}
}
