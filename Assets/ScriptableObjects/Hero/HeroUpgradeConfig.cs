using UnityEngine;

[CreateAssetMenu(fileName = "HeroUpgradeConfig", menuName = "Game/Hero Upgrade Config")]
public class HeroUpgradeConfig : ScriptableObject
{
	[System.Serializable]
	public class LevelData
	{
		public int cost;

		[Header("Damage")]
		public float damageBonus;

		[Header("Attack Speed")]
		public float attackSpeedBonus;

		[Header("Move Speed")]
		public float moveSpeedBonus;
	}

	public LevelData[] levels;

	public int MaxLevel => levels != null ? levels.Length : 0;

	public LevelData GetLevel(int levelIndex)
	{
		if (levels == null || levels.Length == 0)
			return null;

		if (levelIndex < 0) levelIndex = 0;
		if (levelIndex >= levels.Length) levelIndex = levels.Length - 1;

		return levels[levelIndex];
	}
}
