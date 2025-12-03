
using System;

[Flags]
public enum DamageType
{
	None = 0,
	Physical = 1 << 0,
	Fire = 1 << 1,
	Poison = 1 << 2,
	Frost = 1 << 3,
	Lightning = 1 << 4,
	Explosive = 1 << 5
}