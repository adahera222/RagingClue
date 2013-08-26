using UnityEngine;

public class EasePosition : MonoBehaviour {
	public Vector3[] positions;

	public Quaternion[] rotations;

	Vector3 targetPosition;
	Quaternion targetRotation;

    float lastChangeTime = 0.0f;
	float nextChangeTime = 10.0f;
	int i = 0;

	public static EasePosition current;

	public float speed;

	void Awake() {
		current = this;

		targetPosition = positions[0];
		targetRotation = rotations[0];
	}

	void Update () {
		if ( Time.time > nextChangeTime ) {
			Change();
		}

		// Double lerp for great smooveness

		targetPosition = Vector3.Lerp(targetPosition, positions[i], Time.deltaTime * speed);
		targetRotation = Quaternion.Slerp(targetRotation, rotations[i], Time.deltaTime * speed);

		transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
	}

	public void Change() {
		if ( lastChangeTime > Time.time - 9.9f ) return;

		int next = Random.Range(0, positions.Length);
		if ( next != i ) {
			lastChangeTime = Time.time;
			nextChangeTime = Time.time + 10.0f;
			i = next;
		}
	}
}
