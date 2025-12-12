using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI для выбора апгрейдов при level up
/// - Показывает 3 случайных апгрейда
/// - Игрок кликает на один из них
/// - Апгрейд применяется к герою
/// </summary>
public class UpgradeUIManager : MonoBehaviour
{
	public static UpgradeUIManager Instance { get; private set; }

	[Header("=== UI REFERENCES ===")]
	[SerializeField] private GameObject upgradePanel;  // Панель с апгрейдами
	[SerializeField] private UpgradeCard[] upgradeCards;  // 3 карточки апгрейдов

	[Header("=== STATE ===")]
	private UpgradeSO[] _currentUpgrades;  // Текущие 3 апгрейда
	private bool _isPanelOpen = false;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;

		// Скрыть панель в начале
		upgradePanel.SetActive(false);
	}

	/// <summary>
	/// Показать панель с 3 случайными апгрейдами
	/// </summary>
	public void ShowUpgradePanel(UpgradeSO[] upgrades)
	{
		if (_isPanelOpen) return;

		_currentUpgrades = upgrades;
		_isPanelOpen = true;

		// Пауза игры
		Time.timeScale = 0f;

		// Обновить карточки
		for (int i = 0; i < upgradeCards.Length && i < upgrades.Length; i++)
		{
			upgradeCards[i].SetUpgrade(upgrades[i], i);
		}

		// Показать панель
		upgradePanel.SetActive(true);

		Debug.Log($"🎁 Показана панель апгрейдов");
	}

	/// <summary>
	/// Игрок выбрал апгрейд (вызывается из UpgradeCard)
	/// </summary>
	public void OnUpgradeSelected(int index)
	{
		if (index < 0 || index >= _currentUpgrades.Length)
		{
			Debug.LogError($"❌ Неверный индекс апгрейда: {index}");
			return;
		}

		UpgradeSO selectedUpgrade = _currentUpgrades[index];

		// Добавить в список выбранных
		UpgradePoolManager.Instance.SelectUpgrade(selectedUpgrade);

		// Применить апгрейд к герою
		ApplyUpgradeToHero(selectedUpgrade);

		// Закрыть панель
		HideUpgradePanel();
	}

	/// <summary>
	/// Применить апгрейд к герою
	/// </summary>
	private void ApplyUpgradeToHero(UpgradeSO upgrade)
	{
		Debug.Log($"✅ Применяю апгрейд: {upgrade.upgradeName}");

		// Применить все модификации стата
		foreach (StatModification mod in upgrade.statModifications)
		{
			HeroStats.Instance.ApplyStatModification(mod);
		}

		// Вывести итоговые статы
		HeroStats.Instance.LogStats();
	}

	/// <summary>
	/// Скрыть панель апгрейдов
	/// </summary>
	private void HideUpgradePanel()
	{
		_isPanelOpen = false;
		upgradePanel.SetActive(false);

		// Снять паузу
		Time.timeScale = 1f;

		Debug.Log($"🚫 Панель апгрейдов скрыта");
	}

	public bool IsPanelOpen() => _isPanelOpen;
}
