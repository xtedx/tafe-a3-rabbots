using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

namespace Game.Scripts
{
    public class PlayerGUI : NetworkBehaviour
    {
        private const int defaultid = 50000000; 
        [Serializable]
        public class PlayerGUIRendering
        {
            public Image avatar;
            public Text playerName;
            public Text hp;
            public Slider slider;

            public uint netId = defaultid;
        }

        public List<PlayerGUIRendering> renders = new List<PlayerGUIRendering>();

            //todo: use dictionary instead of array?
        //private readonly Dictionary<uint, NetworkPlayer> players = new Dictionary<uint, NetworkPlayer>();
    
        // private readonly SyncDictionary<uint, Image> imagePlayerAvatars = new SyncDictionary<uint, Image>();
        //private readonly SyncDictionary<uint, Text> textPlayerNames = new SyncDictionary<uint, Text>();
        [SerializeField]
        public Image[] imagePlayerAvatar;
        [SerializeField]
        public Text[] textPlayerName;
        [SerializeField]
        public Text[] textPlayerHP;
        [SerializeField]
        public Slider[] sliderPlayerHP;
        [SerializeField]
        public Text textTimer;
    
        // [SyncVar(hook = nameof(OnSetTimerValue)), SerializeField] private int[] timer;
        // [SyncVar(hook = nameof(OnSetPlayerHP)), SerializeField] private int[] playerHP;
        //[SyncVar(hook = nameof(OnSetPlayerColour)), SerializeField] private Color[] playerColour;
        
        public int playerIndex = 0;


        [ClientRpc]
        public void OnSetTimerValue(int oldValue, int newValue)
        {
        
            //update the text value of timer
            textTimer.text = newValue.ToString("00");
            //timer = newValue;
        }
    
        [ClientRpc]
        public void OnSetPlayerHP(int oldValue, int newValue)
        {
            //update the text value and slider of hp
            textPlayerHP[playerIndex].text = newValue.ToString("00");
            sliderPlayerHP[playerIndex].value = (float)newValue;
            //playerHP = newValue;
        }

        [ClientRpc]
        public void OnSetPlayerColour(uint index, Color newValue)
        // public void OnSetPlayerColour(SyncList<Color> newValue)
        {
            Debug.Log($"player gui index {playerIndex} {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            var _index = (int)index;
            imagePlayerAvatar[_index].color = newValue;
            
            // foreach (var pair in newValue)
            // {
            //     imagePlayerAvatar[playerIndex].color = pair;
            // }
        }

        public void AssignPlayer(uint _netId)
        {
            for (int i = 0; i < renders.Count; i++)
            {
                PlayerGUIRendering render = renders[i];
                if (render.netId == defaultid)
                {
                    render.netId = _netId;
                    Debug.Log($"assign player {render.netId}, renders.count {renders.Count}");

                }
            }
        }

        public void UpdateGUI(uint _netId)
        {
            /*(foreach (var pair in MyNetworkManager.Instance.players)
            {
                var pindex = (int) pair.Key - 1;
                Debug.Log($"pindex is {pindex}");
                imagePlayerAvatar[pindex].color = pair.Value.playerColor[pindex];
            }*/
            foreach (PlayerGUIRendering render in renders)
            {
                Debug.Log($"in update gui render {render.netId} passed _netid {_netId}");
                if (render.netId == _netId)
                {
                    NetworkPlayer player = MyNetworkManager.Instance.players[_netId];
                    render.avatar.color = player.playerCol;
                    render.playerName.text = player.playerName;
                }
            }
        }

        public void ClearNetIds()
        {
            foreach (PlayerGUIRendering render in renders)
            {
                render.netId = defaultid;
            }
        }
        
        // Start is called before the first frame update
        void Start()
        {
            playerIndex = (int)netId - 1;
            //ClearNetIds();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
