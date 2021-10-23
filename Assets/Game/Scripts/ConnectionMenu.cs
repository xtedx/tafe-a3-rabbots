using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts
{
    public class ConnectionMenu : MonoBehaviour
    {
        readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
        public MyNetworkDiscovery networkDiscovery;
        [Space] [SerializeField] private Button buttonTemplateIP;
        private Dictionary<long, Button> buttonIPs = new Dictionary<long, Button>();
#if UNITY_EDITOR
        void OnValidate()
        {
            if (networkDiscovery == null)
            {
                Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                networkDiscovery = GetComponent<MyNetworkDiscovery>();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
                UnityEditor.Undo.RecordObjects(new Object[] { this, networkDiscovery }, "Set NetworkDiscovery");
            }
        }
#endif
        public void ButtonStartHost()
        {
            Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            discoveredServers.Clear();
            NetworkManager.singleton.StartHost();
            networkDiscovery.AdvertiseServer();
        }
        
        public void ButtonStartServer()
        {
            Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            discoveredServers.Clear();
            NetworkManager.singleton.StartServer();
            networkDiscovery.AdvertiseServer();
        }
        
        public void ButtonStopServer()
        {
            Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            discoveredServers.Clear();
            NetworkManager.singleton.StopHost();
        }
        
        public void ButtonStopClient()
        {
            Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            discoveredServers.Clear();
            NetworkManager.singleton.StopClient();
            NetworkManager.singleton.OnStartServer();
        }
        
        public void ButtonDiscoverServers()
        {
            Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            discoveredServers.Clear();
            networkDiscovery.StartDiscovery();
        }
        
        public void ButtonDebug()
        {
            Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            Debug.Log($"discoveredServers {discoveredServers}, count {discoveredServers.Count}");
            PopulateServerList();
        }

        /// <summary>
        /// clears then populates the server list (scroll view panel) when the event discovered server is fired
        /// </summary>
        public void PopulateServerList()
        {
            // Debug.Log($"Called {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            // servers
            // scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);
            ClearServerList();
            foreach (ServerResponse info in discoveredServers.Values)
            {
                var ipaddress = info.EndPoint.Address.ToString();
                Debug.Log(ipaddress);
                var button = Instantiate(buttonTemplateIP, buttonTemplateIP.transform.parent);
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<Text>().text = ipaddress;
                buttonIPs.Add(info.serverId, button);
            }
        }

        /// <summary>
        /// clears the server/game list
        /// </summary>
        public void ClearServerList()
        {
            foreach (var pair in buttonIPs)
            {
                Destroy(pair.Value.gameObject);
            }
            buttonIPs.Clear();
        }
        
        public void Connect(ServerResponse info)
        {
            networkDiscovery.StopDiscovery();
            NetworkManager.singleton.StartClient(info.uri);
        }

        public void OnDiscoveredServer(ServerResponse info)
        {
            // Note that you can check the versioning to decide if you can connect to the server or not using this method
            discoveredServers[info.serverId] = info;
            PopulateServerList();
            Debug.Log($"Called {System.Reflection.MethodBase.GetCurrentMethod().Name}");
        }
        
        // Start is called before the first frame update
        void Start()
        {
            //automatically start to discover servers
            ButtonDiscoverServers();
        }

        // Update is called once per frame
        void Update()
        {
            //PopulateServerList();
        }
    }
}
