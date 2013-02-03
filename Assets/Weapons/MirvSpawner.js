#pragma strict

var MirvClone : GameObject;

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

function FixedUpdate () {
	

	
	if (counter == 0) {
		Mirv = Instantiate(MirvClone, spawnPosition, Quaternion.LookRotation(Vector3(0.0, Mathf.Sin(MonkeyController.angle), Mathf.PI / 2),Vector3.up));
		Mirv.rigidbody.AddForce(Mathf.Cos(MonkeyController.angle) * MonkeyController.power, Mathf.Sin(MonkeyController.angle) * MonkeyController.power, 0.0);
		counter++;
	}
	if (Mirv.rigidbody.velocity.y < 0 && counter == 1) {
		//Mirv.rigidbody.velocity.x = 0.0;
		var MirvBaby = Instantiate(MirvClone, Mirv.rigidbody.position + Vector3(2.0,0.0,0.0), Quaternion.LookRotation(Vector3(0.0, Mathf.Sin(MonkeyController.angle), Mathf.PI / 2),Vector3.up));
		var MirvBaby1 = Instantiate(MirvClone, Mirv.rigidbody.position + Vector3(-1.0,0.0,0.0), Quaternion.LookRotation(Vector3(0.0, Mathf.Sin(MonkeyController.angle), Mathf.PI / 2),Vector3.up));
		counter++;
	}
		if (MirvBaby == null && MirvBaby1 == null && Mirv == null)
	{
		Destroy(this.gameObject);
	}
}

function SecondsToNumberOfFixedUpdates () {
	
}

	function OnTriggerEnter(other: Collider) 
	{
//		Explode(other.transform.position);
		//Destroy(other.gameObject);
		Debug.Log("yep");
    }