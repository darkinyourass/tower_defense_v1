using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	[Header("Target Settings")]
	[Tooltip("Об'єкт, за яким слідкуватиме камера. Якщо NULL - шукатиме по тегу")]
	public Transform target;

	[Tooltip("Тег для автоматичного пошуку героя")]
	public string heroTag = "Player";

	[Header("Follow Settings")]
	[Tooltip("Зміщення камери відносно персонажа")]
	public Vector3 offset = new Vector3(0f, 5f, -10f);

	[Tooltip("Швидкість згладжування руху (0 = миттєво, більше = плавніше)")]
	[Range(0f, 1f)]
	public float smoothSpeed = 0.125f;

	[Tooltip("Використовувати згладжування руху")]
	public bool useSmoothFollow = true;

	[Header("Camera Bounds (Optional)")]
	[Tooltip("Обмежити рух камери в межах")]
	public bool useBounds = false;
	public Vector2 minBounds = new Vector2(-50f, -50f);
	public Vector2 maxBounds = new Vector2(50f, 50f);

	[Header("Look At Settings")]
	[Tooltip("Камера дивиться на ціль замість простого слідкування")]
	public bool lookAtTarget = false;

	[Header("Debug")]
	[SerializeField] private bool showDebugInfo = true;

	private void Start()
	{
		FindHero();
	}

	private void FindHero()
	{
		// Якщо target вже встановлений в Inspector - не шукаємо
		if (target != null)
		{
			if (showDebugInfo) Debug.Log($"✅ Camera target встановлений вручну: {target.name}");
			return;
		}

		// Шукаємо героя по тегу
		GameObject hero = GameObject.FindGameObjectWithTag(heroTag);

		if (hero != null)
		{
			target = hero.transform;
			if (showDebugInfo) Debug.Log($"✅ Камера знайшла героя: {hero.name}");

			// Миттєво переміщуємо камеру до героя при старті
			SnapToTarget();
		}
		else
		{
			Debug.LogWarning($"⚠️ Герой з тегом '{heroTag}' не знайдений! Перевір, чи є тег у героя.");
		}
	}

	void LateUpdate()
	{
		// Якщо героя все ще немає - спробуємо знайти
		if (target == null)
		{
			FindHero();
			return;
		}

		FollowTarget();
	}

	void FollowTarget()
	{
		// Розраховуємо бажану позицію камери
		Vector3 desiredPosition = target.position + offset;

		// Обмежуємо позицію, якщо потрібно
		if (useBounds)
		{
			desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
			desiredPosition.z = Mathf.Clamp(desiredPosition.z, minBounds.y, maxBounds.y);
		}

		// Застосовуємо згладжування або миттєве переміщення
		if (useSmoothFollow)
		{
			transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
		}
		else
		{
			transform.position = desiredPosition;
		}

		// Опціонально: камера дивиться на ціль
		if (lookAtTarget)
		{
			transform.LookAt(target);
		}
	}

	/// <summary>
	/// Встановити нову ціль для слідкування
	/// </summary>
	public void SetTarget(Transform newTarget)
	{
		target = newTarget;
		if (showDebugInfo) Debug.Log($"✅ Camera target змінено на: {newTarget.name}");
	}

	/// <summary>
	/// Миттєво перемістити камеру до цілі (без згладжування)
	/// </summary>
	public void SnapToTarget()
	{
		if (target != null)
		{
			transform.position = target.position + offset;
			if (lookAtTarget)
			{
				transform.LookAt(target);
			}
			if (showDebugInfo) Debug.Log("📷 Камера миттєво перемістилась до героя");
		}
	}
}
