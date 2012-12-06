#pragma strict

class Vector extends MonoBehaviour
{
	var maxLength: float;
	var vectorField: VectorField;
	
	function LateUpdate() 
	{
		var xForce: float = vectorField.getXForce(transform.position.x, transform.position.y);
		var yForce: float = vectorField.getYForce(transform.position.x, transform.position.y);
		var forceVector: Vector3 = new Vector3(xForce, yForce, 0.0);
	    var magnitude: float = forceVector.sqrMagnitude;
		var myTrans: Transform[] = GetComponentsInChildren.<Transform>() as Transform[];
	
	    for (var child : Transform in myTrans) 
	    {
	    	if(child.name == "Cylinder")
	    	{
				child.localScale = new Vector3(.5, magnitude*maxLength, .5);
				child.localPosition.z = magnitude*maxLength+.5;
			}
	    }
	    
		if(magnitude > 0)
		{
			transform.rotation = Quaternion.LookRotation(forceVector, Vector3.up);
		}
	}
}