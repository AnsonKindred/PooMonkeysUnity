#pragma strict
var playerPrefab: GameObject;
var spawnObject: Transform;
var gameName: String = "PooMonkeys_Alpha";

private var refreshing: boolean;
private var hostData: HostData[];

private var buttonX: float;
private var buttonY: float;
private var buttonW: float;
private var buttonH: float;

function Start() {
	buttonX = Screen.width * .02;
	buttonY = Screen.width * .02;
	buttonW = Screen.width * .05;
	buttonH = Screen.width * .05;
}

function startServer() {
	Network.InitializeServer(32, 25001, !Network.HavePublicAddress);
	MasterServer.RegisterHost(gameName, "Start the Poo Party", "this is toots game");
}

function refreshHostList() {
	MasterServer.RequestHostList(gameName);
	refreshing = true;
}

function spawnPlayer() {
	Network.Instantiate(playerPrefab, spawnObject.position, Quaternion.LookRotation(Vector3(Mathf.PI / 2, 0.0, 0.0),Vector3.up), 0);
}

function OnServerInitialized() {
	Debug.Log("Server Initialized");
	spawnPlayer();
}

function OnConnectedToServer() {
	spawnPlayer();
}

function OnMasterServerEvent(mse: MasterServerEvent) {
	if (mse == MasterServerEvent.RegistrationSucceeded) {
		Debug.Log("Registered Server");
	}
}

function Update () {
	if (refreshing) {
		if (MasterServer.PollHostList().Length > 0) {
			refreshing = false;
			hostData = MasterServer.PollHostList();
		}
	}
}

function OnGUI () {
	if (!Network.isClient && !Network.isServer) {
		if (GUI.Button(Rect(buttonX, buttonY, buttonW, buttonH), "Start Server")) {
			Debug.Log("Starting Server");
			startServer();
		}
	
		if (GUI.Button(Rect(buttonX, buttonY * 1.2 + buttonH, buttonW, buttonH), "Refresh Hosts")) {
			Debug.Log("Refreshing");
			refreshHostList();
		}
		if (hostData) {
			for (var i: int = 0; i < hostData.Length; i++) {
				if(GUI.Button(Rect(buttonX * 1.5 + buttonW, buttonY * 1.2 + (buttonH * i), buttonW * 3, buttonH * .5), hostData[i].gameName)) {
					Network.Connect(hostData[i]);
				}
			}
		}
	}
}