#pragma strict

function Start () {

}

function Update () {
	var mousePosition: Vector3 = Camera.mainCamera.ScreenToWorldPoint(Input.mousePosition);
	this.rigidbody.transform.position.x = mousePosition.x;
	this.rigidbody.transform.position.y = mousePosition.y;
	
	//this.rigidbody.transform.position = Input.mousePosition;
}