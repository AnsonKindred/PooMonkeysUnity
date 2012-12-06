#pragma strict

function Awake() 
{
	var mesh = new Mesh();
		
	var vertices: Vector3[] = new Vector3[4];
	vertices[0] = new Vector3(-1/2.0, -1/2.0, 0);
	vertices[1] = new Vector3(-1/2.0, 1/2.0, 0);
	vertices[2] = new Vector3(1/2.0, 1/2.0, 0);
	vertices[3] = new Vector3(1/2.0, -1/2.0, 0);

	mesh.vertices = vertices;
	
	var textureCoords: Vector2[] = new Vector2[4];
	textureCoords[0] = new Vector2(0, 0);
	textureCoords[1] = new Vector2(0, 1);
	textureCoords[2] = new Vector2(1, 1);
	textureCoords[3] = new Vector2(1, 0);
	
	mesh.uv = textureCoords;
		
	var tri: int[] = new int[6];

	tri[0] = 0;
	tri[1] = 1;
	tri[2] = 3;

	tri[3] = 3;
	tri[4] = 1;
	tri[5] = 2;
	
	mesh.triangles = tri;
	
	mesh.Optimize();
	
	var mf: MeshFilter = GetComponent(MeshFilter);
	mf.mesh = mesh;
}

function Update () 
{

}