using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Управление XP внутри уровня
/// - Накопление XP
/// - Level up
/// - Не сохраняется между уровнями
/// </summary>
public class XPManager : MonoBehaviour
{
	public static XPManager Instance { get; private set; }

	[Header("=== LEVEL PROGRESSION ===")]
	[SerializeField] private int baseXPRequired = 100;      // XP для level 2
	[SerializeField] private float xpScaling = 1.15f;       // Множитель каждый уровень
	[SerializeField] private int maxLevel = 20;             // Макс уровень за ран

	[Header("=== CURRENT STATE ===")]
	[SerializeField] private int currentLevel = 1;
	[SerializeField] private int currentXP = 0;
	[SerializeField] private int xpRequiredForNextLevel = 100;

	public static event System.Action<int> OnXPGained;           // (amount)
	public static event System.Action<int> OnLevelUp;            // (new level)
	public static event System.Action<int, int> OnXPChanged;     // (current, required)

	private void Awake()
	{
		// Singleton
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	private void Start()
	{
		// Инициализация
		xpRequiredForNextLevel = baseXPRequired;
		OnXPChanged?.Invoke(currentXP, xpRequiredForNextLevel);
	}

	/// <summary>
	/// Добавить XP (вызывается из XPOrb)
	/// </summary>
	public void AddXP(int amount)
	{
		if (currentLevel >= maxLevel)
			return;

		currentXP += amount;
		OnXPGained?.Invoke(amount);

		Debug.Log($"💎 +{amount} XP | Total: {currentXP}/{xpRequiredForNextLevel}");

		// Проверить level up
		while (currentXP >= xpRequiredForNextLevel && currentLevel < maxLevel)
		{
			LevelUp();
		}

		OnXPChanged?.Invoke(currentXP, xpRequiredForNextLevel);
	}

	/// <summary>
	/// Level up - вызывает выбор апгрейдов
	/// </summary>
	private void LevelUp()
	{
		currentLevel++;
		currentXP -= xpRequiredForNextLevel;  // Вычесть использованный XP
		xpRequiredForNextLevel = CalculateNextLevelXP();

		Debug.Log($"⬆️ LEVEL UP! Теперь уровень {currentLevel}");

		// Вызвать событие
		OnLevelUp?.Invoke(currentLevel);

		// Получить 3 случайных апгрейда
		UpgradeSO[] upgrades = UpgradePoolManager.Instance.GetRandomUpgrades(3);

		// Логировать в консоль (временно)
		Debug.Log($"🎁 Доступные апгрейды:");
		foreach (UpgradeSO upgrade in upgrades)
		{
			Debug.Log($"  - {upgrade.upgradeName} ({upgrade.rarity})");
		}

		// Показать UI выбора (если создан)
		if (UpgradeUIManager.Instance != null)
		{
			UpgradeUIManager.Instance.ShowUpgradePanel(upgrades);
		}
	}

	/// <summary>
	/// Получить текущий уровень
	/// </summary>
	public int GetCurrentLevel() => currentLevel;

	/// <summary>
	/// Получить прогресс XP (0-1)
	/// </summary>
	public float GetXPProgress()
	{
		return (float)currentXP / xpRequiredForNextLevel;
	}

	/// <summary>
	/// Сбросить прогресс (при начале нового уровня)
	/// </summary>
	public void ResetProgress()
	{
		currentLevel = 1;
		currentXP = 0;
		xpRequiredForNextLevel = baseXPRequired;

		OnXPChanged?.Invoke(currentXP, xpRequiredForNextLevel);
	}

	/// <summary>
	/// Рассчитать XP для следующего уровня
	/// </summary>
	private int CalculateNextLevelXP()
	{
		// Формула: baseXP * (scaling ^ (level - 1))
		// Пример: 100 * (1.15 ^ 1) = 115 для level 3
		return Mathf.RoundToInt(baseXPRequired * Mathf.Pow(xpScaling, currentLevel - 1));
	}

}
