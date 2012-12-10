#pragma strict

var spawnPosition: Vector3;
var spawnAngle: Quaternion;
var PooChainClone1: GameObject;
var PooChainLinkClone: GameObject;
var pooChainClone44: GameObject;
var counter: int = 0;
function Start () {
	spawnPosition = transform.position;
	spawnAngle = transform.rotation;
}

function Update () {

}

function FixedUpdate () {
	if (counter == 0) {
		var pooChainClone44 = Instantiate(PooChainLinkClone, spawnPosition, spawnAngle);
		pooChainClone44.rigidbody.AddForce(1000,0.0,0.0);
		PooChainClone1 = pooChainClone44;
		counter++;
	}
	if (counter > 0 && Mathf.Abs(PooChainClone1.gameObject.rigidbody.transform.position.x - spawnPosition.x) > PooChainClone1.gameObject.rigidbody.transform.localScale.y * 2 && counter < 10) {

		var pooChainClone = Instantiate(PooChainLinkClone, spawnPosition, spawnAngle);
		pooChainClone.rigidbody.AddForce(1000,500,0.0);		
		pooChainClone.gameObject.AddComponent("HingeJoint");
		pooChainClone.hingeJoint.anchor = Vector3 (1, 0, 0);
		pooChainClone.hingeJoint.axis = Vector3.right;
		pooChainClone.hingeJoint.connectedBody = PooChainClone1.GetComponent(Rigidbody);
		PooChainClone1 = pooChainClone;
		counter++;
	}
}
