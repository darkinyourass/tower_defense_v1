using UnityEngine;
using UnityEngine.InputSystem;

public class Hero : MonoBehaviour
{
	[Header("=== MOVEMENT ===")]
	// Скорость берётся из HeroStats!
	private Vector2 _moveDirection;

	[Header("=== COMBAT ===")]
	[SerializeField] private float attackRange = 3f;
	[SerializeField] private DamageType damageType = DamageType.Physical;

	private float _shootTimer;
	private Enemy _targetEnemy;
	private ObjectPooler _projectilePool;

	private void Start()
	{
		_projectilePool = GetComponent<ObjectPooler>();
		if (_projectilePool == null)
		{
			Debug.LogError("❌ ObjectPooler не найден на Hero!");
			return;
		}

		Debug.Log("🦸 Hero инициализирован!");
		_shootTimer = 0f;
	}

	private void Update()
	{
		HandleMovement();
		HandleShooting();
	}

	private void HandleMovement()
	{
		_moveDirection = Vector2.zero;

		if (Keyboard.current == null)
			return;

		if (Keyboard.current.wKey.isPressed) _moveDirection.y += 1;
		if (Keyboard.current.sKey.isPressed) _moveDirection.y -= 1;
		if (Keyboard.current.aKey.isPressed) _moveDirection.x -= 1;
		if (Keyboard.current.dKey.isPressed) _moveDirection.x += 1;

		_moveDirection = _moveDirection.normalized;

		// ✅ ИСПОЛЬЗУЕМ СКОРОСТЬ ИЗ СТАТОВ!
		float currentSpeed = HeroStats.Instance.GetMoveSpeed();
		transform.position += (Vector3)_moveDirection * currentSpeed * Time.deltaTime;
	}

	private void HandleShooting()
	{
		_shootTimer -= Time.deltaTime;
		_targetEnemy = FindNearestEnemy();

		if (_targetEnemy != null && _shootTimer <= 0)
		{
			ShootAt(_targetEnemy);

			// ✅ ИСПОЛЬЗУЕМ ATTACK SPEED ИЗ СТАТОВ!
			float attackSpeed = HeroStats.Instance.GetAttackSpeed();
			_shootTimer = 1f / attackSpeed;
		}
	}

	private Enemy FindNearestEnemy()
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
		Enemy nearest = null;
		float nearestDistance = float.MaxValue;

		foreach (Collider2D hit in hits)
		{
			if (hit.CompareTag("Enemy"))
			{
				Enemy enemy = hit.GetComponent<Enemy>();
				if (enemy != null && enemy.isActiveAndEnabled)
				{
					float distance = Vector2.Distance(transform.position, enemy.transform.position);
					if (distance < nearestDistance)
					{
						nearestDistance = distance;
						nearest = enemy;
					}
				}
			}
		}

		return nearest;
	}

	private void ShootAt(Enemy target)
	{
		if (_projectilePool == null) return;

		GameObject projectileObj = _projectilePool.GetPooledObject();
		if (projectileObj == null)
		{
			Debug.LogWarning("⚠️ Нет снарядов в пуле!");
			return;
		}

		projectileObj.transform.position = transform.position;
		projectileObj.SetActive(true);

		Vector2 direction = (target.transform.position - transform.position).normalized;
		Projectile projectile = projectileObj.GetComponent<Projectile>();

		if (projectile != null)
		{
			// ✅ ИСПОЛЬЗУЕМ DAMAGE ИЗ СТАТОВ!
			TowerData heroData = new TowerData();
			heroData.damage = HeroStats.Instance.GetDamage();
			heroData.projectileSize = 0.5f;
			heroData.projectileSpeed = 8f;
			heroData.projectileDuration = 5f;

			projectile.Shoot(heroData, (Vector3)direction);

			Debug.Log($"💥 Выстрел! Урон: {heroData.damage}");
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRange);
	}
}
