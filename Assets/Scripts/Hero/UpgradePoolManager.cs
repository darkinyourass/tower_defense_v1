using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Управление pool апгрейдов
/// - Weighted random по rarity
/// - Выбор 3 случайных апгрейдов при level up
/// </summary>
public class UpgradePoolManager : MonoBehaviour
{
	public static UpgradePoolManager Instance { get; private set; }

	[Header("=== UPGRADE POOL ===")]
	[SerializeField] private UpgradeSO[] allUpgrades;  // Все доступные апгрейды

	[Header("=== RARITY WEIGHTS ===")]
	[SerializeField] private float commonWeight = 70f;
	[SerializeField] private float rareWeight = 25f;
	[SerializeField] private float epicWeight = 5f;

	[Header("=== CURRENT RUN ===")]
	private List<UpgradeSO> _selectedUpgrades = new List<UpgradeSO>();  // Взятые в этом ране
	private Dictionary<UpgradeSO, int> _upgradeStacks = new Dictionary<UpgradeSO, int>();  // Сколько раз взяли

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	/// <summary>
	/// Получить 3 случайных апгрейда с учётом rarity
	/// </summary>
	public UpgradeSO[] GetRandomUpgrades(int count = 3)
	{
		int currentLevel = XPManager.Instance.GetCurrentLevel();

		// Отфильтровать доступные апгрейды
		List<UpgradeSO> availableUpgrades = allUpgrades
			.Where(u => u.minLevel <= currentLevel)  // Уровень подходит
			.Where(u => CanSelectUpgrade(u))         // Можно взять (не макс стаков)
			.ToList();

		if (availableUpgrades.Count == 0)
		{
			Debug.LogWarning("⚠️ Нет доступных апгрейдов!");
			return new UpgradeSO[0];
		}

		// Выбрать count случайных с учётом весов
		List<UpgradeSO> selected = new List<UpgradeSO>();

		for (int i = 0; i < count && availableUpgrades.Count > 0; i++)
		{
			UpgradeSO upgrade = SelectWeightedRandom(availableUpgrades);
			selected.Add(upgrade);
			availableUpgrades.Remove(upgrade);  // Убрать чтобы не повторялся
		}

		return selected.ToArray();
	}

	/// <summary>
	/// Можно ли выбрать этот апгрейд (проверка стаков)
	/// </summary>
	private bool CanSelectUpgrade(UpgradeSO upgrade)
	{
		if (!upgrade.canStack && _selectedUpgrades.Contains(upgrade))
			return false;  // Нельзя стакать и уже взят

		if (_upgradeStacks.ContainsKey(upgrade))
		{
			return _upgradeStacks[upgrade] < upgrade.maxStacks;
		}

		return true;
	}

	/// <summary>
	/// Выбрать случайный апгрейд с учётом rarity weights
	/// </summary>
	private UpgradeSO SelectWeightedRandom(List<UpgradeSO> upgrades)
	{
		float totalWeight = 0f;

		// Посчитать общий вес
		foreach (UpgradeSO upgrade in upgrades)
		{
			totalWeight += GetRarityWeight(upgrade.rarity);
		}

		// Случайное число
		float randomValue = Random.Range(0f, totalWeight);

		// Выбрать апгрейд
		float currentWeight = 0f;
		foreach (UpgradeSO upgrade in upgrades)
		{
			currentWeight += GetRarityWeight(upgrade.rarity);
			if (randomValue <= currentWeight)
			{
				return upgrade;
			}
		}

		// Fallback (не должно случиться)
		return upgrades[upgrades.Count - 1];
	}

	/// <summary>
	/// Получить вес rarity
	/// </summary>
	private float GetRarityWeight(UpgradeRarity rarity)
	{
		switch (rarity)
		{
			case UpgradeRarity.Common: return commonWeight;
			case UpgradeRarity.Rare: return rareWeight;
			case UpgradeRarity.Epic: return epicWeight;
			default: return commonWeight;
		}
	}

	/// <summary>
	/// Игрок выбрал апгрейд (добавить в список)
	/// </summary>
	public void SelectUpgrade(UpgradeSO upgrade)
	{
		if (!_selectedUpgrades.Contains(upgrade))
		{
			_selectedUpgrades.Add(upgrade);
			_upgradeStacks[upgrade] = 1;
		}
		else
		{
			_upgradeStacks[upgrade]++;
		}

		Debug.Log($"🎁 Выбран апгрейд: {upgrade.upgradeName} (Stack: {_upgradeStacks[upgrade]})");
	}

	/// <summary>
	/// Сбросить прогресс (при начале нового рана)
	/// </summary>
	public void ResetProgress()
	{
		_selectedUpgrades.Clear();
		_upgradeStacks.Clear();
	}

	/// <summary>
	/// Получить все выбранные апгрейды (для применения к герою)
	/// </summary>
	public List<UpgradeSO> GetSelectedUpgrades() => _selectedUpgrades;

	/// <summary>
	/// Получить количество стаков апгрейда
	/// </summary>
	public int GetUpgradeStacks(UpgradeSO upgrade)
	{
		return _upgradeStacks.ContainsKey(upgrade) ? _upgradeStacks[upgrade] : 0;
	}
}
