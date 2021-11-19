using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles the simple movement of camera using the old input system
/// attach this to the main camera game object
/// </summary>
public class CameraMovement : MonoBehaviour
{
	public float speedX = 2;
	public float speedY = 2;
	public float cameraSpeed = 20;
	private float yaw;
	private float pitch;
	private Vector3 currentMousePos;
	
	// Start is called before the first frame update
	void Start()
	{
		yaw = transform.eulerAngles.x;
		pitch = transform.eulerAngles.y;
	}

	// Update is called once per frame
	void Update()
	{
		//if (UIManager.Instance.isMenu()) return;
		CameraRotation();
		CameraMove();
	}

	/// <summary>
	/// rotate camera while middle button is held
	/// </summary>
	private void CameraRotation()
	{
		//button is held down and not at the same spot. 1 is right, 2 is middle
		//the current mousepos check is only to prevent unnecessary computation if nothing is moved.
		if (Input.GetMouseButton(1) && (currentMousePos != Input.mousePosition))
		{
			currentMousePos = Input.mousePosition;
			yaw += speedX * Input.GetAxis("Mouse X");
			pitch -= speedY * Input.GetAxis("Mouse Y");
			transform.eulerAngles = new Vector3(pitch, yaw, 0);
		}
	}
	
	/// <summary>
	/// move camera while right button is held
	/// </summary>
	public void CameraMove()
	{
		//the current mousepos check is only to prevent unnecessary computation if nothing is moved. 1 is right, 2 is middle
		if (Input.GetMouseButton(2) && (currentMousePos != Input.mousePosition) && (Input.GetAxis("Mouse X") != 0))
		{
			currentMousePos = Input.mousePosition;
			transform.position += new Vector3(
				Input.GetAxisRaw("Mouse X") * Time.deltaTime * cameraSpeed, 0,
				Input.GetAxisRaw("Mouse Y") * Time.deltaTime * cameraSpeed);
		}
	}
}
