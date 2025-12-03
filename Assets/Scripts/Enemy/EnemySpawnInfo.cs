using System;
using UnityEngine;

[Serializable]
public class EnemySpawnInfo
{
	[Tooltip("Тип врага")]
	public EnemyType enemyType;

	[Tooltip("Количество этого типа врагов")]
	[Min(1)]
	public int count = 1;

	[Tooltip("Задержка перед спавном этого типа (сек)")]
	[Min(0f)]
	public float spawnDelay = 0f;
}
