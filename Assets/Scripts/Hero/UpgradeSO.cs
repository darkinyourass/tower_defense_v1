using UnityEngine;

/// <summary>
/// Данные одного апгрейда (temporary, внутри рана)
/// </summary>
[CreateAssetMenu(fileName = "Upgrade", menuName = "Roguelike/Upgrade")]
public class UpgradeSO : ScriptableObject
{
	[Header("=== BASIC INFO ===")]
	public string upgradeName = "Upgrade Name";
	[TextArea(3, 5)]
	public string description = "Upgrade description";
	public Sprite icon;

	[Header("=== RARITY ===")]
	public UpgradeRarity rarity = UpgradeRarity.Common;

	[Header("=== TYPE ===")]
	public UpgradeType upgradeType = UpgradeType.Stat;
	public UpgradeTarget target = UpgradeTarget.Hero;  // Герой или башни

	[Header("=== STAT MODIFICATIONS ===")]
	public StatModification[] statModifications;

	[Header("=== STACKING ===")]
	public bool canStack = true;  // Можно ли брать несколько раз
	public int maxStacks = 5;     // Макс количество раз

	[Header("=== LEVEL REQUIREMENT ===")]
	public int minLevel = 1;      // С какого уровня может выпасть
}

/// <summary>
/// Rarity апгрейда (влияет на шанс выпадения)
/// </summary>
public enum UpgradeRarity
{
	Common,    // 70% шанс
	Rare,      // 25% шанс
	Epic       // 5% шанс
}

/// <summary>
/// Тип апгрейда
/// </summary>
public enum UpgradeType
{
	Stat,       // Изменение статов (+damage, +speed)
	Effect,     // Добавление эффекта (fire, pierce)
	Projectile  // Изменение снаряда (+1 projectile, chain)
}

/// <summary>
/// Цель апгрейда
/// </summary>
public enum UpgradeTarget
{
	Hero,    // Улучшает героя
	Towers   // Улучшает башни
}

/// <summary>
/// Модификация одного стата
/// </summary>
[System.Serializable]
public struct StatModification
{
	public StatType statType;
	public ModificationType modificationType;
	public float value;
}

/// <summary>
/// Тип стата
/// </summary>
public enum StatType
{
	Damage,
	AttackSpeed,
	Range,
	MoveSpeed,
	ProjectileCount,
	PierceCount,
	ChainCount
}

/// <summary>
/// Тип модификации
/// </summary>
public enum ModificationType
{
	Flat,       // +10 damage
	Percentage  // +15% damage
}
