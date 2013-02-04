#pragma strict

var spawnPosition: Vector3;
var spawnAngle: Quaternion;
var PooChainClonePrevious: GameObject;
var PooChainLinkClone: GameObject;
var PooChainCloneHead: GameObject;
var counter: int = 0;

function Start () {
	spawnPosition = transform.position;
	spawnAngle = transform.rotation;
}

function Update () {

}

function FixedUpdate () {
//	if (counter == 0) {
//		PooChainCloneHead = Instantiate(PooChainLinkClone, spawnPosition, Quaternion.LookRotation(Vector3(Mathf.Cos(MonkeyController.angle - Mathf.PI/2),Mathf.Sin(MonkeyController.angle - Mathf.PI/2),0.0),Vector3.right));
//		PooChainCloneHead.rigidbody.AddForce(Mathf.Cos(MonkeyController.angle) * MonkeyController.power, Mathf.Sin(MonkeyController.angle) * MonkeyController.power, 0.0);
//		PooChainClonePrevious = PooChainCloneHead;
//		counter++;
//	}
	var dist = Vector3.Distance(PooChainClonePrevious.gameObject.rigidbody.transform.position, spawnPosition);
	if (counter > 0 && counter < 10 && dist > PooChainClonePrevious.gameObject.rigidbody.transform.localScale.y * 2) {

		var PooChainClone = Instantiate(PooChainLinkClone, spawnPosition, Quaternion.LookRotation(Vector3(Mathf.Cos(MonkeyController.angle - Mathf.PI/2),Mathf.Sin(MonkeyController.angle - Mathf.PI/2),0.0),Vector3.right));
		PooChainClone.rigidbody.AddForce(Mathf.Cos(MonkeyController.angle) * MonkeyController.power * (1 + counter/10) , Mathf.Sin(MonkeyController.angle) * MonkeyController.power * (1 + counter / 10), 0.0);	
		PooChainClone.gameObject.AddComponent("HingeJoint");
		PooChainClone.hingeJoint.breakForce = 1;
		var normalizedVector: Vector3 = Vector3 (Mathf.Cos(MonkeyController.angle), Mathf.Sin(MonkeyController.angle), 0.0);
		normalizedVector.Normalize();
		PooChainClone.hingeJoint.anchor =  Vector3(0.0,dist,0.0);
		PooChainClone.hingeJoint.axis = Vector3 (1, 0, 0);
		PooChainClone.hingeJoint.connectedBody = PooChainClonePrevious.GetComponent(Rigidbody);
		PooChainClonePrevious = PooChainClone;
		counter++;
	}
}

function Fire(position: Vector3, angle: float, power: float)
{
	PooChainCloneHead = Instantiate(PooChainLinkClone, spawnPosition, Quaternion.LookRotation(Vector3(Mathf.Cos(MonkeyController.angle - Mathf.PI/2),Mathf.Sin(MonkeyController.angle - Mathf.PI/2),0.0),Vector3.right));
	PooChainCloneHead.rigidbody.AddForce(Mathf.Cos(MonkeyController.angle) * MonkeyController.power, Mathf.Sin(MonkeyController.angle) * MonkeyController.power, 0.0);
	PooChainClonePrevious = PooChainCloneHead;
	counter++;
}
