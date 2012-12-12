#pragma strict

var IceCuboidClone : GameObject;

var counter: int;

var spawnPosition: Vector3;
var spawnAngle: Quaternion;

function Start () {
	spawnPosition = transform.position;
	spawnAngle = transform.rotation;
}

function Update () {

}

function FixedUpdate() {

	if (counter == 0) {
	var IceCuboid = Instantiate(IceCuboidClone, spawnPosition, Quaternion.LookRotation(Vector3(0.0, Mathf.Sin(MonkeyController.angle), Mathf.PI / 2),Vector3.up));
	counter++;
	}
}