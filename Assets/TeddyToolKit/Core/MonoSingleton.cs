using System;
using UnityEngine;

namespace TeddyToolKit.Core
{
    /// <summary>
    /// A <a href="https://en.wikipedia.org/wiki/Singleton_pattern">Singleton design pattern</a> for any Generic MonoBehaviour class
    /// useful for  xxxManager classes like GameManager, UiManager, AudioManager, etc. 
    /// </summary>
    /// <typeparam name="TSingletonType">The type of the class, e.g. GameManager</typeparam>
    public class MonoSingleton<TSingletonType> : MonoBehaviour
        where TSingletonType : MonoSingleton<TSingletonType> // This makes sure that SINGLETON_TYPE inherits from MonoSingleton
    {
        public static TSingletonType Instance
        {
            get
            {
                // The internal instance isn't set, so attempt to find it in the scene
                if (instance == null)
                {
                    instance = FindObjectOfType<TSingletonType>();

                    // No instance was found, so throw a NullReferenceException detailing what singleton caused the error
                    if (instance == null)
                    {
                        // The 'typeof(TSingletonType).Name' shows the exact class name of the generic type
                        // This line will also give us a stacktrace, showing where the call to the Instance was before it existed
                        throw new NullReferenceException(
                            $"No objects of type: {typeof(TSingletonType).Name} was found.");
                    }
                }
                return instance;
            }
        }

        private static TSingletonType instance = null;

        /// <summary>
        /// Has the singleton been generated?
        /// </summary>
        public static bool IsSingletonValid() => instance != null;

        /// <summary>
        /// Force the singleton instance to not be destroyed on scene load.
        /// </summary>
        public static void FlagAsPersistant() => DontDestroyOnLoad(Instance.gameObject);

        /// <summary>
        /// Finds the instance within the scene
        /// </summary>
        protected TSingletonType CreateInstance() => Instance;
    }
}