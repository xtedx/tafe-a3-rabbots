using System;
using Mirror;
using NetworkGame.Networking;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Scripts
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(CameraMovement))]
    public class NetworkPlayer : NetworkBehaviour
    {
        //[SyncVar(hook = nameof(OnSetPlayerColor)), SerializeField] private Color[] playerColor = new Color[4];
        public readonly SyncDictionary<uint, string> playerNames = new SyncDictionary<uint, string>();
        public readonly SyncList<Color> playerColor = new SyncList<Color>();
        //a list to be synced, containing player data if requried
        //private readonly SyncList<float> syncedFloats = new SyncList<float>();

        // SyncVarHooks get called in the order the VARIABLES are defined not the functions
        // [SyncVar(hook = "SetX")] public float x;
        // [SyncVar(hook = "SetY")] public float y;
        // [SyncVar(hook = "SetZ")] public float z;
        //
        // [Command]
        // public void CmdSetPosition(float _x, float _y, float _z)
        // {
        //     z = _z;
        //     x = _x;
        //     y = _y;
        // }
        
        private Material cachedMaterial;
        [Tooltip("this is the real player model object, in the child of the root player prefab")]
        [SerializeField] public GameObject playerChildGameObject;

        public PlayerGUI playerGUI;
        // Typical naming convention for SyncVarHooks is OnSet<VariableName>
        
        private void OnSetPlayerColor(SyncList<Color>.Operation op, int index, Color oldValue, Color newValue)
        {
            if(cachedMaterial == null)
                cachedMaterial = playerChildGameObject.GetComponent<MeshRenderer>().material;

            cachedMaterial.color = newValue;
            //Debug.Log($"playerGUI is {playerGUI}");
            // playerGUI.OnSetPlayerColour(index, newValue);
            playerGUI.UpdateGUI();
        }
        
        private void Awake()
        {
            // This will run REGARDLESS if we are the local or remote player
        }

        private void Update()
        {
            // First determine if this function is being run on the local player
            if(isLocalPlayer)
            {
                if(Input.GetKeyDown(KeyCode.Return))
                {
                    // Run a function that tells every client to change the colour of this gameObject
                    CmdRandomColor();
                    Debug.Log($"player colour is now {playerColor[playerGUI.playerIndex]}");
                }

                if(Input.GetKeyDown(KeyCode.X))
                {
                    CmdSpawnEnemy();
                }
            }
        }

        [Command]
        public void CmdSpawnEnemy()
        {
            // NetworkServer.Spawn requires an instance of the object in the server's scene to be present
            // so if the object being spawned is a prefab, instantiate needs to be called first
            // GameObject newEnemy = Instantiate(enemyToSpawn);
            // NetworkServer.Spawn(newEnemy);
        }
        
        // RULES FOR COMMANDS:
        // 1. Cannot return anything
        // 2. Must follow the correct naming convention: The function name MUST start with 'Cmd' exactly like that
        // 3. The function must have the attribute [Command] found in Mirror namespace
        // 4. Can only be certain serializable types (see Command in the documentation)
        [Command]
        public void CmdRandomColor()
        {
            // SyncVar MUST be set on the server, otherwise it won't be synced between clients
            try
            {
                playerColor[playerGUI.playerIndex] = Random.ColorHSV(0, 1, 1, 1, 1, 1);
            } catch (ArgumentOutOfRangeException e)
            {
                Debug.Log($"playerColor count is {playerColor.Count}, id is {playerGUI.playerIndex}");
            }
            // This is running on the server
            // RpcRandomColor(Random.Range(0f, 1f));
        }
        
        // RULES FOR CLIENT RPC:
        // 1. Cannot return anything
        // 2. Must follow the correct naming convention: The function name MUST start with 'Rpc' exactly like that
        // 3. The function must have the attribute [ClientRpc] found in Mirror namespace
        // 4. Can only be certain serializable types (see Command in the documentation)
        // [ClientRpc]
        // public void RpcRandomColor(float _hue)
        // {
        //     // This is running on every instance of the same object that the client was calling from.
        //     // i.e. Red GO on Red Client runs Cmd, Red GO on Red, Green and Blue client's run Rpc
        //     MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();
        //     rend.material.color = Color.HSVToRGB(_hue, 1, 1);
        // }

        // This is run via the network starting and the player connecting...
        // NOT Unity
        // It is run when the object is spawned via the networking system NOT when Unity
        // instantiates the object
        public override void OnStartLocalPlayer()
        {
            // This is run if we are the local player and NOT a remote player
            //GetComponent<NetworkSceneManager>().LoadNetworkScene("GUI");
            GameManager.Instance.LoadLocalScene("GUI");
            Debug.Log("loaded scene in network player");
        }

        // This is run via the network starting and the player connecting...
        // NOT Unity
        // It is run when the object is spawned via the networking system NOT when Unity
        // instantiates the object
        public override void OnStartClient()
        {
            // This will run REGARDLESS if we are the local or remote player
            // isLocalPlayer is true if this object is the client's local player otherwise it's false
            PlayerController controller = gameObject.GetComponent<PlayerController>();
            controller.enabled = isLocalPlayer;
            
            MyNetworkManager.AddPlayer(this);
        }

        public override void OnStopClient()
        {
            MyNetworkManager.RemovePlayer(this);
        }

        // This runs when the server starts... ON the server on all clients
        public override void OnStartServer()
        {
            //subscribe to events on the synced dicts
            playerColor.Callback += OnSetPlayerColor;
            for (var i = 0; i < 4; i++)
            {
                playerColor.Add(new Color(0,0,0, 1));
            }
        }
    }
}