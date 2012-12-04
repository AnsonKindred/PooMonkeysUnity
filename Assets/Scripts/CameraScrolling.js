#pragma strict

// The object in our scene that our camera is currently tracking.
var target : Transform;
// How far back should the camera be from the target?
var distance = 15.0;
// How strict should the camera follow the target?  Lower values make the camera more lazy.
var springiness = 4.0;
// Constrains camera in Y position
var yConstraint = 13.0;

// Our camera script can take attributes from the target.  If there are no attributes attached, we have
// the following defaults.

// How high in world space should the camera look above the target?
private var heightOffset = 0.0;
// How much should we zoom the camera based on this target?
private var distanceModifier = 1.0;
// By default, we won't account for any target velocity in our calculations;
private var velocityLookAhead = 0.0;
private var maxLookAhead = Vector2 (0.0, 0.0);

// Look for CameraTarget in our target.
private var cameraTarget : CameraTarget;
private var monkeyTarget : MonkeyController;

private var cam : Camera;
private var trans : Transform;
	
function Awake () {
	if(target){
		SetTarget(target);
	}
	cam = camera;
	trans = transform;
}

function SetTarget(newTarget : Transform) {
	// Set our current target to be the value passed to SetTarget ()
	target = newTarget;
	if(target){
		cameraTarget = target.GetComponent(CameraTarget) as CameraTarget;
		// If our target has special attributes, use these instead of our above defaults.
		if  (cameraTarget) {
			heightOffset = cameraTarget.heightOffset;
			distanceModifier = cameraTarget.distanceModifier;
			velocityLookAhead = cameraTarget.velocityLookAhead;
			maxLookAhead = cameraTarget.maxLookAhead;
		}
		monkeyTarget = target.GetComponent (MonkeyController) as MonkeyController;
	}else{
		cameraTarget = null;
		monkeyTarget = null;
	}
}

// You almost always want camera motion to go inside of LateUpdate (), so that the camera follows
// the target _after_ it has moved.  Otherwise, the camera may lag one frame behind.
function LateUpdate() {
	if(Time.time < 0.1)
		return;
	// Where should our camera be looking right now?
	var goalPosition = GetGoalPosition();
	
	// Interpolate between the current camera position and the goal position.
	// See the documentation on Vector3.Lerp () for more information.
	trans.position = Vector3.Lerp(trans.position, goalPosition, Time.smoothDeltaTime * springiness);	
}

// Based on the camera attributes and the target's special camera attributes, find out where the
// camera should move to.
function GetGoalPosition () {
	// If there is no target, don't move the camera.  So return the camera's current position as the goal position.
	if  (!target)
		return trans.position;
		
	// First do a rough goalPosition that simply follows the target at a certain relative height and distance.
	var goalPosition = target.position + Vector3 (0, heightOffset, -distance * distanceModifier);
	
	// Next, we refine our goalPosition by taking into account our target's current velocity.
	// This will make the camera slightly look ahead to wherever the character is going.
	
	// First assume there is no velocity.
	// This is so if the camera's target is not a Rigidbody, it won't do any look-ahead calculations because everything will be zero.
	var targetVelocity = Vector3.zero;
	
	// If we find a MonkeyController on the target, we can access a velocity from that!
	if (monkeyTarget){
		targetVelocity = monkeyTarget.GetVelocity();
	}
	
	// If you've had a physics class, you may recall an equation similar to: position = velocity * time;
	// Here we estimate what the target's position will be in velocityLookAhead seconds.
	var lookAhead = targetVelocity * velocityLookAhead;
	
	// We clamp the lookAhead vector to some sane values so that the target doesn't go offscreen.
	// This calculation could be more advanced (lengthy), taking into account the target's viewport position,
	// but this works pretty well in practice.
	lookAhead.x = Mathf.Clamp (lookAhead.x, -maxLookAhead.x, maxLookAhead.x);
	lookAhead.y = Mathf.Clamp (lookAhead.y, -maxLookAhead.y, maxLookAhead.y);
	// We never want to take z velocity into account as this is 2D.  Just make sure it's zero.
	lookAhead.z = 0.0;
	
	// Now add in our lookAhead calculation.  Our camera following is now a bit better!
	goalPosition += lookAhead;
	
	// To put the icing on the cake, we will make so the positions beyond the level boundaries
	// are never seen.  This gives your level a great contained feeling, with a definite beginning
	// and ending.

	goalPosition.y = Mathf.Max(goalPosition.y, -10 + yConstraint);
		
	// Send back our spiffily calculated goalPosition back to the caller!
	return goalPosition;
}
