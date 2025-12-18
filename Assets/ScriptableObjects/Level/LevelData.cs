using UnityEngine;

public class LevelData : ScriptableObject
{
	public string levelName; // match a scene name
	public int wavesToWin;
	public int startingResources;
	public int startingLives;

	public int baseReward = 100;          // базовая награда за идеальное прохождение
	public bool scaleRewardByLives = true; // флаг: учитывать ли оставшиеся жизни

	public Vector2 initialSpawnPosition;
	public WaveData[] waves;
}