#pragma strict

var counter: int;

function Start () {

}

function Update () {

}

function OnTriggerEnter (collider : Collider) {
	if (collider.gameObject.name == "Terrain") {
        this.rigidbody.useGravity = false;
        this.rigidbody.velocity = this.rigidbody.velocity / 3;
        this.rigidbody.transform.position = this.rigidbody.transform.position + Vector3(0.0,0.0,-5.0);
       	yield WaitForSeconds(3);
        Destroy(this.gameObject);
    }
}

function OnTriggerExit (collider : Collider) {	
	if (collider.gameObject.name == "Terrain" && counter == 1) {
		Destroy(this.gameObject);
	}
	if (collider.gameObject.name == "Terrain") {
		counter++;
	}
}