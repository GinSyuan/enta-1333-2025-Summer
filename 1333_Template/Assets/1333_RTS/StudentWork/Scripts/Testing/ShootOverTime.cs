using UnityEngine;

public class ShootOverTime : MonoBehaviour
{
	[Header("Projectile")]
	[SerializeField] public ArcingProjectile ProjectilePrefab;
	[SerializeField] private Transform ProjectileStartPoint;

	// Replace this with some kind of targeted enemy position
	[SerializeField] private Transform DebugEndPoint;

	[Header("Flight Settings")]
	[SerializeField] [Min(0.1f)] private float ProjectileSpeed = 5f; // units / second
	[SerializeField] private float ArcHeight = 2f; // world units

	/// <summary>
	///     Fires the projectile
	/// </summary>
	public void Shoot()
	{
		// Instantiate with generic overload so no GetComponent is needed.
		var proj =
			Instantiate(ProjectilePrefab, transform.position, Quaternion.identity);

		proj.Launch(transform.position, DebugEndPoint.position, ProjectileSpeed, ArcHeight);
	}
}
