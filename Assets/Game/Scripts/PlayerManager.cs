using System;
using Mirror;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public SyncDictionary<uint, double> dashTimestamp = new SyncDictionary<uint, double>();

    public void UpdatePlayerDashStart(uint _netid) => CmdUpdatePlayerDashStart(_netid);
    
    [Command]
    public void CmdUpdatePlayerDashStart(uint _netid)
    {
        // Update times here
        // dashTimestamp.Add(_netid, NetworkTime.time);
        dashTimestamp[_netid] = NetworkTime.time;
    }
}