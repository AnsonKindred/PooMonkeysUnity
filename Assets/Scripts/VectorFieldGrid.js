#pragma strict

class VectorFieldGrid extends MonoBehaviour
{
	var object: GameObject;
	
	var rows: float;
	var cols: float;
	var spacing: float;
	var vectorFieldObject: GameObject;
	
	private var width: float;
	private var height: float;

	function Start() 
	{
		width = cols*spacing;
		height = rows*spacing;
		for(var i = 0; i <= rows; i++)
		{
			for(var j = 0; j <= cols; j++)
			{
				var pos: Vector3 = new Vector3(j*spacing , i*spacing, 0);
				var instance: GameObject = Instantiate(object, pos, Quaternion.identity);
				var script: Vector = instance.GetComponent(MonoBehaviour);
				script.maxLength = spacing;
				script.vectorField = vectorFieldObject.GetComponent(MonoBehaviour);
			}
		}
	}
}
