using System;
using UnityEngine;

namespace TeddyToolKit.Core
{
    public static class RunnableUtils
    {
        /// <summary>
        /// Attempts to retrieve the runnable behaviour from the passed gameObject or its children.
        /// </summary>
        /// <param name="runnable">The reference the runnable will be set to.</param>
        /// <param name="optional">Whether or not the runnable is optional</param>
        /// <param name="from">The gameObject we are attempting to get a runnable from.</param>
        public static bool Validate<TRunnable>(ref TRunnable runnable, GameObject from, bool optional)
            where TRunnable : IRunnable
        {
            // If the passed Runnable is already set, return true
            if (runnable != null)
            {
                return true;
            }

            // If the passed runnable isn't set, attempt to get it from the passed GameObject
            if (runnable == null)
            {
                runnable = from.GetComponent<TRunnable>();

                // We successfully retrieved the component, so return true
                if (runnable != null)
                {
                    return true;
                }
            }

            // If the passed runnable isn't set, attempt to get it from the passed GameObject's children
            if (runnable == null)
            {
                runnable = from.GetComponentInChildren<TRunnable>();

                // We successfully retrieved the component, so return true
                if (runnable != null)
                {
                    return true;
                }
            }

            if (!optional)
            {
                // The second parameter of Unity's debug.log is the object associated with the LogMessage
                Debug.LogException(
                    new NullReferenceException(
                        $"Component {typeof(TRunnable).Name} is not present in the hierarchy of gameObject {from.name}."),
                    from);
            }

            return false;
        }

        /// <summary>
        /// Attempts to validate then setup the IRunnable, returning whether or not it succeeded.
        /// </summary>
        /// <param name="runnable">The runnable being setup.</param>
        /// <param name="from">The gameObject the runnable is attached to.</param>
        /// <param name="optional">Whether or not the runnable is optional</param>
        /// <param name="parameters">Any additional information the Runnable's setup function needs.</param>
        public static bool Setup<TRunnable>(ref TRunnable runnable, GameObject from, bool optional,
            params object[] parameters)
            where TRunnable : IRunnable
        {
            // Validate the component, if we can, set it up and return true
            if (Validate(ref runnable, from, optional))
            {
                runnable.Setup(parameters);
                return true;
            }

            // We failed to validate the component, so return false
            return false;
        }

        /// <summary>
        /// Attempts to validate the runnable and if successful, run it using the information provided.
        /// </summary>
        /// <param name="runnable">The runnable being run.</param>
        /// <param name="from">The gameObject the runnable is attached to.</param>
        /// <param name="optional">Whether or not the runnable is optional</param>
        /// <param name="parameters">Any additional information the Runnable's run function needs.</param>
        public static void Run<TRunnable>(ref TRunnable runnable, GameObject from, bool optional,
            params object[] parameters)
            where TRunnable : IRunnable
        {
            // Validate the component in case we didn't do it earlier
            if (Validate(ref runnable, from, optional))
            {
                runnable.Run(parameters);
            }
        }
    }
}