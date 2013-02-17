#pragma strict

var counter: int;
//var terrainController1: GameObject;
//var terrainController1 = GameObject.Find("TerrainController");
//var terrainController2: TerrainController;
//var terrainController2: TerrainController = terrainController1.GetComponent(TerrainController);
//var terrainController1 : TerrainController;// = GetComponent(TerrainController);
//var MirvSpawner: GameObject;
//var MirvScript : MirvSpawnerScript = MirvSpawner.GetComponent(MirvSpawnerScript);


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
       	//terrainController1 = gameObject.GetComponent(TerrainControllerScript);
      	//terrainController2.Explode(this.transform.position);
      	//this way is slow but it can suck my balls cause i cant get it to work the other way even though i did it before maybe because
      	//its c# and java mixin'
      	var WTF: Vector2 = Vector2(transform.position.x, transform.position.y);
      	GameObject.Find("Terrain").SendMessage("Explode",WTF);
        Destroy(this.gameObject);
    }
    if (collider.gameObject.name == "Character(Clone)")
    {
    	Destroy(this.gameObject);
    	Destroy(collider.gameObject);
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