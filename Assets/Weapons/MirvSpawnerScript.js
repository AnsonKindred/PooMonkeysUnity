#pragma strict

var MirvClone : GameObject;
var MirvBabyClone : GameObject;


var Mirv: GameObject;

var counter: int;

var spawnPosition: Vector3;
var spawnAngle: Quaternion;

function Start() {
	spawnPosition = transform.position;
	spawnAngle = transform.rotation;
}

function Update () {

}

function FixedUpdate () 
{
	if (Mirv.rigidbody.velocity.y < 0)
	{
		var MirvBaby = Instantiate(MirvBabyClone, Mirv.rigidbody.position + Vector3(2.0,0.0,0.0), Quaternion.LookRotation(Vector3(0.0, Mathf.Sin(MonkeyController.angle), Mathf.PI / 2),Vector3.up));
		var MirvBaby1 = Instantiate(MirvBabyClone, Mirv.rigidbody.position + Vector3(-1.0,0.0,0.0), Quaternion.LookRotation(Vector3(0.0, Mathf.Sin(MonkeyController.angle), Mathf.PI / 2),Vector3.up));
	}
}

function Fire(position: Vector3, angle: float, power: float)
{
	Mirv = Instantiate(MirvClone, position + Vector3 (Mathf.Cos(angle) * MirvClone.gameObject.rigidbody.transform.localScale.y, Mathf.Sin(angle) * MirvClone.gameObject.rigidbody.transform.localScale.y, 0.0), Quaternion.identity);
	Mirv.rigidbody.AddForce(Vector3 (Mathf.Cos(angle)*power, Mathf.Sin(angle)*power, 0.0));
}

function OnTriggerEnter(other: Collider) 
{
//		Explode(other.transform.position);
	//Destroy(other.gameObject);
	Debug.Log("yep");
}