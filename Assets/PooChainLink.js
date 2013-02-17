#pragma strict

function Start () {

}

function Update () {

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