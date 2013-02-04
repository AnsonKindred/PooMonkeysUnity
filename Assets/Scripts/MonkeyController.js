//TODO: does hitting your head stop your upward speed? if it hits a spot it cant move to it'll store up xVelocity and then use it upon a jump, add sliding down slopes,


// Based off of http://unity3d.com/support/resources/tutorials/2d-gameplay-tutorial.html manipulated by Ehren von Lehe(Langman).

#pragma strict

// Does this script currently respond to Input?
var canControl = true;

// The character will spawn at spawnPoint's position when needed.  This could be changed via a script at runtime to implement, e.g. waypoints/savepoints.
var spawnPoint : Transform;

var fire1: boolean;
var fire2: boolean;
var fire3: boolean;
var fire4: boolean;
//var currentMovementOffset: Vector3;
//var lastPosition: float;
static var angle: float;
static var power: float;

//var MirvClone: GameObject;
//var PooChainSpawnerPrefab: GameObject;
var DrillerSpawnerPrefab: GameObject;
var IceCuboidSpawnerPrefab: GameObject;

var canMirv: boolean = true;
var canChain: boolean = true;

var MirvSpawner: GameObject;
var MirvScript : MirvSpawnerScript = MirvSpawner.GetComponent(MirvSpawnerScript);

var PooChainSpawner: GameObject;
var PCScript : PooChainSpawnerScript = PooChainSpawner.GetComponent(PooChainSpawnerScript);

class MonkeyControllerMovement {
	// The speed when running
	var runSpeed = 7.0;

	// The speed when sliding up and around corners 
	// The next line, @System.NonSerialized , tells Unity to not serialize the variable or show it in the inspector view.  Very handy for organization!
	var slideAroundEdgesSpeedFactor = 0.05;
	@System.NonSerialized
	var slideX = 0.0;
	
	// The gravityAndJumpSpeed for the character
	var gravityAndJumpSpeed = 60.0;
	var maxFallSpeed = 20.0;

	// How fast does the character change speeds?  Higher is faster.
	var speedSmoothing = 20.0;

	// The current move direction in x-y.  This will always been (1,0,0) or (-1,0,0)
	@System.NonSerialized
	var direction = Vector3.zero;

	// The current vertical speed
	@System.NonSerialized
	var verticalSpeed = 0.0;

	// The current movement speed.  This gets smoothed by speedSmoothing.
	@System.NonSerialized
	var speed = 0.0;

	// Is the user pressing the left or right movement keys?
	@System.NonSerialized
	var isMoving = false;

	// The last collision flags returned from controller.Move
	@System.NonSerialized
	var collisionFlags : CollisionFlags; 

	// We will keep track of an approximation of the character's current velocity, so that we return it from GetVelocity () for our camera to use for prediction.
	@System.NonSerialized
	var velocity : Vector3;
	
	// This will keep track of how long we have we been in the air (not grounded)
	@System.NonSerialized
	var hangTime = 0.0;
}

var movement : MonkeyControllerMovement;

// We will contain all the jumping related variables in one helper class for clarity.
class MonkeyControllerJumping {
	// Can the character jump?
	var enabled = true;

	// How high do we jump when pressing jump and letting go immediately
	var height = 0.5;
	// We add extraHeight units (meters) on top when holding the button down longer while jumping
	var extraHeight = 1.6;
	
	// How fast does the character change speeds?  Higher is faster.
	var speedSmoothing = 3.0;

	// How fast does the character move horizontally when in the air.
	var airSpeedX = 9.5;
	
	// This prevents inordinarily too quick jumping
	// The next line, @System.NonSerialized , tells Unity to not serialize the variable or show it in the inspector view.  Very handy for organization!
	@System.NonSerialized
	var minTimeBetweenJumps = 0.05;

	@System.NonSerialized
	var canPressJumpBeforeLandingTime = 0.15;

	// Are we jumping? (Initiated with jump button and not grounded yet)
	@System.NonSerialized
	var jumping = false;
	
	@System.NonSerialized
	var reachedApex = false;
  
	// Last time the jump button was clicked down
	@System.NonSerialized
	var lastButtonTime = -10.0;
	
	// Last time we were grounded
	@System.NonSerialized
	var lastGroundedTime = -10.0;

	@System.NonSerialized
	var timeAfterLeavingGroundAndCanJump = .1;

	// Last time we performed a jump
	@System.NonSerialized
	var lastTime = -1.0;

	// the height we jumped from (Used to determine for how long to apply extra jump power after jumping.)
	@System.NonSerialized
	var lastStartHeight = 0.0;

	@System.NonSerialized
	var touchedCeiling = false;

	@System.NonSerialized
	var buttonReleased = true;
}

var jump : MonkeyControllerJumping;

private var controller : CharacterController;

// Moving platform support.
private var activePlatform : Transform;
private var activeLocalPlatformPoint : Vector3;
private var activeGlobalPlatformPoint : Vector3;
private var lastPlatformVelocity : Vector3;

private var sprite : GameObject;

function Awake () {
	movement.direction = transform.TransformDirection (Vector3.forward);
	controller = GetComponent (CharacterController) as CharacterController;
	sprite = transform.Find("Sprite").gameObject;
}

function Spawn () {
	// reset the character's speed
	movement.verticalSpeed = 0.0;
	movement.speed = 0.0;
	
	// make sure we're not attached to a platform
	activePlatform = null;
	
	// reset the character's position to the spawnPoint
	transform.position = spawnPoint.position;
	
	sprite.SetActive(true);
	canControl = true;
}

function Unspawn() {
	canControl = false;
	sprite.SetActive(false);
}

function UpdateSmoothedMovementDirection () {	
	var h = Input.GetAxisRaw ("Horizontal");
	
	if (!canControl)
		h = 0.0;
	
	movement.isMoving = Mathf.Abs (h) > 0.1;
	
	// Smooth the speed based on the current target direction
	var curSmooth = 0.0;
	// Choose target speed
	var targetSpeed = h;
	
	if(controller.isGrounded){
		curSmooth = movement.speedSmoothing * Time.smoothDeltaTime;
		targetSpeed *= movement.runSpeed;
		movement.hangTime = 0.0;
	}else{
		curSmooth = jump.speedSmoothing * Time.smoothDeltaTime;
		targetSpeed *= jump.airSpeedX;
		movement.hangTime += Time.smoothDeltaTime;
	}
	
	var newSpeed: float = Mathf.Lerp (movement.speed*movement.direction.x, targetSpeed, curSmooth);
	if((newSpeed >= 0) != (movement.direction.x >= 0))
	{
		movement.direction = new Vector3(h, 0, 0);
	}
	movement.speed = Mathf.Abs(newSpeed);
}

function AnimateCharacter() {
	// For an example of animating a sprite sheet, see:
	// http://www.unifycommunity.com/wiki/index.php?title=Animating_Tiled_texture
	if (movement.isMoving){
		// run
	}else if(controller.isGrounded){
		// stand
	}
}

function JustBecameUngrounded() {

	return (Time.time < (jump.lastGroundedTime + jump.timeAfterLeavingGroundAndCanJump) && jump.lastGroundedTime > jump.lastTime);
}

function ApplyJumping() {
	if (Input.GetButton ("Jump") && canControl) {
		jump.lastButtonTime = Time.time;
	}

	// Prevent jumping too fast after each other
	if (jump.lastTime + jump.minTimeBetweenJumps > Time.time){
		return;
	}

	var isGrounded = controller.isGrounded;
	
	// Allow jumping slightly after the character leaves a ledge,
	// as long as a jump hasn't occurred since we became ungrounded.
	if (isGrounded || JustBecameUngrounded()) {
		if(isGrounded){
			jump.lastGroundedTime = Time.time;
		}
		
		// Jump		
		if (jump.enabled && Input.GetButton ("Jump")) {
			movement.verticalSpeed = CalculateJumpVerticalSpeed (jump.height);
			SendMessage ("DidJump", SendMessageOptions.DontRequireReceiver);
		}
	}
}

//makes it so you have to press to jump everytime instead of holding it down
function ApplyJumpingPressEveryJump() {
	if (Input.GetButtonDown ("Jump") && canControl) {
		jump.lastButtonTime = Time.time;
	}

	// Prevent jumping too fast after each other
	if (jump.lastTime + jump.minTimeBetweenJumps > Time.time){
		return;
	}

	var isGrounded = controller.isGrounded;
	
	// Allow jumping slightly after the character leaves a ledge,
	// as long as a jump hasn't occurred since we became ungrounded.
	if (isGrounded || JustBecameUngrounded()) {
		if(isGrounded){
			jump.lastGroundedTime = Time.time;
		}
		
		// Jump
		// - Only when pressing the button down
		// - With a canPressJumpBeforeLandingTime so you can press the button slightly before landing		
		if (jump.enabled && Time.time < jump.lastButtonTime + jump.canPressJumpBeforeLandingTime) {
			movement.verticalSpeed = CalculateJumpVerticalSpeed (jump.height);
			SendMessage ("DidJump", SendMessageOptions.DontRequireReceiver);
		}
	}
}

function ApplyGravity () {
	// Apply gravity
	var jumpButton = Input.GetButton ("Jump");
	
	if (!canControl)
		jumpButton = false;
		
	// When we reach the apex of the jump we send out a message
	if (jump.jumping && !jump.reachedApex && movement.verticalSpeed <= 0.0) {
		jump.reachedApex = true;
		SendMessage ("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
	}
	
	// * When jumping up we don't apply gravity for some time when the user is holding the jump button
	//   This gives more control over jump height by pressing the button longer
	if (!jump.touchedCeiling && IsTouchingCeiling()){
		jump.touchedCeiling = true; // store this so we don't allow extra power jump to continue after character hits ceiling.
	}
	if (!jumpButton){
		jump.buttonReleased = true;
	}
	
	var extraPowerJump = jump.jumping && movement.verticalSpeed > 0.0 && jumpButton && !jump.buttonReleased && transform.position.y < jump.lastStartHeight + jump.extraHeight && !jump.touchedCeiling;
	
	if (extraPowerJump){
		return;
	}else if (controller.isGrounded){
		movement.verticalSpeed = -movement.gravityAndJumpSpeed * Time.smoothDeltaTime;
		
	}else{
		movement.verticalSpeed -= movement.gravityAndJumpSpeed * Time.smoothDeltaTime;
	}
		
	// Make sure we don't fall any faster than maxFallSpeed.  This gives our character a terminal velocity.
	movement.verticalSpeed = Mathf.Max (movement.verticalSpeed, -movement.maxFallSpeed);
}

function CalculateJumpVerticalSpeed (targetJumpHeight : float) {
	// From the jump height and gravityAndJumpSpeed we deduce the upwards speed 
	// for the character to reach at the apex.
	return Mathf.Sqrt (2 * targetJumpHeight * movement.gravityAndJumpSpeed);
}

function DidJump () {
	jump.jumping = true;
	jump.reachedApex = false;
	jump.lastTime = Time.time;
	jump.lastStartHeight = transform.position.y;
	jump.lastButtonTime = -10;
	jump.touchedCeiling = false;
	jump.buttonReleased = false;
}

//does jumping and hitting your head make you zero y velocity???
function FixedUpdate() 
{
	FlingPoo();
}
function TwoSeconds()
{	
	yield WaitForSeconds(2.0);
	canMirv = true;
}
function FiveSeconds()
{
	yield WaitForSeconds(5.0);
	canChain = true;
}

//might want to make the transition to FixedUpdate()
function Update () {
	// Make sure we are always in the 2D plane.
	transform.position.z = 0.0;
	if (networkView.isMine) 
	{
		if (Input.GetKeyDown ("1") && canMirv) {
			fire1 = true;
			canMirv = false;		
			TwoSeconds();
		}
		if (Input.GetKeyDown ("2") && canChain) {
			fire2 = true;
			canChain = false;
			FiveSeconds();
		}
		if (Input.GetKeyDown ("3")) {
			fire3 = true;
		}
		if (Input.GetKeyDown ("4")) {
			fire4 = true;
		}
	
		UpdateSmoothedMovementDirection();
		
		AnimateCharacter();
	
		// Apply gravity
		// - extra power jump modifies gravity
		ApplyGravity ();
	
		// Apply jumping logic
		ApplyJumping ();
		
		var platformMovementOffset = Vector3.zero;
		
		// Moving platform support
		if (activePlatform != null && !jump.jumping) {
			var newGlobalPlatformPoint = activePlatform.TransformPoint(activeLocalPlatformPoint);
			var moveDistance = (newGlobalPlatformPoint - activeGlobalPlatformPoint);
			// Setting transform.position directly causes us to go through walls if we're on a rotating block.
			// But it's necessary to make moving platforms work.
			if(activePlatform.rigidbody.isKinematic && activePlatform.rigidbody.velocity.sqrMagnitude > 0.0){
				// Moving platform. Change the position directly so the character
				// won't fall through the platform.
				transform.position = transform.position + moveDistance;
			}else{
				// Store the desired movement for use in CharacterController.Move.
				platformMovementOffset = moveDistance;
			}
			lastPlatformVelocity = (newGlobalPlatformPoint - activeGlobalPlatformPoint) / Time.smoothDeltaTime;
		} else {
			lastPlatformVelocity = Vector3.zero;	
		}
		
		activePlatform = null;
		
		// Save lastPosition for velocity calculation.
		var lastPosition = transform.position;
		
		// Calculate actual motion
		var currentMovementOffset = (movement.direction * movement.speed) + Vector3 (0.0, movement.verticalSpeed, 0.0);
		
		// We always want the movement to be framerate independent.  Multiplying by Time.smoothDeltaTime does this.
		currentMovementOffset *= Time.smoothDeltaTime;
		currentMovementOffset += platformMovementOffset;
		currentMovementOffset.x += movement.slideX * movement.slideAroundEdgesSpeedFactor;
		// Reset sliding to zero. It will be set in controller.Move
		movement.slideX = 0.0;
		
	   	// Move our character!
	   	// We can get null refs here
	   	movement.collisionFlags = controller.Move (currentMovementOffset);
		
		// Calculate the velocity based on the current and previous position.  
		// This means our velocity will only be the amount the character actually moved as a result of collisions.
		movement.velocity = (transform.position - lastPosition) / Time.smoothDeltaTime;
		
		// Moving platforms support
		if (activePlatform != null) {
			activeGlobalPlatformPoint = transform.position;
			activeLocalPlatformPoint = activePlatform.InverseTransformPoint (transform.position);
		}
	
		// We are in jump mode but just became grounded
		if (controller.isGrounded) {
			if (jump.jumping) {
				jump.jumping = false;
				SendMessage ("DidLand", SendMessageOptions.DontRequireReceiver);
	
				var jumpMoveDirection = movement.direction * movement.speed;
				if (jumpMoveDirection.sqrMagnitude > 0.01)
					movement.direction = jumpMoveDirection.normalized;
			}
		}
	}
	else 
	{
		enabled = false;
	}
}

function OnControllerColliderHit (hit : ControllerColliderHit)
{
	// Make sure we are really standing on a straight platform
	// Not on the underside of one and not falling down from it either!
	if (hit.moveDirection.y < -0.9 && hit.normal.y > 0.9 
		&& hit.rigidbody 
		&& (!hit.rigidbody.isKinematic || hit.rigidbody.velocity.sqrMagnitude > 0.0)) {
		activePlatform = hit.collider.transform;
	}else if (jump.jumping && hit.moveDirection.y > 0.0 && hit.normal.y < 0.0 && Mathf.Abs(hit.normal.x) > 0.01){
		movement.slideX = hit.normal.x;
	}
}

function FlingPoo ()
{	
	var a = Input.GetAxisRaw ("Angle");
	var p = Input.GetAxisRaw ("Power");

	if (a > 0) {
		angle = angle + .05;
	}
	if (a < 0) {
		angle = angle - .05;
	}
	if (p > 0) {
		power++;
		Debug.Log("power " + power);
	}
	if (p < 0) {
		power--;
	}
	if (power < 0) {
		power = 0;
	}
	if (power > 400) {
		power = 400;
	}
	
	//Debug.Log("power" + power);
	//Debug.Log("angle" + angle);
	if (fire4) 
	{
		var IceCuboidSpawnerClone = Instantiate(IceCuboidSpawnerPrefab, transform.position + Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0.0), transform.rotation);
	    fire4 = false;
	}
	
	if (fire3) 
	{
		var DrillerClone = Instantiate(DrillerSpawnerPrefab, transform.position + Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0.0), transform.rotation);
	    fire3 = false;
	}
	
	if (fire2) 
	{
		//var PooChainClone = Instantiate(PooChainSpawnerPrefab, transform.position + Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0.0), transform.rotation);
	    PCScript.Fire(transform.position, angle, power);
	    fire2 = false;
	    //PooChainClone.rigidbody.AddForce(Vector3 (Mathf.Cos(angle)*power, Mathf.Sin(angle)*power, 0.0));
	    //fire2 = false;
	}
	
	if (fire1) 
	{
	    //var MirvClonetClone = Instantiate(MirvSpawnerPrefab, transform.position + Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0.0), transform.rotation);
	    //var Mirv = Network.Instantiate(MirvClone, transform.position + Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0.0), Quaternion.LookRotation(Vector3(0.0, Mathf.Sin(angle), Mathf.PI / 2),Vector3.up),0);
//	    Mirv.rigidbody.AddForce(Mathf.Cos(angle) * power, Mathf.Sin(angle) * power, 0.0);
	    MirvScript.Fire(transform.position, angle, power);
	    fire1 = false;
	    //MirvClone.rigidbody.AddForce(Vector3 (Mathf.Cos(angle)*power, Mathf.Sin(angle)*power, 0.0));
	    //Network.Instantiate(playerPrefab, spawnObject.position, Quaternion.LookRotation(Vector3(Mathf.PI / 2, 0.0, 0.0),Vector3.up), 0);
	    //fire1 = false;
    }
}

function OnTriggerEnter(other: Collider)
	{
		Destroy(this.gameObject);
		Destroy(other.gameObject);
    }

// Various helper functions below:
function GetSpeed () {
	return movement.speed;
}

function GetVelocity () {
	return movement.velocity;
}


function IsMoving () {
	return movement.isMoving;
}

function IsJumping () {
	return jump.jumping;
}

function IsTouchingCeiling () {
	return (movement.collisionFlags & CollisionFlags.CollidedAbove) != 0;
}

function GetDirection () {
	return movement.direction;
}

function GetHangTime() {
	return movement.hangTime;
}

function Reset () {
	gameObject.tag = "Player";
}

function SetControllable (controllable : boolean) {
	canControl = controllable;
}

// Require a character controller to be attached to the same game object
@script RequireComponent (CharacterController)
@script AddComponentMenu ("2D Platformer/Monkey Controller")
