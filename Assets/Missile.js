#pragma strict
//
//var myObject : GameObject;
//var objectScript : myScript;
//function Start()
//{
//    objectScript=myObject.GetComponent(myScript);
//    objectScript.doStuff();
//}

//var controller : GameObject;
var counter : int;
//var other : MonkeyController;
//var other: MonkeyController = GetComponent(MonkeyController); 

function Start() {
   //other = controller.GetComponent("MonkeyController"); 
}

function Update () {

}

function FixedUpdate () {
	if (counter > 50 && counter < 100) {
	this.gameObject.rigidbody.AddForce(Mathf.Cos(MonkeyController.angle)* 40.0, Mathf.Sin(MonkeyController.angle) * 40.0, 0.0);
	}
	counter++;
}

function SecondsToNumberOfFixedUpdates () {
	
}