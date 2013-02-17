//have to make this separate trigger for the roller because if i were to put trigger in the parent roller, if its trigger it cant be acted upon by the terrain and roll.
//so this one is just slightly larger than the other one so you can detect when the character(clone) is hit for splosions

#pragma strict

function Start () {

}

function Update () {

}

function OnTriggerEnter(other: Collider)
{
	//Debug.Log("YOHO44444444444444444444444444444444444444444444444444444");
	if (other.collider.gameObject.name == "Character(Clone)")
	{
		Destroy(transform.parent.gameObject);
		Destroy(this.gameObject);
		Destroy(other.gameObject);
		var WTF: Vector2 = Vector2(transform.position.x, transform.position.y);
	    GameObject.Find("Terrain").SendMessage("Explode",WTF);
	}
}