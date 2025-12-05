using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerUpgradePanel : MonoBehaviour
{
	[SerializeField] private GameObject panel;
	[SerializeField] private Image towerIcon;
	[SerializeField] private TextMeshProUGUI towerNameText;
	[SerializeField] private TextMeshProUGUI levelText;

	[SerializeField] private TextMeshProUGUI damageText;
	[SerializeField] private TextMeshProUGUI rangeText;
	[SerializeField] private TextMeshProUGUI fireRateText;

	[SerializeField] private Button upgradeButton;
	[SerializeField] private TextMeshProUGUI upgradeCostText;
	[SerializeField] private TextMeshProUGUI upgradeBonusText;  // Спеціальний ефект на 5 рівні
	[SerializeField] private Button closeButton;

	private Tower _selectedTower;
	private TowerData _towerData;

	private void OnEnable()
	{
		upgradeButton.onClick.AddListener(OnUpgradeClicked);
		closeButton.onClick.AddListener(Close);
	}

	private void OnDisable()
	{
		upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
		closeButton.onClick.RemoveListener(Close);
	}

	/// <summary>
	/// Відкрити панель апгрейду для обраної башни
	/// </summary>
	public void Open(Tower tower)
	{
		_selectedTower = tower;
		_towerData = tower.GetTowerData();

		if (_towerData == null)
		{
			Debug.LogWarning("Башня не має TowerData!");
			return;
		}

		// Заповнити інформацію башни
		towerNameText.text = _towerData.name;
		if (_towerData.sprite != null)
			towerIcon.sprite = _towerData.sprite;

		// Заповнити рівень і статистику
		levelText.text = $"Рівень: {_towerData.currentLevel}/5";
		damageText.text = $"Урон: {_towerData.damage:F1}";
		rangeText.text = $"Радіус: {_towerData.range:F1}";
		fireRateText.text = $"Швидкість атаки: {(1f / _towerData.shootInterval):F2}";

		// Скасувати бонус текст спочатку
		upgradeBonusText.text = "";

		// Заповнити кнопку апгрейду
		if (_towerData.CanUpgrade())
		{
			int cost = _towerData.GetUpgradeCost();
			upgradeCostText.text = $"Апгрейд: {cost} 💰";
			upgradeButton.interactable = true;

			// Показати спеціальний бонус якщо це буде 5 рівень
			TowerUpgradeLevel nextLevel = _towerData.GetNextUpgradeLevel();
			if (nextLevel != null && nextLevel.isUltimateUpgrade)
			{
				string bonus = "🌟 СПЕЦІАЛЬНИЙ АПГРЕЙД!\n";

				if (nextLevel.unlocksAOE)
					bonus += "✨ Розблокована атака по площі (AOE)\n";

				if (nextLevel.unlocksChain)
					bonus += "⚡ Розблокована ланцюжкова блискавка\n";

				if (nextLevel.unlocksDOT)
					bonus += "🔥 Розблокований вогневий урон\n";

				upgradeBonusText.text = bonus;
				upgradeBonusText.color = new Color(1f, 0.84f, 0f); // Золотий колір
			}
		}
		else
		{
			upgradeCostText.text = "⭐ МАКСИМАЛЬНИЙ РІВЕНЬ";
			upgradeButton.interactable = false;
			upgradeBonusText.text = "";
		}

		panel.SetActive(true);
		Time.timeScale = 0f; // Пауза
	}

	public void OnUpgradeClicked()
	{
		if (_selectedTower == null || _towerData == null)
			return;

		int cost = _towerData.GetUpgradeCost();

		// Перевірити ресурси
		if (GameManager.Instance.Resources >= cost)
		{
			GameManager.Instance.SpendResources(cost);
			_selectedTower.UpgradeTower();

			// Грати звук апгрейду
			AudioManager.Instance.PlayTowerPlaced();

			// Оновити панель
			Open(_selectedTower);
		}
		else
		{
			// Показати попередження
			Debug.LogWarning("Недостатньо ресурсів для апгрейду!");
			// Можна додати анімацію тремтіння кнопки або звук помилки
			StartCoroutine(ShowInsufficientResourcesWarning());
		}
	}

	/// <summary>
	/// Закрити панель апгрейду
	/// </summary>
	public void Close()
	{
		panel.SetActive(false);
		Time.timeScale = 1f; // Знести паузу
	}

	private System.Collections.IEnumerator ShowInsufficientResourcesWarning()
	{
		Color originalColor = upgradeCostText.color;
		upgradeCostText.color = Color.red;
		upgradeCostText.text = "❌ Недостатньо ресурсів!";

		yield return new WaitForSecondsRealtime(1f);

		int cost = _towerData.GetUpgradeCost();
		upgradeCostText.text = $"Апгрейд: {cost} 💰";
		upgradeCostText.color = originalColor;
	}
}
