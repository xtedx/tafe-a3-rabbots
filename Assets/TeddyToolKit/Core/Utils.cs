using UnityEngine;

namespace TeddyToolKit.Core
{
    /// <summary>
    /// Essential Game menu button commands and auxiliary methods
    /// </summary>
    public class Utils : MonoBehaviour
    {

        /// <summary>
        /// convert the point where we click/tap on the screen to the world because we are not clicking
        /// an object that is actually on the screen.
        /// for 2d use nearclip plane,
        /// for 3d use screen point to ray
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector3 ScreenToWorld(Camera camera, Vector3 position)
        {
            position.z = camera.nearClipPlane;
            return camera.ScreenToWorldPoint(position);
        }
        
        /// <summary>
        /// Quit the game application, call this function from the quit button on the UI
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
        }
    }
}
