using UnityEngine;
using System.Collections;

public class VectorFieldGrid : MonoBehaviour {
	
	public GameObject o;
	
	public float rows;
	public float cols;
	public float spacing;
	public GameObject vectorFieldObject;
	
	float width;
	float height;
	
	// Use this for initialization
	void Start () 
	{
		width = cols*spacing;
		height = rows*spacing;
		for(var i = 0; i <= rows; i++)
		{
			for(var j = 0; j <= cols; j++)
			{
				Vector3 pos = new Vector3(j*spacing , i*spacing, 0f);
				GameObject instance = Instantiate(o, pos, Quaternion.identity) as GameObject;
				Vector script = instance.GetComponent("MonoBehaviour") as Vector;
				script.maxLength = spacing;
				script.vectorField = vectorFieldObject.GetComponent("MonoBehaviour") as VectorField;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
