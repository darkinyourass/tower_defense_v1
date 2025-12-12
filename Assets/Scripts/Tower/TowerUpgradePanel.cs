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
	[SerializeField] private Button closeButton;

	private Tower _selectedTower;
	private TowerData _towerData;
	private bool _isProcessingUpgrade = false;  // ⭐ ДОБАВИТЬ ЭТОТ ФЛАГ

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

	public void Open(Tower tower)
	{
		_selectedTower = tower;
		_towerData = tower.GetTowerData();
		_isProcessingUpgrade = false;  // ⭐ СБРОСИТЬ ФЛАГ

		if (_towerData == null)
		{
			Debug.LogWarning("Башня не має TowerData!");
			return;
		}

		towerNameText.text = _towerData.name;
		if (_towerData.sprite != null)
			towerIcon.sprite = _towerData.sprite;

		levelText.text = $"Level: {_towerData.currentLevel}/5";
		damageText.text = $"DMG: {_towerData.damage:F1}";
		rangeText.text = $"Range: {_towerData.range:F1}";
		fireRateText.text = $"Attack Speed: {(1f / _towerData.shootInterval):F2}";

		upgradeBonusText.text = "";

		if (_towerData.CanUpgrade())
		{
			int cost = _towerData.GetUpgradeCost();
			upgradeCostText.text = $"Upgrade: {cost} 💰";
			upgradeButton.interactable = true;

			TowerUpgradeLevel nextLevel = _towerData.GetNextUpgradeLevel();
			if (nextLevel != null && nextLevel.isUltimateUpgrade)
			{
				string bonus = "🌟 SPECIEL UPGRADE!\n";

				if (nextLevel.unlocksAOE)
					bonus += "✨ Розблокована атака по площі (AOE)\n";

				if (nextLevel.unlocksChain)
					bonus += "⚡ Розблокована ланцюжкова блискавка\n";

				if (nextLevel.unlocksDOT)
					bonus += "🔥 Розблокований вогневий урон\n";

				upgradeBonusText.text = bonus;
				upgradeBonusText.color = new Color(1f, 0.84f, 0f);
			}
		}
		else
		{
			upgradeCostText.text = "⭐ MAX LEVEL";
			upgradeButton.interactable = false;
			upgradeBonusText.text = "";
		}

		panel.SetActive(true);
		Time.timeScale = 0f;
	}

	public void OnUpgradeClicked()
	{
		// ⭐ ЗАЩИТА: Если уже обрабатываем апгрейд, выходим
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
			_isProcessingUpgrade = true;  // ⭐ УСТАНОВИТЬ ФЛАГ

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

	public void Close()
	{
		panel.SetActive(false);
		Time.timeScale = 1f;
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
