using System;
using Mirror;
using UnityEngine;

namespace Game.Scripts
{
	/// <summary>
	/// player controller, for the character
	/// </summary>
	public class PlayerController : NetworkBehaviour
	{
		/// <summary>
		/// built in character controller object from unity
		/// </summary>
		[SerializeField] private CharacterController controller;
		[SerializeField] public NetworkPlayer netPlayer;

		/// <summary>
		/// the actual model object to rotate
		/// </summary>
		private GameObject playerChildGameObject;
		private Camera mainCamera;
		private AudioManager _audioManager;

		[Space]
    
		[Header("Movement Settings")]
		/// <summary>
		/// speed of the character
		/// </summary>
		[SerializeField] private float speed;
		/// <summary>
		/// how high can the player jump
		/// </summary>
		[SerializeField] private float jumpForce;
		/// <summary>
		/// how far can the player dash
		/// </summary>
		[SerializeField] private float dashForce;
		/// <summary>
		/// dash cooldown time
		/// </summary>
		[SerializeField] private float dashCooldown = 1;
		/// <summary>
		/// how long is the speed boost for
		/// </summary>
		[SerializeField] private float dashModeDuration = 0.2f;
		/// <summary>
		/// gravity to simulate player falling
		/// </summary>
		[SerializeField] private float gravity = 9.81f;
		/// <summary>
		/// how long does it take to turn face
		/// </summary>
		[SerializeField] private float turnSmoothTime = 0.5f;
		[SerializeField] private bool isTurnSmooth;
		[SerializeField] private bool isSimpleMove;
		[Space]
		[Header("Diagnostic")]
		[SerializeField] private bool canDash = true;
		[SerializeField] public bool isDashing = false;
		
		
		/// <summary>
		/// dash cooldown timer
		/// </summary>
		[SerializeField] private float dashCooldownTimer = 1;
		[SerializeField] private float hitCooldownTimer = 1;
		[SerializeField] private float dashModeTimer;
		[SerializeField] private float originalSpeed;
		
		
		
		
		private float turnSmoothVelocity; // to hold temp value for angle smoothing

		[Header("Diagnostics")] public Vector3 direction;
		
		/// <summary>
		/// speed of character falling in air
		/// </summary>
		private Vector3 verticalVelocity;
    
		private Vector3 playerKeyboardInput;
    
		private Animator animator;
		private float animationBlend;
    
		// Start is called before the first frame update
		void Start()
		{
			netPlayer = gameObject.GetComponent<NetworkPlayer>();
			_audioManager = GameObject.FindObjectOfType<AudioManager>();
			//only enable control if is local player
			controller.enabled = isLocalPlayer;
			//get the actual model to turn when moving
			playerChildGameObject = netPlayer.playerChildGameObject;
			//keep the original speed to revert after dash boost
			originalSpeed = speed;
			
			animator = GetComponentInChildren<Animator>();
		}
    
		public override void OnStartLocalPlayer()
		{
			mainCamera = Camera.main;
			mainCamera.orthographic = false;
			mainCamera.transform.SetParent(transform);
			mainCamera.transform.localPosition = new Vector3(0f, 3f, -8f);
			mainCamera.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
		}
		void OnValidate()
		{
			if (controller == null)
				controller = GetComponent<CharacterController>();
		}
		
		/// <summary>
		/// resets the camera when player is disabled
		/// </summary>
		void OnDisable()
		{
			if (isLocalPlayer && mainCamera != null)
			{
				mainCamera.orthographic = true;
				mainCamera.transform.SetParent(null);
				mainCamera.transform.localPosition = new Vector3(0f, 70f, 0f);
				mainCamera.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
			}
		}
    
		// Update is called once per frame
		void Update()
		{
			//only process if this is the local player, not for other clients
			if (!isLocalPlayer)
				return;
	    
			movePlayer();
			//reduce hitCooldown
			if (hitCooldownTimer > 0) hitCooldownTimer -= Time.deltaTime;

		}

		/// <summary>
		/// reads input from user to move the player
		/// </summary>
		private void movePlayer()
		{
			//normalise so that it does not get too fast when moving diagonally
			//basically normalise the vector to a direction, or use .normalize
			//Vector3 direction = transform.TransformDirection(playerKeyboardInput);
        
			//playerKeyboardInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
			playerKeyboardInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			direction = playerKeyboardInput.normalized;

			#region dash logic

			if (!isDashing)
			{
				if (Input.GetKeyDown(KeyCode.Space) && canDash)
				{
					//amplify the speed of movement for a period of time
					// Debug.Log("dash = speed boost!");
					speed *= dashForce;
					isDashing = true;
					canDash = false;
					dashModeTimer = dashModeDuration;
					startAnimation(2);
					_audioManager.playRunFx(false);
					_audioManager.playDashFx(true);
					netPlayer.CmdPlayerDashStart();
					return;
				}
			}
			else
			{
				//reduce timer
				dashModeTimer -= Time.deltaTime;

				//reset if time is up
				if (dashModeTimer <= 0)
				{
					isDashing = false;
					speed = originalSpeed;
					startAnimation(0);
					_audioManager.playDashFx(false);
					// Debug.Log("dash is complete");
				}
			}


			#endregion dash logic
			
			#region jump logic
			if (controller.isGrounded)
			{
				verticalVelocity.y = -1f;
				//only jump when grounded
				if (Input.GetKeyDown(KeyCode.J))
				{
					verticalVelocity.y = jumpForce;
				}
			}
			else
			{
				//standard physics vertical velocity formula
				//reduce vertical position by gravity at every frame
				verticalVelocity.y -= gravity * 2f * Time.deltaTime;
			}
			controller.Move(verticalVelocity * Time.deltaTime);
			#endregion jump logic
			
			#region normal move logic
			//move only if there is input
			if (direction.magnitude < 0.1f)
			{
				startAnimation(0);
				_audioManager.playRunFx(false);
				_audioManager.playDashFx(false);
				return;
			}

			direction = turnTo(direction, isTurnSmooth);
			controller.Move(direction * speed * Time.deltaTime);

			startAnimation(1);
			_audioManager.playRunFx(true);

			#endregion normal move logic
			
			#region cool down logic
			if (!canDash)
			{
				//reduce timer
				dashCooldownTimer -= Time.deltaTime;

				//reset if time is up
				if (dashCooldownTimer <= 0)
				{
					canDash = true;
					dashCooldownTimer = dashCooldown;
					// Debug.Log("can dash again");
				}
			}
			#endregion
			
			//blendAnimation();
		}

		private void startAnimation(int mode)
		{
			if (mode == 0) //idle
			{
				animator.SetFloat(Animator.StringToHash("speed"), 0);
			}
			else //run or dash
			{
				animator.SetFloat(Animator.StringToHash("speed"), speed);
			}
			//testing different way to animate. aparently there is a mirror bug causing animation not work before network client is connected
			// if (mode == 0)
			// {
			// 	animator.Play("Idle");
			// }
			// else if (mode == 1)
			// {
			// 	animator.Play("Run");
			// }
			// else if (mode == 2)
			// {
			// 	animator.Play("Dash");
			// }

		}

		private void blendAnimation()
		{
			//lerp to the full speed for nice blending, but doesn't seem to work well for me
			animationBlend = Mathf.Lerp(animationBlend, speed, Time.deltaTime);
			animator.SetFloat(Animator.StringToHash("speed"), animationBlend);
		}

		/// <summary>
		/// make the character turn to a given direction
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="isSmooth"></param>
		/// <returns></returns>
		private Vector3 turnTo(Vector3 direction, bool isSmooth)
		{
			//use atan x/y because we are facing positive y, standard atan y/x starts the angle 0deg from positive x if drawn on cartesian
			//atan is in rad, and convert to deg by multiplying
			//use z instead of y because we don't move up
			float turnTo = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
			//make character go forward to the angle where main camera is facing.
			turnTo += mainCamera.transform.eulerAngles.y;

			if (isSmooth)
			{
				//smooth the angle to make it anime nicely
				turnTo =
					Mathf.SmoothDampAngle(playerChildGameObject.transform.eulerAngles.y, turnTo, ref turnSmoothVelocity, turnSmoothTime);

			}
			//turn object
			playerChildGameObject.transform.rotation = Quaternion.Euler(0, turnTo, 0);
        
			//to move, now we need to convert the angle to a direction
			direction = Quaternion.Euler(0, turnTo, 0) * Vector3.forward;
			return direction.normalized;
		}
		
		#region collisions

		/// <summary>
		/// oncollisionenter does not work for character controller class, use this instead.
		/// only the hit on the local player is
		/// </summary>
		/// <param name="hit"></param>
		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			if (hit.gameObject.CompareTag("Player"))
			{
				//prevents multiple hit
				if (hitCooldownTimer > 0) return;
				hitCooldownTimer = 0.5f;
				_audioManager.playHitFx(true);
				
				//call server command to decide who wins the hit
				var enemy = hit.gameObject.GetComponent<PlayerController>();
				netPlayer.CmdDecidePlayerCollision(isDashing, netPlayer.playerDashTime, enemy.isDashing, enemy.netPlayer.playerDashTime, enemy.netId);
				Debug.Log("called CmdDecidePlayerCollision");
			}
			else if (hit.gameObject.CompareTag("environment"))
			{
				if (hit.gameObject.name.StartsWith("Mattress") || hit.gameObject.name.StartsWith("Tire"))
				{
					//prevents multiple hit
					if (hitCooldownTimer > 0) return;
					hitCooldownTimer = 0.5f;
					
					//Debug.Log($"controller colliderhit with something else to animate {hit.gameObject.name}");
					var a = hit.gameObject.GetComponent<AnimatedProp>();
					a.Animate();
				}
			}
		}
		
		#endregion
	}
}