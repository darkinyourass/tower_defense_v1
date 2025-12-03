using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private TowerData data;
    private CircleCollider2D _circleCollider;

    private List<Enemy> _enemiesInRange;
    private ObjectPooler _projectilePool;

    private float _shootTimer;

    private void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    private void Start()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _circleCollider.radius = data.range;
        _enemiesInRange = new List<Enemy>();
        _projectilePool = GetComponent<ObjectPooler>();
        _shootTimer = data.shootInterval;
    }

    private void Update()
    {
        _shootTimer -= Time.deltaTime;
        if (_shootTimer <= 0)
        {
            _shootTimer = data.shootInterval;
            Shoot();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, data.range);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            bool _wasEmpty = _enemiesInRange.Count == 0;
            Enemy enemy = collision.GetComponent<Enemy>();
            _enemiesInRange.Add(enemy);

            if (_wasEmpty && data.attackSound != null)
            {
                AudioManager.Instance.PlaySound(data.attackSound);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (_enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Remove(enemy);
            }
        }
    }

	private void Shoot()
	{
		_enemiesInRange.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);

		if (_enemiesInRange.Count > 0)
		{
			GameObject projectile = _projectilePool.GetPooledObject();
			projectile.transform.position = transform.position;
			projectile.SetActive(true);

			Vector2 _shootDirection = (_enemiesInRange[0].transform.position - transform.position).normalized;

			DamageInfo damageInfo = new DamageInfo(data.damage, data.damageType);
			damageInfo.HitPosition = transform.position;
			damageInfo.AoeRadius = data.aoeRadius;
			damageInfo.SlowAmount = data.slowAmount;
			damageInfo.SlowDuration = data.debuffDuration;
			damageInfo.DotDamagePerSecond = data.dotDamagePerSecond;
			damageInfo.DotDuration = data.debuffDuration;
			damageInfo.ChainBounces = data.chainBounces;
			damageInfo.ChainDamageFalloff = data.chainDamageFalloff;

			if (data.damageType.HasFlag(DamageType.Explosive))
			{
				ExplosiveProjectile explosive = projectile.GetComponent<ExplosiveProjectile>();
				if (explosive != null)
				{
					explosive.Shoot(data, _shootDirection, _enemiesInRange[0].transform, damageInfo);
				}
			}
			else
			{
				Projectile proj = projectile.GetComponent<Projectile>();
				if (proj != null)
				{
					proj.Shoot(data, _shootDirection);
				}
			}
		}
	}

	private void HandleEnemyDestroyed(Enemy enemy)
    {
        _enemiesInRange.Remove(enemy);
    }
}
