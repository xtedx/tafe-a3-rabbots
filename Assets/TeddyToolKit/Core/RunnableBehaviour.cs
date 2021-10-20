using System;
using UnityEngine;

namespace TeddyToolKit.Core
{
	public abstract class RunnableBehaviour : MonoBehaviour, IRunnable
	{
        //must change default to enabled true
		public bool Enabled { get; set; } = true;

        private bool isSetup = false;
        public void Setup(params object[] parameters)
        {
			// If the runnable is already setup, throw an exception to warn the developer
            if (isSetup)
            {
                throw new InvalidOperationException($"GameObject {gameObject.name} already setup");
            }
            OnSetup(parameters);
            isSetup = true;

        }

        public void Run(params object[] parameters)
        {
            //if runnable is enabled and setup, run Onrun with the passed values
            if(Enabled && isSetup) OnRun(parameters);
        }

        protected abstract void OnSetup(params object[] parameters);
        protected abstract void OnRun(params object[] parameters);
    }

}