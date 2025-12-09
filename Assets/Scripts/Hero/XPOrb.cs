using UnityEngine;

public class XPOrb : MonoBehaviour
{
	[Header("=== XP VALUE ===")]
	[SerializeField] private int xpAmount = 10;  // Default значение

	[Header("=== PICKUP ===")]
	[SerializeField] private float pickupRadius = 1.5f;
	[SerializeField] private float magnetSpeed = 8f;

	private Transform _heroTransform;
	private bool _isBeingCollected = false;

	/// <summary>
	/// Установить количество XP (вызывается из Enemy)
	/// </summary>
	public void SetXPAmount(int amount)
	{
		xpAmount = amount;
	}

	private void Start()
	{
		Hero hero = FindObjectOfType<Hero>();
		if (hero != null)
		{
			_heroTransform = hero.transform;
		}
	}

	private void Update()
	{
		if (_heroTransform == null)
			return;

		float distanceToHero = Vector2.Distance(transform.position, _heroTransform.position);

		if (distanceToHero <= pickupRadius && !_isBeingCollected)
		{
			_isBeingCollected = true;
		}

		if (_isBeingCollected)
		{
			transform.position = Vector2.MoveTowards(
				transform.position,
				_heroTransform.position,
				magnetSpeed * Time.deltaTime
			);

			if (distanceToHero < 0.3f)
			{
				Collect();
			}
		}
	}

	private void Collect()
	{
		XPManager.Instance?.AddXP(xpAmount);
		Destroy(gameObject);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, pickupRadius);
	}
}
