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
	[SerializeField] private TextMeshProUGUI upgradeBonusText;

	// ⭐ НОВЕ: Кнопка продажу
	[SerializeField] private Button sellButton;
	[SerializeField] private TextMeshProUGUI sellCostText;

	[SerializeField] private Button closeButton;

	private Tower _selectedTower;
	private TowerData _towerData;
	private bool _isProcessingUpgrade = false;

	private void OnEnable()
	{
		upgradeButton.onClick.AddListener(OnUpgradeClicked);
		sellButton.onClick.AddListener(OnSellClicked);
		closeButton.onClick.AddListener(Close);
	}

	private void OnDisable()
	{
		upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
		sellButton.onClick.RemoveListener(OnSellClicked);
		closeButton.onClick.RemoveListener(Close);
	}

	/// <summary>
	/// Відкрити панель апгрейду для обраної башни
	/// </summary>
	public void Open(Tower tower)
	{
		_selectedTower = tower;
		_towerData = tower.GetTowerData();
		_isProcessingUpgrade = false;

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
		levelText.text = $"Level: {_towerData.currentLevel}/5";
		damageText.text = $"DMG: {_towerData.damage:F1}";
		rangeText.text = $"Range: {_towerData.range:F1}";
		fireRateText.text = $"Attack Speed: {(1f / _towerData.shootInterval):F2}";

		// Скасувати бонус текст спочатку
		upgradeBonusText.text = "";

		// Заповнити кнопку апгрейду
		if (_towerData.CanUpgrade())
		{
			int cost = _towerData.GetUpgradeCost();
			upgradeCostText.text = $"Upgrade: {cost} 💰";
			upgradeButton.interactable = true;

			// Показати спеціальний бонус якщо це буде 5 рівень
			TowerUpgradeLevel nextLevel = _towerData.GetNextUpgradeLevel();
			if (nextLevel != null && nextLevel.isUltimateUpgrade)
			{
				string bonus = "🌟 SPECIAL UPGRADE!\n";

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
			upgradeCostText.text = "⭐ MAX LEVEL";
			upgradeButton.interactable = false;
			upgradeBonusText.text = "";
		}

		// ⭐ НОВЕ: Заповнити кнопку продажу
		int sellAmount = Mathf.RoundToInt(_selectedTower.TotalInvestedCost * 0.5f);
		sellCostText.text = $"Sell: {sellAmount} 💰";

		panel.SetActive(true);
		Time.timeScale = 0f; // Пауза
	}

	public void OnUpgradeClicked()
	{
		if (_isProcessingUpgrade)
		{
			Debug.LogWarning("Апгрейд уже обрабатывается!");
			return;
		}

		if (_selectedTower == null || _towerData == null)
			return;

		int cost = _towerData.GetUpgradeCost();

		if (GameManager.Instance.Resources >= cost)
		{
			_isProcessingUpgrade = true;

			GameManager.Instance.SpendResources(cost);
			_selectedTower.UpgradeTower();

			AudioManager.Instance.PlayTowerPlaced();

			Close();
		}
		else
		{
			Debug.LogWarning("Недостатньо ресурсів для апгрейду!");
			StartCoroutine(ShowInsufficientResourcesWarning());
		}
	}

	/// <summary>
	/// ⭐ НОВЕ: Обробник кнопки продажу
	/// </summary>
	public void OnSellClicked()
	{
		if (_selectedTower == null)
			return;

		// Грати звук (можна додати окремий звук продажу)
		AudioManager.Instance.PlayTowerPlaced();

		// Продати башню
		_selectedTower.Sell();

		// Закрити панель
		Close();
	}

	/// <summary>
	/// Закрити панель апгрейду
	/// </summary>
	public void Close()
	{
		panel.SetActive(false);
		Time.timeScale = 1f; // Зняти паузу
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
