using Mirror;
using UnityEngine;

namespace Game.Scripts
{
	public class PlayerController : NetworkBehaviour
	{
		[Header("Movement Settings")]
		public float moveSpeed = 8f;
		public float dashSpeed = 3f;
		public float turnSensitivity = 5f;
		public float maxTurnSpeed = 150f;

		[Header("Diagnostics")]
		public float horizontal;
		public float vertical;
		public float turn;
		public float jumpSpeed;
		public bool isGrounded = true;
		public bool isFalling;
		public Vector3 velocity;
		
		public CharacterController characterController;

		void Start()
		{
			//only enable control if is local player
			characterController.enabled = isLocalPlayer;
		}
		
		public override void OnStartLocalPlayer()
		{
			Camera.main.orthographic = false;
			Camera.main.transform.SetParent(transform);
			Camera.main.transform.localPosition = new Vector3(0f, 3f, -8f);
			Camera.main.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
		}
		
		void OnValidate()
		{
			if (characterController == null)
				characterController = GetComponent<CharacterController>();
		}
		
		/// <summary>
		/// resets the camera when player is disabled
		/// </summary>
		void OnDisable()
		{
			if (isLocalPlayer && Camera.main != null)
			{
				Camera.main.orthographic = true;
				Camera.main.transform.SetParent(null);
				Camera.main.transform.localPosition = new Vector3(0f, 70f, 0f);
				Camera.main.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
			}
		}

		// Update is called once per frame
		void Update()
		{
			if (!isLocalPlayer)
				return;

			horizontal = Input.GetAxis("Horizontal");
			vertical = Input.GetAxis("Vertical");

			// see CameraMovement.cs for right mouse hold and rotate camera.

			if (isGrounded)
				isFalling = false;

			if ((isGrounded || !isFalling) && jumpSpeed < 1f && Input.GetKeyUp(KeyCode.Space))
			{
				jumpSpeed = Mathf.Lerp(jumpSpeed, 1f, 0.5f);
			}
			else if (!isGrounded)
			{
				isFalling = true;
				jumpSpeed = 0;
			}

		}
		
		void FixedUpdate()
		{
			if (!isLocalPlayer || characterController == null)
				return;

			transform.Rotate(0f, turn * Time.fixedDeltaTime, 0f);

			//effeciency according to rider:
			var mytransform = transform;

			Vector3 direction = new Vector3(horizontal, 0, vertical);
			direction = Vector3.ClampMagnitude(direction, 1f);
			direction = mytransform.TransformDirection(direction);
			direction *= moveSpeed;

			if (jumpSpeed > 0)
			{
				//characterController.Move(direction * Time.fixedDeltaTime);
				//dash forward to the transform face direction instead of jump
				characterController.Move(transform.forward * dashSpeed);
				//Debug.Log($"dash facing {facing}");
			}
			else
				//normal movement asdf
				characterController.SimpleMove(direction);

			isGrounded = characterController.isGrounded;
			velocity = characterController.velocity;
		}
	}
}