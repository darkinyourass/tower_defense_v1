using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
	private Platform _platform;
	[SerializeField] private TowerData data;
	private CircleCollider2D _circleCollider;
	private List<Enemy> _enemiesInRange;
	private ObjectPooler _projectilePool;
	private float _shootTimer;
	private SpriteRenderer _spriteRenderer;

	// ⭐ НОВЕ: Трекінг вкладених грошей
	private int _totalInvestedCost;
	public int TotalInvestedCost => _totalInvestedCost;

	// Event для UI
	public static event System.Action<Tower> OnTowerClicked;

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
		data = ScriptableObject.Instantiate(data);
		_circleCollider = GetComponent<CircleCollider2D>();
		_spriteRenderer = GetComponent<SpriteRenderer>();

		_circleCollider.radius = data.range;
		_enemiesInRange = new List<Enemy>();
		_projectilePool = GetComponent<ObjectPooler>();
		_shootTimer = data.shootInterval;

		// Ініціалізувати рівень башни
		data.currentLevel = 1;

		// ⭐ НОВЕ: Базова вартість як початкова інвестиція
		_totalInvestedCost = data.cost;
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
		if (data == null) return;
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
				AudioManager.Instance.PlaySound(data.attackSound);
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Enemy"))
		{
			Enemy enemy = collision.GetComponent<Enemy>();
			if (_enemiesInRange.Contains(enemy))
				_enemiesInRange.Remove(enemy);
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

			// ⭐ ПРАВИЛЬНА ПРІОРИТЕЗАЦІЯ: Frost спершу, потім Explosive
			if (data.damageType.HasFlag(DamageType.Frost))
			{
				FreezeProjectile freeze = projectile.GetComponent<FreezeProjectile>();
				if (freeze != null)
					freeze.Shoot(data, _shootDirection, _enemiesInRange[0].transform, damageInfo);
			}
			else if (data.damageType.HasFlag(DamageType.Explosive))
			{
				ExplosiveProjectile explosive = projectile.GetComponent<ExplosiveProjectile>();
				if (explosive != null)
					explosive.Shoot(data, _shootDirection, _enemiesInRange[0].transform, damageInfo);
			}
			else
			{
				Projectile proj = projectile.GetComponent<Projectile>();
				if (proj != null)
				{
					proj.Shoot(
						data.damage,
						0,
						data.projectileSpeed,
						data.projectileSize,
						data.projectileDuration,
						_shootDirection
					);
				}
			}
		}
	}

	/// <summary>
	/// Застосувати апгрейд до башни
	/// </summary>
	public void UpgradeTower()
	{
		if (!data.CanUpgrade())
		{
			Debug.LogWarning($"Башня {data.name} вже на максимальному рівні!");
			return;
		}

		// ⭐ НОВЕ: Додати вартість апгрейду до загальної інвестиції
		int upgradeCost = data.GetUpgradeCost();
		_totalInvestedCost += upgradeCost;

		// Застосувати нові статистики
		data.ApplyUpgradeLevelStats();

		// Оновити collider з новим радіусом
		_circleCollider.radius = data.range;

		// Поміняти спрайт якщо є
		TowerUpgradeLevel level = data.GetCurrentUpgradeLevel();
		if (level != null && level.upgradeSprite != null)
		{
			_spriteRenderer.sprite = level.upgradeSprite;
		}
	}

	/// <summary>
	/// ⭐ НОВЕ: Продати башню за 50% від загальної вартості
	/// </summary>
	public int Sell()
	{
		int refund = Mathf.RoundToInt(_totalInvestedCost * 0.5f);
		GameManager.Instance.AddResources(refund);

		Debug.Log($"🔥 Башня {data.name} продана за {refund}💰 (50% від {_totalInvestedCost}💰)");

		if (_platform != null)
		{
			_platform.OnTowerSold();
		}

		Destroy(gameObject);

		Destroy(gameObject);
		return refund;
	}

	public void SetPlatform(Platform platform)
	{
		_platform = platform;
	}


	/// <summary>
	/// Отримати дані башни
	/// </summary>
	public TowerData GetTowerData() => data;

	private void HandleEnemyDestroyed(Enemy enemy)
	{
		_enemiesInRange.Remove(enemy);
	}

	private void OnMouseDown()
	{
		OnTowerClicked?.Invoke(this);
	}

	public void Click()
	{
		OnTowerClicked?.Invoke(this);
	}
}
