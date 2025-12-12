using UnityEngine;

/// <summary>
/// Статы героя (изменяются апгрейдами)
/// </summary>
public class HeroStats : MonoBehaviour
{
	public static HeroStats Instance { get; private set; }

	[Header("=== BASE STATS ===")]
	[SerializeField] private float baseDamage = 10f;
	[SerializeField] private float baseAttackSpeed = 1f;
	[SerializeField] private float baseMoveSpeed = 5f;
	[SerializeField] private float baseRange = 10f;
	[SerializeField] private int baseProjectileCount = 1;
	[SerializeField] private int basePierceCount = 0;

	[Header("=== CURRENT STATS ===")]
	private float _currentDamage;
	private float _currentAttackSpeed;
	private float _currentMoveSpeed;
	private float _currentRange;
	private int _currentProjectileCount;
	private int _currentPierceCount;

	[Header("=== MODIFIERS ===")]
	private float _damageMultiplier = 1f;
	private float _attackSpeedMultiplier = 1f;
	private float _moveSpeedMultiplier = 1f;
	private float _rangeMultiplier = 1f;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;

		// Инициализация статов
		ResetStats();
	}

	/// <summary>
	/// Сбросить статы до базовых значений
	/// </summary>
	public void ResetStats()
	{
		_currentDamage = baseDamage;
		_currentAttackSpeed = baseAttackSpeed;
		_currentMoveSpeed = baseMoveSpeed;
		_currentRange = baseRange;
		_currentProjectileCount = baseProjectileCount;
		_currentPierceCount = basePierceCount;

		_damageMultiplier = 1f;
		_attackSpeedMultiplier = 1f;
		_moveSpeedMultiplier = 1f;
		_rangeMultiplier = 1f;

		Debug.Log($"📊 Статы героя сброшены");
	}

	/// <summary>
	/// Применить модификацию стата
	/// </summary>
	public void ApplyStatModification(StatModification mod)
	{
		switch (mod.statType)
		{
			case StatType.Damage:
				if (mod.modificationType == ModificationType.Flat)
					_currentDamage += mod.value;
				else
					_damageMultiplier += mod.value;
				break;

			case StatType.AttackSpeed:
				if (mod.modificationType == ModificationType.Flat)
					_currentAttackSpeed += mod.value;
				else
					_attackSpeedMultiplier += mod.value;
				break;

			case StatType.MoveSpeed:
				if (mod.modificationType == ModificationType.Flat)
					_currentMoveSpeed += mod.value;
				else
					_moveSpeedMultiplier += mod.value;
				break;

			case StatType.Range:
				if (mod.modificationType == ModificationType.Flat)
					_currentRange += mod.value;
				else
					_rangeMultiplier += mod.value;
				break;

			case StatType.ProjectileCount:
				_currentProjectileCount += (int)mod.value;
				break;

			case StatType.PierceCount:
				_currentPierceCount += (int)mod.value;
				break;
		}

		Debug.Log($"📈 Стат изменён: {mod.statType} {(mod.modificationType == ModificationType.Flat ? "+" : "+%")}{mod.value}");
	}

	// === GETTERS ===

	public float GetDamage() => _currentDamage * _damageMultiplier;
	public float GetAttackSpeed() => _currentAttackSpeed * _attackSpeedMultiplier;
	public float GetMoveSpeed() => _currentMoveSpeed * _moveSpeedMultiplier;
	public float GetRange() => _currentRange * _rangeMultiplier;
	public int GetProjectileCount() => _currentProjectileCount;
	public int GetPierceCount() => _currentPierceCount;

	/// <summary>
	/// Вывести все статы в консоль (для дебага)
	/// </summary>
	public void LogStats()
	{
		Debug.Log($"📊 СТАТЫ ГЕРОЯ:\n" +
				  $"Damage: {GetDamage():F1} (base: {baseDamage}, x{_damageMultiplier:F2})\n" +
				  $"AttackSpeed: {GetAttackSpeed():F2} (base: {baseAttackSpeed}, x{_attackSpeedMultiplier:F2})\n" +
				  $"MoveSpeed: {GetMoveSpeed():F1} (base: {baseMoveSpeed}, x{_moveSpeedMultiplier:F2})\n" +
				  $"Range: {GetRange():F1} (base: {baseRange}, x{_rangeMultiplier:F2})\n" +
				  $"Projectiles: {GetProjectileCount()}\n" +
				  $"Pierce: {GetPierceCount()}");
	}
}
