using UnityEngine;

public class DamageInfo
{
	public float Amount { get; set; }
	public DamageType Type { get; set; }
	public Vector3 HitPosition { get; set; }

	public float AoeRadius { get; set; }
	public float SlowAmount { get; set; }
	public float SlowDuration { get; set; }
	public float DotDamagePerSecond { get; set; }
	public float DotDuration { get; set; }
	public int ChainBounces { get; set; }
	public float ChainDamageFalloff { get; set; }
	public GameObject SourceProjectile { get; set; }

	public DamageInfo(float amount, DamageType type)
	{
		Amount = amount;
		Type = type;
	}
}
