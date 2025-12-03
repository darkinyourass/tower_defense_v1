using UnityEngine;

public class ExplosiveProjectile : MonoBehaviour
{
	private TowerData _data;
	private DamageInfo _damageInfo;
	private Vector3 _shootDirection;
	private float _projectileDuration;
	private Transform _target;

	[Header("Visual")]
	[SerializeField] private GameObject explosionEffectPrefab;
	[SerializeField] private LayerMask enemyMask;

	void Start()
	{
		if (_data != null)
		{
			transform.localScale = Vector3.one * _data.projectileSize;
		}
	}

	void Update()
	{
		if (_data == null)
		{
			gameObject.SetActive(false);
			return;
		}

		if (_projectileDuration <= 0)
		{
			gameObject.SetActive(false);
		}
		else
		{
			_projectileDuration -= Time.deltaTime;

			if (_target != null && _target.gameObject.activeInHierarchy)
			{
				Vector3 direction = (_target.position - transform.position).normalized;
				transform.position += direction * _data.projectileSpeed * Time.deltaTime;

				float distanceToTarget = Vector3.Distance(transform.position, _target.position);
				if (distanceToTarget < 0.2f)
				{
					Explode(transform.position);
				}
			}
			else
			{
				transform.position += new Vector3(_shootDirection.x, _shootDirection.y) * _data.projectileSpeed * Time.deltaTime;
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Enemy"))
		{
			Explode(transform.position);
		}
	}

	private void Explode(Vector3 position)
	{
		if (_data == null) return;

		if (explosionEffectPrefab != null)
		{
			GameObject explosion = Instantiate(explosionEffectPrefab, position, Quaternion.identity);
			Destroy(explosion, 2f);
		}

		Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(position, _damageInfo.AoeRadius, enemyMask);

		Debug.Log($"💥 Взрыв! Попали по {hitEnemies.Length} врагам");

		foreach (Collider2D col in hitEnemies)
		{
			Enemy enemy = col.GetComponent<Enemy>();
			if (enemy != null)
			{
				float distance = Vector3.Distance(position, enemy.transform.position);
				float damageMultiplier = 1f - (distance / _damageInfo.AoeRadius);

				DamageInfo aoeDamage = new DamageInfo(
					_damageInfo.Amount * damageMultiplier,
					_damageInfo.Type
				);
				aoeDamage.HitPosition = position;

				enemy.TakeDamage(aoeDamage);
			}
		}

		gameObject.SetActive(false);
	}

	public void Shoot(TowerData data, Vector3 shootDirection, Transform target = null, DamageInfo damageInfo = null)
	{
		_data = data;
		_shootDirection = shootDirection;
		_projectileDuration = _data.projectileDuration;
		_target = target;
		_damageInfo = damageInfo ?? new DamageInfo(_data.damage, DamageType.Physical);
	}

	private void OnDrawGizmosSelected()
	{
		if (_damageInfo != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, _damageInfo.AoeRadius);
		}
	}
}
