using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Mirror;
using Mirror.Examples.Chat;
using NetworkGame.Networking;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Game.Scripts
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(CameraMovement))]
    public class NetworkPlayer : NetworkBehaviour
    {
        //[SyncVar(hook = nameof(OnSetPlayerColor)), SerializeField] private Color[] playerColor = new Color[4];
        public readonly SyncDictionary<uint, string> playerNames = new SyncDictionary<uint, string>();
        
        [SyncVar(hook = nameof(OnSetPlayerColour)), SerializeField] private Color playerColour;
        [SyncVar(hook = nameof(OnSetPlayerName)), SerializeField] private string playerName;
        
        public readonly Dictionary<uint, Color> playerColoursNormal = new Dictionary<uint, Color>();

        public Dictionary<uint, Material> cachedMaterial = new Dictionary<uint, Material>();

        [Tooltip("this is the real player model object, in the child of the root player prefab")]
        [SerializeField] public GameObject playerChildGameObject;

        public PlayerGUI playerGUI;
        //for debugging
        public uint playerID;
        // Typical naming convention for SyncVarHooks is OnSet<VariableName>
        
        /// <summary>
        /// when player colour is changed, this function is called
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        private void OnSetPlayerColour(Color oldValue, Color newValue)
        {
            var cm = playerChildGameObject.GetComponent<MeshRenderer>().material;
            cm.color = newValue;
            playerGUI.renders[(int) netId].avatar.color = newValue;
        }
        
        /// <summary>
        /// when player name is changed, this function is called
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        private void OnSetPlayerName(string oldValue, string newValue)
        {
            playerGUI.renders[(int) netId].playerName.text = newValue;
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
                    CmdRandomColor(netId);
                    CmdRandomName(netId);
                    //CmdTestToggleGUI();
                    Debug.Log($"player {netId }colour changed");
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
        public void CmdRandomColor(uint _netId)
        {
            // SyncVar MUST be set on the server, otherwise it won't be synced between clients
            playerColour = Random.ColorHSV(0, 1, 1, 1, 1, 1);
            RpcUpdateGUIcolor(netId, playerColour);
        }
        
        [ClientRpc]
        public void RpcRandomColor(uint _netId)
        {
            //this is run on client
            
        }

        [Command]
        public void CmdRandomName(uint _netId)
        {
            // SyncVar MUST be set on the server, otherwise it won't be synced between clients
            playerName = DateTime.Now.ToString("hhmmss");
            RpcUpdateGUIname(netId, playerName);
        }
        
        [Command]
        public void CmdTestToggleGUI()
        {
            //test if a client can toggle a server gui and shown on every client
            //if this works it means the gui doesn't need to be in player prefab
            //this works by togling the menu in the server only.
            //the second client doesn't see anything.
            //now test if rpc can do this?
            //yes rpc can do this, the command just need to call the RPC
            //so it means both command and rpc need to be called for it to be the same on all
            //LocalTestToggleGUI();
            RpcTestToggleGUI();
        }

        public void LocalTestToggleGUI()
        {
            RpcTestToggleGUI();
        }
        
        [ClientRpc]
        public void RpcTestToggleGUI()
        {
            var guiObjects = SceneManager.GetSceneByName("GUI").GetRootGameObjects();
            foreach (var go in guiObjects)
            {
                var menu = go.GetComponent<MainMenuGUI>();
                try
                {
                    menu.ToggleMenu();
                    break;
                }
                catch (NullReferenceException)
                {
                    continue;
                }
            }
        }
        [ClientRpc]
        private void RpcUpdateGUIcolor(uint key, Color value)
        {
            Debug.Log($"RpcUpdateGUIcolor netid {netId}");
            playerGUI.UpdateGUIcolour(key, value);
        }
        
        [ClientRpc]
        private void RpcUpdateGUIname(uint key, string value)
        {
            Debug.Log($"RpcUpdateGUItext netid {netId}");
            playerGUI.UpdateGUIname(key, value);
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
            playerID = netId;
//            playerGUI.AssignPlayer(netId);
        }

        public override void OnStopClient()
        {
            MyNetworkManager.RemovePlayer(this);
        }

        // This runs when the server starts... ON the server on all clients
        public override void OnStartServer()
        {
        }
    }
}