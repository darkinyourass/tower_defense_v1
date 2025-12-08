using UnityEngine;
using UnityEngine.InputSystem;

public class Hero : MonoBehaviour
{
	[Header("=== MOVEMENT ===")]
	[SerializeField] private float moveSpeed = 5f;
	private Vector2 _moveDirection;

	[Header("=== COMBAT ===")]
	[SerializeField] private float shootCooldown = 0.8f;
	[SerializeField] private float attackRange = 3f;
	[SerializeField] private float baseDamage = 10f;
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
		_shootTimer = shootCooldown;
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
		transform.position += (Vector3)_moveDirection * moveSpeed * Time.deltaTime;
	}

	private void HandleShooting()
	{
		_shootTimer -= Time.deltaTime;
		_targetEnemy = FindNearestEnemy();

		if (_targetEnemy != null && _shootTimer <= 0)
		{
			ShootAt(_targetEnemy);
			_shootTimer = shootCooldown;
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
			// Создать TowerData для героя
			TowerData heroData = new TowerData();
			heroData.damage = baseDamage;
			heroData.projectileSize = 0.5f;
			heroData.projectileSpeed = 8f;
			heroData.projectileDuration = 5f;

			// Запустить снаряд
			projectile.Shoot(heroData, (Vector3)direction);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRange);
	}
}
