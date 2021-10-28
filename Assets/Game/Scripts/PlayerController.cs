using UnityEngine;

namespace Game.Scripts
{
	public class PlayerController : MonoBehaviour
	{
		[SerializeField] private float speed = 5f;

		// Update is called once per frame
		private void Update()
		{
			transform.position += transform.right * Time.deltaTime * speed * Input.GetAxis("Horizontal");
			transform.position += transform.forward * Time.deltaTime * speed * Input.GetAxis("Vertical");
		}
	}
}