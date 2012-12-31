using UnityEngine;
using System.Collections;

public class Vector : MonoBehaviour {
	
	public float maxLength;
	public VectorField vectorField;
	
	void LateUpdate()
	{
		float xForce = vectorField.getXForce(transform.position.x, transform.position.y);
		float yForce = vectorField.getYForce(transform.position.x, transform.position.y);
		Vector3 forceVector = new Vector3(xForce, yForce, 0.0f);
	    float magnitude = forceVector.sqrMagnitude;
		Transform[] myTrans = GetComponentsInChildren<Transform>() as Transform[];
	
	    foreach (Transform child in myTrans)
	    {
	    	if(child.name == "Cylinder")
	    	{
				child.localScale = new Vector3(.5f, magnitude * maxLength, .5f);
				child.localPosition = new Vector3(0.0f, 0.0f, magnitude * maxLength + .5f);
			}
	    }
	    
		if(magnitude > 0)
		{
			transform.rotation = Quaternion.LookRotation(forceVector, Vector3.up);
		}
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
