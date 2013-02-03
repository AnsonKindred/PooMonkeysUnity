#pragma strict

var MirvBabyClone:GameObject; 
var hasBroken: boolean;
function Start () {

}

function Update () {
	if (this.rigidbody.velocity.y < 0 && !hasBroken)
	{
		var MirvBaby = Instantiate(MirvBabyClone, this.rigidbody.position + Vector3(2.0,0.0,0.0), Quaternion.LookRotation(Vector3.up,Vector3.up));
		var MirvBaby1 = Instantiate(MirvBabyClone, this.rigidbody.position + Vector3(-2.0,0.0,0.0), Quaternion.LookRotation(Vector3.up,Vector3.up));
		MirvBaby.rigidbody.velocity = this.rigidbody.velocity;
		MirvBaby1.rigidbody.velocity = this.rigidbody.velocity;
		this.transform.localScale -= new Vector3(.50f,.50f,.50f);
		hasBroken = true;
	}
}