using UnityEngine;

public class FreezeProjectile : MonoBehaviour
{
	private TowerData _data;
	private DamageInfo _damageInfo;
	private Vector3 _shootDirection;
	private float _projectileDuration;
	private Transform _target;

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
					HitTarget();
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
			HitTarget();
		}
	}

	private void HitTarget()
	{
		if (_data == null) return;

		Enemy enemy = _target?.GetComponent<Enemy>();
		if (enemy != null)
		{
			enemy.TakeDamage(_damageInfo);
			Debug.Log($"❄️ Заморозка! Враг замедлен на {_damageInfo.SlowAmount * 100}%");
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
}
