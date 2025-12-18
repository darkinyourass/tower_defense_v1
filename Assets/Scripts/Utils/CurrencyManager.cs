using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
	public static CurrencyManager Instance { get; private set; }

	[SerializeField] private int startCurrency = 0;
	private int _currentCurrency;
	public int CurrentCurrency => _currentCurrency;

	public event Action<int> OnCurrencyChanged;

	private const string CurrencyKey = "PLAYER_CURRENCY";

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		LoadCurrency();
	}

	private void LoadCurrency()
	{
		_currentCurrency = PlayerPrefs.GetInt(CurrencyKey, startCurrency);
	}

	private void SaveCurrency()
	{
		PlayerPrefs.SetInt(CurrencyKey, _currentCurrency);
		PlayerPrefs.Save();
	}

	public void AddCurrency(int amount)
	{
		_currentCurrency += amount;
		OnCurrencyChanged?.Invoke(_currentCurrency);
		SaveCurrency();
	}

	public bool SpendCurrency(int amount)
	{
		if (_currentCurrency < amount)
			return false;

		_currentCurrency -= amount;
		OnCurrencyChanged?.Invoke(_currentCurrency);
		SaveCurrency();
		return true;
	}
}
