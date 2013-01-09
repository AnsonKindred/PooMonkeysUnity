//using UnityEngine;
//using System.Collections;
//
//public class MirvSpawner : MonoBehaviour 
//{
//
//	public Transform MirvClone;
//	
//	public Transform Mirv;
//	
//	int counter;
//	
//	Vector3 spawnPosition = new Vector3();
//	Quaternion spawnAngle;
//	
//	void Start() {
//		spawnPosition = transform.position;
//		spawnAngle = transform.rotation;
//	}
//	
//	void Update () {
//	
//	}
//	
//	void FixedUpdate () 
//	{
//		
//		if (counter == 0) {
//			Mirv = Instantiate(MirvClone, spawnPosition, Quaternion.LookRotation(Vector3(0.0, Mathf.Sin(MonkeyController.angle), Mathf.PI / 2),Vector3.up)) as Transform;
//			Mirv.rigidbody.AddForce(Mathf.Cos(MonkeyController.angle) * MonkeyController.power, Mathf.Sin(MonkeyController.angle) * MonkeyController.power, 0.0);
//			counter++;
//		}
//		if (Mirv.rigidbody.velocity.y < 0 && counter == 1) {
//			//Mirv.rigidbody.velocity.x = 0.0;
//			GameObject MirvBaby = Instantiate(MirvClone, Mirv.rigidbody.position + Vector3(2.0,0.0,0.0), Quaternion.LookRotation(Vector3(0.0, Mathf.Sin(MonkeyController.angle), Mathf.PI / 2),Vector3.up)) as Transform;
//			GameObject MirvBaby1 = Instantiate(MirvClone, Mirv.rigidbody.position + Vector3(-1.0,0.0,0.0), Quaternion.LookRotation(Vector3(0.0, Mathf.Sin(MonkeyController.angle), Mathf.PI / 2),Vector3.up)) as Transform;
//			counter++;
//		}
//	}
//}
