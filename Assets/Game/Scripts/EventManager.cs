using System;
using TeddyToolKit.Core;
using UnityEngine;

namespace Game.Scripts
{
    /// <summary>
    /// Manages C# Events (not the UnityEvents) in the game to reduce dependency, using Observer design pattern
    /// The interested classes will register/listen and do action themselves.
    /// </summary>
    public class EventManager : MonoSingleton<EventManager>
    {
        #region events declaration

        //the events
        public event Action OnTimerDone;
        public event Action<int> OnTimer1SecondTick;

        //the corresponding methods to broadcast called when event happens  
        public void TriggerTimerDone()
        {
            //? means only call Invoke if Ontimer is not null, like if (x==null) 
            OnTimerDone?.Invoke();
        }

        public void TriggerTimer1SecondTick(int id)
        {
            //? means only call Invoke if Ontimer is not null, like if (x==null) 
            OnTimer1SecondTick?.Invoke(id);
        }

        #endregion
    }
}