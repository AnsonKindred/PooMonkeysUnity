using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VectorField : MonoBehaviour {
	
	int MAX_SEED_SPOTS = 20;
	
	float width;
	float height;
	
	List<Vector3> seedSpots = new List<Vector3>();
	
	void Awake()
	{
		Debug.Log("Init vector field");
		int numSeedSpots = (int) (Random.value*(MAX_SEED_SPOTS+1));
		for(int i = 0; i < numSeedSpots; i++)
		{
			seedSpots.Add(new Vector3(Random.value*width, Random.value*height, Random.value*height*.15f));
		}
	}
	
	public float getXForce(float x, float y)
	{
//		float force = 0;
//		int closeSpots = 0;
//		for(int i = 0; i < seedSpots.Count; i++)
//		{
//			float d = Mathf.Sqrt(Mathf.Pow(seedSpots[i].y - y, 2) + Mathf.Pow(seedSpots[i].x - x, 2))/seedSpots[i].z;
//			if(d <= Mathf.PI/2)
//			{
//				if(i%2 == 0)
//				{
//					force += Mathf.Sin((seedSpots[i].y - y)/seedSpots[i].z);
//				}
//				else
//				{
//					force += Mathf.Sin((y - seedSpots[i].y)/seedSpots[i].z);
//				}
//				closeSpots++;
//			}
//		}
//		
//		if(closeSpots == 0)
//		{
//			return 0;
//		}
//		
//		force /= closeSpots;
//		return force;
		
		return Mathf.Sin((y + Time.fixedTime)*Time.fixedTime*.05f);
	}
	
	public float getYForce(float x, float y)
	{
//		float force = 0;
//		int closeSpots = 0;
//		for(var i = 0; i < seedSpots.Count; i++)
//		{
//			float d = Mathf.Sqrt(Mathf.Pow(seedSpots[i].y - y, 2) + Mathf.Pow(seedSpots[i].x - x, 2))/seedSpots[i].z;
//			if(d <= Mathf.PI/2)
//			{
//				if(i%2 == 0)
//				{
//					force += Mathf.Cos((seedSpots[i].x - x)/seedSpots[i].z + Mathf.PI/2.0f);
//				}
//				else
//				{
//					force += Mathf.Cos((x-seedSpots[i].x)/seedSpots[i].z + Mathf.PI/2.0f);
//				}
//				closeSpots++;
//			}
//		}
//		
//		if(closeSpots == 0)
//		{
//			return 0;
//		}
//		
//		force /= closeSpots;
//		return force;
		
		return Mathf.Sin((x + Time.fixedTime)*Time.fixedTime*.05f);
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
