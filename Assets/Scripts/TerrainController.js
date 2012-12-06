@script ExecuteInEditMode()
import System.IO;

class TerrainController extends MonoBehaviour
{
	private var THICKNESS: float = .1;
	private var segmentWidth: float;
	public var numSegments: int;
	public var width: float;
	public var height: float;
	
	public var warpTime: int;
	public var warpScale: float;
	
	public var points: List.<Vector2> = new List.<Vector2>();
	public var segments: GameObject[];
	
	var vectorFieldObject: GameObject;
	
	var doneGenerating: boolean = false;
	
	public function Start()
	{
	 	segmentWidth = width/numSegments;
	 	var success: boolean = false;
	 	var count: int = 0;
	 	while(success == false && count < 10)
	 	{
       		if(count > 0) Debug.Log("Terrain generation failed, trying again");
			_generate();
			success = buildTerrainMesh();
			count++;
		}
	}
	
	public function buildTerrainMesh()
	{
		// Use the triangulator to get indices for creating triangles
        var indices: int[] = Triangulator.Triangulate(points);
        
        if(indices == null) return false;
        
        // Create the Vector3 vertices
        var vertices: Vector3[] = new Vector3[points.Count];
        var normals: Vector3[] = new Vector3[points.Count];
		var textureCoords: Vector2[] = new Vector2[points.Count];
		
        for (var i = 0; i < vertices.Length; i++) 
        {
            vertices[i] = new Vector3(points[i].x, points[i].y, 0);
            normals[i] = Vector3.forward;
			textureCoords[i] = new Vector2(0, 0);
        }
	
		var mf: MeshFilter = GetComponent(MeshFilter);
		
		var mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = indices;
		mesh.normals = normals;
		mesh.uv = textureCoords;
		mesh.Optimize();
	    mesh.RecalculateBounds();
	    
	    mf.mesh = mesh;
	    
	    return true;
	}

	private function _generate() 
	{
		points.Clear();
		points.Capacity = numSegments+1+2; // segments + 2 bottom points to create a polygon
		
		for(var i:int = 0; i < points.Capacity; i++)
		{
			points.Add(new Vector2(0, 0));
		}
		
		points[0] = new Vector2(0, 0);
		points[1] = new Vector2(0, Random.value*height);
		
		points[numSegments+1] = new Vector2(numSegments*segmentWidth, Random.value*height);
		points[numSegments+2] = new Vector2(numSegments*segmentWidth, 0);
		
		_midPointDivision(1, numSegments+1, 0);
		
		for(i = 2; i < points.Count-3; i++)
		{
			var lastSlope: float = (points[i].y - points[i-1].y)/(points[i].x - points[i-1].x);
			var newSlope: float = (points[i+1].y - points[i].y)/(points[i+1].x - points[i].x);
		}
		
		var vectorField: VectorField = vectorFieldObject.GetComponent(MonoBehaviour);
		
		for(var t: int = 0; t < warpTime; t++)
		{
			for(i=2; i < points.Count-2; i++)
			{
				var xForce: float = vectorField.getXForce(points[i].x, points[i].y);
				var yForce: float = vectorField.getYForce(points[i].x, points[i].y);
				
				var nextX: float = Mathf.Clamp(points[i].x + xForce*warpScale, 0, width);
				var nextY: float = Mathf.Clamp(points[i].y + yForce*warpScale, 1, height);
				
				points[i] = new Vector2(nextX, nextY);
				
				if(i != 2)
				{
					var distance: float = Vector2.Distance(points[i], points[i-1]);
					if(distance > segmentWidth*2)
					{
						points.Insert(i, points[i-1] + (points[i]-points[i-1])*.5);
						i--;
					}
				}
			}
		}
	}
	
	private function _midPointDivision(start: int, end: int, depth: int) 
	{
		if (end <= start+1) 
		{
			return;
		}
		
		var midPoint: int = (end + start) / 2;
		var midY: float = (points[start].y + points[end].y) / 2;
		var depthFactor: float = .5 / (depth*depth);
		
		var y: float = midY + (Random.value * 2 * depthFactor - depthFactor)*height;
		
		// Make sure y is between 0 and height
		y = Mathf.Max(Mathf.Min(height, y), 1); 
		
		points[midPoint] = new Vector2((midPoint-1)*segmentWidth, y);
		
		_midPointDivision(start, midPoint, depth + 1);
		_midPointDivision(midPoint, end, depth + 1);
	}

}
