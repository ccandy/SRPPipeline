using UnityEngine;
using System.Collections;

public class Following2DViewer : MonoBehaviour
{
	#region UI Param
	public GameObject FollowObj = null;

	public Camera FollowCamera = null;

	public Vector3 FollowOffset = Vector3.back;

	public float HorizontalAngle = 0.0f;

	public float VerticalAngle = 0.0f;

	public float FollowAngleSpeed = 1.0f;
	#endregion

	Vector3 LastTargetPos = Vector3.zero;
	Vector2 LastOffset = Vector2.zero;
	Vector3 LastFollowAngle = Vector3.zero;

	Vector3 CurTargetPos = Vector3.zero;
	Vector3 CurOffset 	 = Vector3.zero;
	Vector3 CurFollowAngle = Vector3.zero;

	Quaternion CurRotation = Quaternion.identity;

	public void ResetPositionAndRotation()
	{
		LastFollowAngle.x = VerticalAngle;
		LastFollowAngle.y = HorizontalAngle;

		CurRotation.eulerAngles = LastFollowAngle;

		Vector3 curOffset = CurRotation * FollowOffset;

		if (FollowObj != null)
		{
			LastTargetPos = FollowObj.transform.position;
		}
	
		transform.SetPositionAndRotation( LastTargetPos+curOffset, CurRotation);
	}

	public void UpdatePositionAndRotation(float deltaTime)
	{
		CurFollowAngle.x = VerticalAngle;
		CurFollowAngle.y = HorizontalAngle;
	}


	// Update is called once per frame
	void Update()
	{
		ResetPositionAndRotation();
	}
}
