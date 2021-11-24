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
        private MyNetworkDiscovery myNetworkDiscovery;
        
        [Space]
        [SerializeField] private Button btnStartHost;
        [SerializeField] private Button btnStartServer;
        [SerializeField] private Button btnConnectLocalhost;
        [SerializeField] private Button btnStopServer;
        [SerializeField] private Button btnStopClient;
        [SerializeField] private Button btnDiscoverServers;
        [SerializeField] private Button btnDebug;
        [SerializeField] private Button buttonTemplateIP;
        [SerializeField] private InputField txtAddress;

        private Dictionary<long, Button> buttonIPs = new Dictionary<long, Button>();

        public void ButtonStartHost()
        {
            Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            discoveredServers.Clear();
            MyNetworkManager.Instance.StartHost();
            myNetworkDiscovery.AdvertiseServer();
        }
        
        public void ButtonStartServer()
        {
            Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            discoveredServers.Clear();
            MyNetworkManager.Instance.StartServer();
            myNetworkDiscovery.AdvertiseServer();
        }
        
        public void ButtonConnectLocalhost()
        {
            Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            //make the default value localhost
            if (txtAddress.text == "") txtAddress.text = "localhost";
            MyNetworkManager.Instance.networkAddress = txtAddress.text;
            MyNetworkManager.Instance.StartClient();
            //save the preference for next time
            PlayerPrefs.SetString("host_address", txtAddress.text);
        }
        
        public void ButtonStopServer()
        {
            Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            discoveredServers.Clear();
            MyNetworkManager.Instance.StopHost();
        }
        
        public void ButtonStopClient()
        {
            Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            discoveredServers.Clear();
            MyNetworkManager.Instance.StopClient();
        }
        
        public void ButtonDiscoverServers()
        {
            Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            discoveredServers.Clear();
            myNetworkDiscovery.StartDiscovery();
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
            myNetworkDiscovery.StopDiscovery();
            MyNetworkManager.Instance.StartClient(info.uri);
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
            Debug.Log($"Called {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            RegisterListeners();
            //automatically start to discover servers
            ButtonDiscoverServers();
            //add the last connected server text
            txtAddress.text = PlayerPrefs.GetString("host_address", "localhost");
        }

        private void OnValidate()
        {
        }

        private void OnEnable()
        {

        }

        private void RegisterListeners()
        {
            myNetworkDiscovery = MyNetworkManager.Instance.GetComponent<MyNetworkDiscovery>();
            myNetworkDiscovery.OnServerFound.AddListener(OnDiscoveredServer);
            Debug.Log($"register listener networkDiscovery {myNetworkDiscovery} {myNetworkDiscovery.OnServerFound}");
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
