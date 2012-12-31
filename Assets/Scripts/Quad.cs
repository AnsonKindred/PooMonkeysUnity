using UnityEngine;
using System.Collections;

[ExecuteInEditMode]

public class Quad : MonoBehaviour {
	
void Awake() 
{
	var mesh = new Mesh();
		
	Vector3[] vertices = new Vector3[4];
	vertices[0] = new Vector3(-1/2.0f, -1/2.0f, 0.0f);
	vertices[1] = new Vector3(-1/2.0f, 1/2.0f, 0.0f);
	vertices[2] = new Vector3(1/2.0f, 1/2.0f, 0.0f);
	vertices[3] = new Vector3(1/2.0f, -1/2.0f, 0.0f);

	mesh.vertices = vertices;
	
	Vector2[] textureCoords = new Vector2[4];
	textureCoords[0] = new Vector2(0, 0);
	textureCoords[1] = new Vector2(0, 1);
	textureCoords[2] = new Vector2(1, 1);
	textureCoords[3] = new Vector2(1, 0);
	
	mesh.uv = textureCoords;
		
	int[] tri = new int[6];

	tri[0] = 0;
	tri[1] = 1;
	tri[2] = 3;

	tri[3] = 3;
	tri[4] = 1;
	tri[5] = 2;
	
	mesh.triangles = tri;
	
	mesh.Optimize();
	mesh.RecalculateNormals();
	MeshFilter mf = GetComponent("MeshFilter") as MeshFilter;
	mf.mesh = mesh;
}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
