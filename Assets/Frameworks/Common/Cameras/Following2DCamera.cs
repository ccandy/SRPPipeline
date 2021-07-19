using UnityEngine;

public class Following2DCamera : MonoBehaviour
{
	public Transform Target;

	[Header ("Follow Settings")]
	public float distanceToTarget = 5;	// The distance to the target
	public float heightOffset = 5;      // the height offset of the camera relative to it's target

	public float viewAngle = 10;		// A downwards rotation

	public Vector3 AdditionalOffset;	// any additional offset
	public bool FollowZAxis;			// enable or disable the camera following the z axis

	[Header ("Damp Settings")]
	public float DampX = 3f;
	public float DampY = 3f;
	public float DampZ = 3f;

	[Header ("View Area")]
	public float MinLeft;
	public float MaxRight;

	[Header ("Wave Area collider")]
	public bool UseWaveAreaCollider;
	public BoxCollider CurrentAreaCollider;
	public float AreaColliderViewOffset;

	Quaternion m_ViewQuaternion = Quaternion.identity;

	void Start()
	{

		//set camera start position
		if (Target)
		{
			Vector3 playerPos = Target.transform.position;
			transform.position = new Vector3(playerPos.x, playerPos.y - heightOffset, playerPos.z + (distanceToTarget));
		}
	}

	void Update ()
	{
		//initial values
		float currentX = transform.position.x;
		float currentY = transform.position.y;
		float currentZ = transform.position.z;
		Vector3 playerPos = Target.transform.position;

		//Damp X
		currentX = Mathf.Lerp(currentX, playerPos.x, DampX * Time.deltaTime);

		//DampY
		currentY = Mathf.Lerp(currentY, playerPos.y - heightOffset, DampY * Time.deltaTime);

		//DampZ
		if (FollowZAxis)
		{ 
			currentZ = Mathf.Lerp (currentZ, playerPos.z + distanceToTarget, DampZ * Time.deltaTime);
		}
		else
		{
			currentZ = distanceToTarget;
		}

		//Set cam position
		if(CurrentAreaCollider == null) UseWaveAreaCollider = false;
		if (!UseWaveAreaCollider)
		{
			transform.position = new Vector3 (Mathf.Clamp (currentX, MaxRight, MinLeft), currentY, currentZ) + AdditionalOffset;
		}
		else
		{
			transform.position = new Vector3 (Mathf.Clamp (currentX, CurrentAreaCollider.transform.position.x + AreaColliderViewOffset, MinLeft), currentY, currentZ) + AdditionalOffset;
		}

		//Set cam rotation
		transform.rotation = Quaternion.Euler( viewAngle, 0.0f, 0.0f);
		
	}
}