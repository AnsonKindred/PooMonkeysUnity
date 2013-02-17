#pragma strict

var MirvBabyClone:GameObject; 
var hasBroken: boolean;
function Start () {

}

function Update () {
	if (this.rigidbody.velocity.y < 0 && !hasBroken)
	{
		var MirvBaby = Instantiate(MirvBabyClone, this.rigidbody.position + Vector3(3.0,0.0,0.0), Quaternion.LookRotation(Vector3.up,Vector3.up));
		var MirvBaby1 = Instantiate(MirvBabyClone, this.rigidbody.position + Vector3(-3.0,0.0,0.0), Quaternion.LookRotation(Vector3.up,Vector3.up));
		MirvBaby.rigidbody.velocity = this.rigidbody.velocity;
		MirvBaby.rigidbody.AddForce(Vector3 (8.0, 0.0, 0.0));
		MirvBaby1.rigidbody.velocity = this.rigidbody.velocity;
		MirvBaby1.rigidbody.AddForce(Vector3 (-8.0, 0.0, 0.0));
		//this.transform.localScale -= new Vector3(.50f,.50f,.50f);
		hasBroken = true;
	}
}

function OnTriggerEnter(other: Collider)
{
	if (other.collider.gameObject.name == "Character(Clone)")
	{
		Destroy(this.gameObject);
		Destroy(other.gameObject);
		var WTF: Vector2 = Vector2(transform.position.x, transform.position.y);
	    GameObject.Find("Terrain").SendMessage("Explode",WTF);
	}
	if (other.collider.gameObject.name == "Terrain")
	{
		Destroy(this.gameObject);
		var WTF1: Vector2 = Vector2(transform.position.x, transform.position.y);
	    GameObject.Find("Terrain").SendMessage("Explode",WTF1);
	}
}