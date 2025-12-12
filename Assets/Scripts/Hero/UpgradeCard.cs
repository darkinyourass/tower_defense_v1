using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI карточка одного апгрейда
/// </summary>
public class UpgradeCard : MonoBehaviour
{
	[Header("=== UI REFERENCES ===")]
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private TextMeshProUGUI descriptionText;
	[SerializeField] private Image backgroundImage;
	[SerializeField] private Button selectButton;

	[Header("=== RARITY COLORS ===")]
	[SerializeField] private Color commonColor = new Color(0.7f, 0.7f, 0.7f);  // Серый
	[SerializeField] private Color rareColor = new Color(0.3f, 0.5f, 1f);      // Синий
	[SerializeField] private Color epicColor = new Color(0.8f, 0.3f, 1f);      // Фиолетовый

	private int _index;

	/// <summary>
	/// Установить данные апгрейда
	/// </summary>
	public void SetUpgrade(UpgradeSO upgrade, int index)
	{
		_index = index;

		// Текст
		nameText.text = upgrade.upgradeName;
		descriptionText.text = upgrade.description;

		// Цвет фона по rarity
		backgroundImage.color = GetRarityColor(upgrade.rarity);

		// Кнопка
		selectButton.onClick.RemoveAllListeners();
		selectButton.onClick.AddListener(() => OnSelectClicked());
	}

	/// <summary>
	/// Клик по кнопке "Select"
	/// </summary>
	private void OnSelectClicked()
	{
		UpgradeUIManager.Instance.OnUpgradeSelected(_index);
	}

	/// <summary>
	/// Получить цвет по rarity
	/// </summary>
	private Color GetRarityColor(UpgradeRarity rarity)
	{
		switch (rarity)
		{
			case UpgradeRarity.Common: return commonColor;
			case UpgradeRarity.Rare: return rareColor;
			case UpgradeRarity.Epic: return epicColor;
			default: return commonColor;
		}
	}
}
