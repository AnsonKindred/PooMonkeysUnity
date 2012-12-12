#pragma strict

var DrillerClone : GameObject;

var spawnPosition: Vector3;
var spawnAngle: Quaternion;

var counter: int;

function Start () {
	spawnPosition = transform.position;
	spawnAngle = transform.rotation;
}

function Update () {

}

function FixedUpdate () {
if (counter == 0) {
	var Driller = Instantiate(DrillerClone, spawnPosition, Quaternion.LookRotation(Vector3(Mathf.Cos(MonkeyController.angle - Mathf.PI/2),Mathf.Sin(MonkeyController.angle - Mathf.PI/2),0.0),Vector3.right));
		Driller.rigidbody.AddForce(Mathf.Cos(MonkeyController.angle) * MonkeyController.power, Mathf.Sin(MonkeyController.angle) * MonkeyController.power, 0.0);
		counter++;
	}
}

