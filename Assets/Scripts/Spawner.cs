using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour
{
	public static Spawner Instance { get; private set; }

	public static event Action<int> OnWaveChanged;
	public static event Action OnMissionComplete;
	public static event Action<bool> OnWaveCooldownChanged;

	private WaveData[] _waves => LevelManager.Instance.CurrentLevel.waves;
	private int _currentWaveIndex = 0;
	private int _waveCounter = 0;
	private WaveData CurrentWave => _waves[_currentWaveIndex];

	private float _spawnTimer;
	private int _enemiesRemoved;

	[SerializeField] private ObjectPooler orcPool;
	[SerializeField] private ObjectPooler dragonPool;
	[SerializeField] private ObjectPooler kaijuPool;
	[SerializeField] private ObjectPooler squidCritterPool;
	[SerializeField] private ObjectPooler squidMorphPool;
	[SerializeField] private ObjectPooler squidBossPool;
	[SerializeField] private ObjectPooler locustMorphPool;
	[SerializeField] private ObjectPooler locustBossPool;

	private Dictionary<EnemyType, ObjectPooler> _poolDictionary;

	[Header("Wave Timing")]
	[SerializeField] private float _timeBetweenWaves = 10f;
	[SerializeField] private int _earlyStartBonus = 25;
	public float WaveCooldownRemaining => _waveCooldown;
	public float WaveCooldownProgress => _waveCooldown / _timeBetweenWaves;
	public bool IsBetweenWaves => _isBetweenWaves;

	private float _waveCooldown;
	private bool _isBetweenWaves = false;
	private bool _isEndlessMode = false;
	private bool _isGamePlayScene = false;

	private Path _currentPath;

	// ========== НОВОЕ: Система спавна нескольких врагов ==========
	private Queue<EnemySpawnInfo> _currentWaveSpawnQueue;
	private EnemySpawnInfo _currentSpawningEnemy;
	private int _currentSpawnCount;
	private float _currentSpawnDelay;
	private int _totalEnemiesInWave;

	private void Awake()
	{
		_poolDictionary = new Dictionary<EnemyType, ObjectPooler>()
		{
			{ EnemyType.Orc, orcPool},
			{ EnemyType.Dragon, dragonPool},
			{ EnemyType.Kaiju, kaijuPool},
			{ EnemyType.Squid_critter, squidCritterPool},
			{ EnemyType.Squid_morph, squidMorphPool},
			{ EnemyType.Squid_boss, squidBossPool},
			{ EnemyType.Locust_morph, locustMorphPool},
			{ EnemyType.Locust_boss, locustBossPool}
		};

		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
		}
		else
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	private void OnEnable()
	{
		Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
		Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
		Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void Start()
	{
		OnWaveChanged?.Invoke(_waveCounter);
	}

	void Update()
	{
		if (!_isGamePlayScene) return;

		if (_isBetweenWaves)
		{
			_waveCooldown -= Time.deltaTime;
			if (_waveCooldown <= 0f)
			{
				StartNextWave();
			}
		}
		else
		{
			// Обрабатываем задержку перед началом спавна нового типа врага
			if (_currentSpawnDelay > 0f)
			{
				_currentSpawnDelay -= Time.deltaTime;
				return;
			}

			_spawnTimer -= Time.deltaTime;

			// Спавним текущий тип врага
			if (_spawnTimer <= 0 && _currentSpawningEnemy != null && _currentSpawnCount < _currentSpawningEnemy.count)
			{
				_spawnTimer = CurrentWave.spawnInterval;
				SpawnEnemy(_currentSpawningEnemy.enemyType);
				_currentSpawnCount++;

				// Если закончили спавнить текущий тип - переходим к следующему
				if (_currentSpawnCount >= _currentSpawningEnemy.count)
				{
					if (_currentWaveSpawnQueue.Count > 0)
					{
						_currentSpawningEnemy = _currentWaveSpawnQueue.Dequeue();
						_currentSpawnCount = 0;
						_currentSpawnDelay = _currentSpawningEnemy.spawnDelay;
					}
					else
					{
						_currentSpawningEnemy = null; // Все враги заспавнены
					}
				}
			}
			// Проверяем, закончилась ли волна
			else if (_currentSpawningEnemy == null && _enemiesRemoved >= _totalEnemiesInWave)
			{
				if (_waveCounter + 1 >= LevelManager.Instance.CurrentLevel.wavesToWin && !_isEndlessMode && GameManager.Instance.Lives > 0)
				{
					OnMissionComplete?.Invoke();
				}
				else
				{
					Debug.Log("Spawner: Волна завершена, начинаем кулдаун");
					_isBetweenWaves = true;
					_waveCooldown = _timeBetweenWaves;
					OnWaveCooldownChanged?.Invoke(true);
				}
			}
		}
	}

	/// <summary>
	/// Начинает следующую волну
	/// </summary>
	private void StartNextWave()
	{
		_currentWaveIndex = (_currentWaveIndex + 1) % _waves.Length;
		_waveCounter++;
		OnWaveChanged?.Invoke(_waveCounter);

		// Конвертируем старый формат в новый (если нужно)
		CurrentWave.ValidateAndConvert();

		// Подготавливаем очередь спавна
		_currentWaveSpawnQueue = new Queue<EnemySpawnInfo>(CurrentWave.enemies);
		_totalEnemiesInWave = CurrentWave.TotalEnemyCount;

		if (_currentWaveSpawnQueue.Count > 0)
		{
			_currentSpawningEnemy = _currentWaveSpawnQueue.Dequeue();
			_currentSpawnCount = 0;
			_currentSpawnDelay = _currentSpawningEnemy.spawnDelay;
		}

		AudioManager.Instance.PlaySound(CurrentWave.waveSpawnClip);
		_enemiesRemoved = 0;
		_spawnTimer = 0f;
		_isBetweenWaves = false;

		OnWaveCooldownChanged?.Invoke(false);

		Debug.Log($"🌊 Wave {_waveCounter} started! Total enemies: {_totalEnemiesInWave}");
	}

	/// <summary>
	/// Спавнит врага указанного типа
	/// </summary>
	private void SpawnEnemy(EnemyType enemyType)
	{
		if (_poolDictionary.TryGetValue(enemyType, out var pool))
		{
			GameObject spawnedObject = pool.GetPooledObject();
			spawnedObject.transform.position = transform.position;

			float healthMultiplier = 1f + (_waveCounter * 0.4f); // +40% per wave
			Enemy enemy = spawnedObject.GetComponent<Enemy>();
			enemy.Initialize(_currentPath, healthMultiplier);

			spawnedObject.SetActive(true);
		}
		else
		{
			Debug.LogError($"Pool for enemy type {enemyType} not found!");
		}
	}

	private void HandleEnemyReachedEnd(EnemyData data)
	{
		_enemiesRemoved++;
	}

	private void HandleEnemyDestroyed(Enemy enemy)
	{
		_enemiesRemoved++;
	}

	public void EnableEndlessMode()
	{
		_isEndlessMode = true;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		_isGamePlayScene = scene.name != "MainMenu";
		ResetWaveState();

		if (!_isGamePlayScene) return;

		_currentPath = GameObject.Find("Path1").GetComponent<Path>();
		AudioManager.Instance.PlaySound(CurrentWave.waveSpawnClip);

		if (LevelManager.Instance.CurrentLevel != null)
		{
			transform.position = LevelManager.Instance.CurrentLevel.initialSpawnPosition;
		}
	}

	private void ResetWaveState()
	{
		_currentWaveIndex = 0;
		_waveCounter = 0;
		OnWaveChanged?.Invoke(_waveCounter);
		_enemiesRemoved = 0;
		_spawnTimer = 0f;
		_isBetweenWaves = false;
		_isEndlessMode = false;
		_currentWaveSpawnQueue = null;
		_currentSpawningEnemy = null;

		foreach (var pool in _poolDictionary.Values)
		{
			if (pool != null)
			{
				pool.ResetPool();
			}
		}
	}

	public void SkipWaveCooldown()
	{
		if (!_isBetweenWaves)
		{
			Debug.LogWarning("Нельзя скипнуть - волна уже идёт!");
			return;
		}

		int bonusResources = Mathf.RoundToInt((_waveCooldown / _timeBetweenWaves) * _earlyStartBonus);

		if (bonusResources > 0)
		{
			GameManager.Instance.AddResources(bonusResources);
			Debug.Log($"⭐ Получено {bonusResources} ресурсов за ранний старт!");
		}

		_waveCooldown = 0f;
	}
}
