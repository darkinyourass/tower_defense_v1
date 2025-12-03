using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    public float range;
    public float shootInterval;
    public float projectileSpeed;
    public float projectileDuration;
    public float projectileSize;
    public float damage;

    public int cost;
    public Sprite sprite;

    public GameObject prefab;
    public AudioClip attackSound;

	[Header("Damage Type")]
	public DamageType damageType = DamageType.Physical;

	[Header("Special Effects")]
	[Tooltip("AOE радиус для Explosive")]
	public float aoeRadius = 2f;

	[Tooltip("% замедления для Frost (0.5 = 50%)")]
	[Range(0f, 1f)]
	public float slowAmount = 0.5f;

	[Tooltip("Длительность дебаффа (сек)")]
	public float debuffDuration = 3f;

	[Tooltip("DoT урон в секунду")]
	public float dotDamagePerSecond = 0.5f;

	[Tooltip("Количество прыжков Chain Lightning")]
	public int chainBounces = 3;

	[Tooltip("% урона на каждый прыжок")]
	[Range(0f, 1f)]
	public float chainDamageFalloff = 0.5f;
}
