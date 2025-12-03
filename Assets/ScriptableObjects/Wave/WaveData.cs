using System;
using UnityEngine;

[Serializable]
public class WaveData
{
	[Header("Wave Configuration")]
	[Tooltip("Враги в этой волне")]
	public EnemySpawnInfo[] enemies; // ← ГЛАВНОЕ ИЗМЕНЕНИЕ!

	[Header("Timing")]
	[Tooltip("Интервал между спавнами")]
	public float spawnInterval = 1f;

	[Header("Audio")]
	public AudioClip waveSpawnClip;

	// ========== Старые поля (скрыты, но работают для совместимости) ==========
	[HideInInspector]
	public EnemyType enemyType;

	[HideInInspector]
	public int enemiesPerWave;

	// ========== Вспомогательные свойства ==========

	/// <summary>
	/// Общее количество врагов в волне
	/// </summary>
	public int TotalEnemyCount
	{
		get
		{
			// Если используется старая система
			if (enemies == null || enemies.Length == 0)
			{
				return enemiesPerWave;
			}

			// Считаем всех врагов
			int total = 0;
			foreach (var enemy in enemies)
			{
				total += enemy.count;
			}
			return total;
		}
	}

	/// <summary>
	/// Конвертирует старый формат в новый
	/// </summary>
	public void ValidateAndConvert()
	{
		// Если новый массив пустой, но старые поля заполнены
		if ((enemies == null || enemies.Length == 0) && enemiesPerWave > 0)
		{
			Debug.LogWarning($"Converting old WaveData: {enemyType} x{enemiesPerWave}");
			enemies = new EnemySpawnInfo[]
			{
				new EnemySpawnInfo
				{
					enemyType = enemyType,
					count = enemiesPerWave,
					spawnDelay = 0f
				}
			};
		}
	}
}
