using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game.Scripts
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(CameraMovement))]
    public class NetworkPlayer : NetworkBehaviour
    {
        /// <summary>
        /// for debugging only to show at the inspector window
        /// </summary>
        public uint playerID;
        /// <summary>
        /// this is used as the index in the GUI elements (gui renderer class PlayerGUIRendering)
        /// because netid starts from 1, but csharp arrays starts from 0
        /// </summary>
        /// <exception cref="Exception"></exception>
        public int GuiIndex
        {
            get
            {
                var index = netId - 1;
                if (index >= 0)
                {
                    return (int) index;
                }
                else
                {
                    throw new Exception("player id used for index cannot be less than 0");
                }
            }
        }
        /// <summary>
        /// this is to contain the main menu gui, but because this is in a different scene, it will be set in code using the public getter
        /// </summary>
        private MainMenuGUI mainMenuGUI;
        /// <summary>
        /// this is to prevent null on the game object, because unity game object cannot accurately be compared with == null
        /// until a property is used, and thats why we need the try catch to check the count of renders element
        /// otherwise for other objects, using the object?.property syntax would be easier.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public MainMenuGUI MainMenuGUI
        {
            get
            {
                if (mainMenuGUI != null)
                {
                    return mainMenuGUI;
                }
                else
                {
                    //initialise the GUI
                    var guiObjects = SceneManager.GetSceneByName("GUI").GetRootGameObjects();
                    bool hasGui = false;
                    foreach (var go in guiObjects)
                    {
                        mainMenuGUI = go.GetComponent<MainMenuGUI>();
                        try
                        {
                            var testnull = mainMenuGUI.renders.Count;
                            //if no exception after this line, then all good
                            hasGui = true;
                            break;
                        }
                        catch (NullReferenceException nre)
                        {
                            continue;
                        }
                    }

                    //if after looping but still no gui then fatal error
                    if (!hasGui)
                    {
                        throw new NullReferenceException("need gui to continue, but not found");
                    }

                    return mainMenuGUI;
                }
            }
        }
        
        public Text textTimer;
        
        // although the gui needs to display all players' name/colour/hp etc,
        // we dont need SyncList/SyncDict here, we'll use a list in the GUI as we only need them there.
        // each variables here per client will be managed by mirror
        
        [SyncVar(hook = nameof(OnSetPlayerColour)), SerializeField] private Color playerColour;
        [SyncVar(hook = nameof(OnSetPlayerName)), SerializeField] private string playerName;
        
        /// <summary>
        /// to change the object colour
        /// </summary>
        private Material cachedMaterial;

        [Tooltip("this is the real player model object, in the child of the root player prefab")]
        [SerializeField] public GameObject playerChildGameObject;

        // Typical naming convention for SyncVarHooks is OnSet<VariableName>
        
        /// <summary>
        /// when player colour is changed, this function is called and update the player object's material
        /// then update the gui
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        private void OnSetPlayerColour(Color oldValue, Color newValue)
        {
            cachedMaterial = playerChildGameObject.GetComponent<MeshRenderer>().material;
            //the scarf colour
            cachedMaterial.color = newValue;
            //the lines/eyes colour
            cachedMaterial.SetColor("_EmissionColor", newValue);
            MainMenuGUI.renders[GuiIndex].avatar.color = newValue;
        }
        
        /// <summary>
        /// when player name is changed, this function is called, and update the gui here
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        private void OnSetPlayerName(string oldValue, string newValue)
        {
            MainMenuGUI.renders[GuiIndex].playerName.text = newValue;
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
                    CmdRandomName();
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

        /// <summary>
        /// RULES FOR COMMANDS:
        /// 1. Cannot return anything
        /// 2. Must follow the correct naming convention: The function name MUST start with 'Cmd' exactly like that
        /// 3. The function must have the attribute [Command] found in Mirror namespace
        /// 4. Can only be certain serializable types (see Command in the documentation)
        /// This is called by the client, and being run in the server
        /// then to propagate the change to all other cliets, call an RPC function to deal with the change
        /// in this case update tgui
        /// </summary>
        [Command]
        public void CmdRandomColor()
        {
            // SyncVar MUST be set on the server, otherwise it won't be synced between clients
            playerColour = Random.ColorHSV(0, 1, 1, 1, 1, 1);
            
            RpcUpdateGUIcolor(netId, playerColour);
        }
        
        /// <summary>
        /// RULES FOR CLIENT RPC:
        /// 1. Cannot return anything
        /// 2. Must follow the correct naming convention: The function name MUST start with 'Rpc' exactly like that
        /// 3. The function must have the attribute [ClientRpc] found in Mirror namespace
        /// 4. Can only be certain serializable types (see Command in the documentation)
        ///
        /// this is run on the client
        ///
        /// <example>
        /// [ClientRpc]
        /// public void RpcRandomColor(float _hue)
        /// {
        ///     // This is running on every instance of the same object that the client was calling from.
        ///     // i.e. Red GO on Red Client runs Cmd, Red GO on Red, Green and Blue client's run Rpc
        ///     MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();
        ///     rend.material.color = Color.HSVToRGB(_hue, 1, 1);
        /// }
        /// </example>
        /// </summary>
        [ClientRpc]
        public void RpcRandomColor()
        {
            
        }

        [Command]
        public void CmdRandomName()
        {
            // SyncVar MUST be set on the server, otherwise it won't be synced between clients
            playerName = DateTime.Now.ToString("mmss");
            playerName = $"Bot{playerName}";
            RpcUpdateGUIname(netId, playerName);
        }
        
        [ClientRpc]
        private void RpcUpdateGUIcolor(uint key, Color value)
        {
            Debug.Log($"RpcUpdateGUIcolor netid {netId}");
            foreach (PlayerGUIRendering render in MainMenuGUI.renders)
            {
                if (render.netId == key)
                {
                    Debug.Log($"in update gui render {render.avatar.name} for {key} color is {value}");
                    render.avatar.color = value;
                }
            }
        }
        
        [ClientRpc]
        private void RpcUpdateGUIname(uint key, string value)
        {
            Debug.Log($"RpcUpdateGUIname netid {netId}");
            foreach (PlayerGUIRendering render in MainMenuGUI.renders)
            {
                if (render.netId == key)
                {
                    Debug.Log($"in update gui render {render.playerName.text} for {key} color is {value}");
                    render.playerName.text = value;
                }
            }
        }
        
        #region test gui code
        /// <summary>
        ///test if a client can toggle a server gui and shown on every client
        ///if this works it means the gui doesn't need to be in player prefab
        ///this works by togling the menu in the server only.
        ///the second client doesn't see anything.
        ///now test if rpc can do this?
        ///yes rpc can do this, the command just need to call the RPC
        ///so it means both command and rpc need to be called for it to be the same on all
        ///LocalTestToggleGUI();
        /// </summary>
        [Command]
        public void CmdTestToggleGUI()
        {
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
        #endregion
        
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
        
        /// <summary>
        /// This is run via the network starting and the player connecting...
        /// NOT Unity
        /// It is run when the object is spawned via the networking system NOT when Unity
        /// instantiates the object
        /// This will run REGARDLESS if we are the local or remote player
        /// isLocalPlayer is true if this object is the client's local player otherwise it's false
        /// </summary>
        public override void OnStartClient()
        {
            PlayerController controller = gameObject.GetComponent<PlayerController>();
            controller.enabled = isLocalPlayer;
            
            MyNetworkManager.AddPlayer(this);
            //for debugging
            playerID = netId;
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