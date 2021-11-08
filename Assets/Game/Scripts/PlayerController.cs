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
		public Vector3 direction;
		public Vector3 velocity;
		
		public CharacterController characterController;
		private GameObject playerChildGameObject;

		void Start()
		{
			//only enable control if is local player
			characterController.enabled = isLocalPlayer;
			//get the actual model to turn when moving
			playerChildGameObject = gameObject.GetComponent<NetworkPlayer>().playerChildGameObject;
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

			//don't dash when there is no input
			if (horizontal == 0 && vertical == 0) jumpSpeed = 0;
			
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

			var rotation = new Vector3(0f, turn * Time.fixedDeltaTime, 0f);
			transform.Rotate(rotation);
			

			//effeciency according to rider:
			var mytransform = transform;

			Vector3 direction = new Vector3(horizontal, 0, vertical);
			direction = Vector3.ClampMagnitude(direction, 1f);
			direction = mytransform.TransformDirection(direction);
			direction *= moveSpeed;

			//use atan x/y because we are facing positive y, standard atan y/x starts the angle 0deg from positive x if drawn on cartesian
			//atan is in rad, and convert to deg by multiplying
			//use z instead of y because we don't move up
			float turnTo = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
			
			if (jumpSpeed > 0)
			{
				//characterController.Move(direction * Time.fixedDeltaTime);
				//dash forward to the transform face direction instead of jump
				// characterController.SimpleMove(transform.forward * dashSpeed);
				characterController.SimpleMove(direction * dashSpeed);
				//Debug.Log($"dash facing {facing}");
			}
			else
			{
				//normal movement asdf
				characterController.SimpleMove(direction);
				playerChildGameObject.transform.rotation = Quaternion.Euler(0,turnTo,0);
			}


			isGrounded = characterController.isGrounded;
			velocity = characterController.velocity;
			this.direction = direction;
		}
	}
}