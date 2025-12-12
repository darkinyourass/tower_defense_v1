using UnityEngine;

public class Projectile : MonoBehaviour
{
	private float _damage;
	private int _pierceCount;
	private int _currentPierceHits;
	private float _speed;
	private float _size;
	private Vector3 _shootDirection;
	private float _projectileDuration;

	void Start()
	{
		transform.localScale = Vector3.one * _size;
	}

	void Update()
	{
		if (_projectileDuration <= 0)
		{
			gameObject.SetActive(false);
		}
		else
		{
			_projectileDuration -= Time.deltaTime;
			transform.position += _shootDirection * _speed * Time.deltaTime;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Enemy"))
		{
			Enemy enemy = collision.GetComponent<Enemy>();
			enemy.TakeDamage(_damage);

			// ✅ ПИРСИНГ
			_currentPierceHits++;

			if (_currentPierceHits > _pierceCount)
			{
				// Пробили всех, кого могли - уничтожаем снаряд
				gameObject.SetActive(false);
			}

			Debug.Log($"🎯 Попадание! Pierce: {_currentPierceHits}/{_pierceCount + 1}");
		}
	}

	public void Shoot(float damage, int pierce, float speed, float size, float duration, Vector3 shootDirection)
	{
		_damage = damage;
		_pierceCount = pierce;
		_currentPierceHits = 0;  // ✅ СБРОСИТЬ СЧЁТЧИК
		_speed = speed;
		_size = size;
		_shootDirection = shootDirection;
		_projectileDuration = duration;

		transform.localScale = Vector3.one * _size;
	}
}
