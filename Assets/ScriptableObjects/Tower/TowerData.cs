using UnityEngine;

[System.Serializable]
public class TowerUpgradeLevel
{
	[Header("Stats Multipliers")]
	[Range(0.8f, 2f)]
	public float damageMultiplier = 1.1f;           // На 10% більше урону за рівень

	[Range(0.8f, 2f)]
	public float fireRateMultiplier = 1.05f;        // На 5% швидше

	[Range(0.8f, 2f)]
	public float rangeMultiplier = 1.05f;           // На 5% більший радіус

	[Header("Visual")]
	public Sprite upgradeSprite;                    // Спрайт башни на цьому рівні

	[Header("Cost")]
	public int upgradeCost = 100;                   // Вартість апгрейду ТА цього рівня

	[Header("Special Effect (Tier 5)")]
	public bool isUltimateUpgrade = false;          // Чи це спеціальний апгрейд на 5 рівні

	[Tooltip("На рівні 5 ледяна башня отримує AOE")]
	public bool unlocksAOE = false;                 // Розблокувати AOE

	public bool unlocksChain = false;               // Розблокувати Chain Lightning

	public bool unlocksDOT = false;                 // Розблокувати DoT
}

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
	[Header("Base Stats")]
	public float range = 5f;
	public float shootInterval = 1f;
	public float projectileSpeed = 10f;
	public float projectileDuration = 3f;
	public float projectileSize = 0.5f;
	public float damage = 10f;

	public int cost = 100;
	public Sprite sprite;
	public GameObject prefab;
	public AudioClip attackSound;

	[Header("Damage Type")]
	public DamageType damageType = DamageType.Physical;

	[Header("Special Effects")]
	[Tooltip("AOE радіус для Explosive")]
	public float aoeRadius = 2f;

	[Tooltip("% замедления для Frost (0.5 = 50%)")]
	[Range(0f, 1f)]
	public float slowAmount = 0.5f;

	[Tooltip("Тривалість дебафу (сек)")]
	public float debuffDuration = 3f;

	[Tooltip("DoT урон в секунду")]
	public float dotDamagePerSecond = 0.5f;

	[Tooltip("Кількість стрибків Chain Lightning")]
	public int chainBounces = 3;

	[Tooltip("% урону на кожний стрибок")]
	[Range(0f, 1f)]
	public float chainDamageFalloff = 0.5f;

	[Header("Upgrade System")]
	[SerializeField] private TowerUpgradeLevel[] upgradeLevels = new TowerUpgradeLevel[5];

	// Runtime данные
	[HideInInspector] public int currentLevel = 1; // Поточний рівень башни (1-5)

	/// <summary>
	/// Отримати дані апгрейду для поточного рівня
	/// </summary>
	public TowerUpgradeLevel GetCurrentUpgradeLevel()
	{
		int index = currentLevel - 1; // Рівні: 1-5, індекс: 0-4
		if (index >= 0 && index < upgradeLevels.Length)
			return upgradeLevels[index];
		return null;
	}

	/// <summary>
	/// Отримати дані апгрейду для наступного рівня
	/// </summary>
	public TowerUpgradeLevel GetNextUpgradeLevel()
	{
		int nextLevel = currentLevel + 1;
		if (nextLevel <= 5)
		{
			int index = nextLevel - 1;
			if (index >= 0 && index < upgradeLevels.Length)
				return upgradeLevels[index];
		}
		return null;
	}

	/// <summary>
	/// Отримати вартість апгрейду на наступний рівень
	/// </summary>
	public int GetUpgradeCost()
	{
		TowerUpgradeLevel next = GetNextUpgradeLevel();
		return next != null ? next.upgradeCost : 0;
	}

	/// <summary>
	/// Чи можна апгрейдити башню ще
	/// </summary>
	public bool CanUpgrade()
	{
		return currentLevel < 5;
	}

	/// <summary>
	/// Застосувати апгрейд - змінити статистику
	/// </summary>
	public void ApplyUpgradeLevelStats()
	{
		if (!CanUpgrade()) return;

		TowerUpgradeLevel nextLevel = GetNextUpgradeLevel();
		if (nextLevel != null)
		{
			// Помножити статистику на мультиплікатори
			damage *= nextLevel.damageMultiplier;
			shootInterval *= (1f / nextLevel.fireRateMultiplier);
			range *= nextLevel.rangeMultiplier;

			// Застосувати спеціальні ефекти 5-го рівня
			if (nextLevel.isUltimateUpgrade)
			{
				if (nextLevel.unlocksAOE)
				{
					aoeRadius = Mathf.Max(aoeRadius, 3f); // Мінімум 3f AOE
					damageType |= DamageType.Explosive;   // Додати флаг Explosive
				}

				if (nextLevel.unlocksChain)
				{
					damageType |= DamageType.Lightning;
					chainBounces = Mathf.Max(chainBounces, 5);
				}

				if (nextLevel.unlocksDOT)
				{
					damageType |= DamageType.Fire;
					dotDamagePerSecond = Mathf.Max(dotDamagePerSecond, 2f);
				}
			}

			currentLevel++;

			Debug.Log($"?? {name} апгрейдена до рівня {currentLevel}! " +
					  $"Урон: {damage:F1}, Радіус: {range:F1}");
		}
	}
}
