#pragma strict

function Start () {

}

function Update () {
	if (this.rigidbody.IsSleeping())
	{
		var WTF: Vector2 = Vector2(transform.position.x, transform.position.y);
    	GameObject.Find("Terrain").SendMessage("Explode",WTF);
		Destroy(this.gameObject);
	}
}
////cant be trigger because it needs to interact by rolling on the terrain
//function OnTriggerEnter(other: Collider)
//{
//	Debug.Log("YOHO44444444444444444444444444444444444444444444444444444");
//	if (other.gameObject.name == "Character(Clone)")
//	{
//		Destroy(this.gameObject);
//		Destroy(other.gameObject);
//		var WTF: Vector2 = Vector2(transform.position.x, transform.position.y);
//	    GameObject.Find("Terrain").SendMessage("Explode",WTF);
//	}
//}