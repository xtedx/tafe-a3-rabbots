using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

namespace Game.Scripts
{
    public class PlayerGUI : NetworkBehaviour
    {
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

        [ClientRpc]
        public void UpdateGUI()
        {
            foreach (var pair in MyNetworkManager.Instance.players)
            {
                var pindex = (int) pair.Key - 1;
                Debug.Log($"pindex is {pindex}");
                imagePlayerAvatar[pindex].color = pair.Value.playerColor[pindex];
            }
        }
        
        // Start is called before the first frame update
        void Start()
        {
            playerIndex = (int)netId - 1;
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
