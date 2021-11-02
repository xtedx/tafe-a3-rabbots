using System;
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
        private MyNetworkDiscovery networkDiscovery;
        
        [Space]
        [SerializeField] private Button btnStartHost;
        [SerializeField] private Button btnStartServer;
        [SerializeField] private Button btnConnectLocalhost;
        [SerializeField] private Button btnStopServer;
        [SerializeField] private Button btnStopClient;
        [SerializeField] private Button btnDiscoverServers;
        [SerializeField] private Button btnDebug;
        [SerializeField] private Button buttonTemplateIP;

        private Dictionary<long, Button> buttonIPs = new Dictionary<long, Button>();

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
        
        public void ButtonConnectLocalhost()
        {
            Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            NetworkManager.singleton.networkAddress = "localhost";
            NetworkManager.singleton.StartClient();
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
            networkDiscovery = NetworkManager.singleton.GetComponent<MyNetworkDiscovery>();
            networkDiscovery.OnServerFound.AddListener(OnDiscoveredServer);
            Debug.Log($"networkDiscovery {networkDiscovery} {networkDiscovery.OnServerFound}");
            //automatically start to discover servers
            ButtonDiscoverServers();
        }

        private void OnValidate()
        {
        }

        private void OnEnable()
        {
            Debug.Log($"Called {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            RegisterListeners();
        }

        private void RegisterListeners()
        {
            btnStartHost.onClick.AddListener(ButtonStartHost);
            btnStartServer.onClick.AddListener(ButtonStartServer);
            btnConnectLocalhost.onClick.AddListener(ButtonConnectLocalhost);
            btnStopServer.onClick.AddListener(ButtonStopServer);
            btnStopClient.onClick.AddListener(ButtonStopClient);
            btnDiscoverServers.onClick.AddListener(ButtonDiscoverServers);
            btnDebug.onClick.AddListener(ButtonDebug);
        }

        // Update is called once per frame
        void Update()
        {
            //PopulateServerList();
        }
        
        
    }
}
