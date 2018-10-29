using UnityEngine;

public class TempTower : MonoBehaviour {

	public TempBlock[] blocks;
	public float distanceBetween = 2f;
	public float sortSpeed = 0.5f;

	void Update()
	{
		float height = 1f;
		for (int i = 0; i < blocks.Length; i++)
		{
			blocks[i].transform.position = Vector3.Lerp(
				blocks[i].transform.position,
				new Vector3(transform.position.x, height, transform.position.z),
				sortSpeed * Time.deltaTime
				);

			height += 2f;
		}
	}
}
