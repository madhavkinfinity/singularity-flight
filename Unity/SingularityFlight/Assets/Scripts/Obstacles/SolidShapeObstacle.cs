using UnityEngine;

/// <summary>
/// SolidShapeObstacle
/// Purpose: Minimal concrete obstacle used for pooled geometric shape hazards.
/// Responsibilities:
/// - Expose ObstacleBase spawn/despawn lifecycle for primitive geometry obstacles.
/// - Ensure each runtime shape participates in collision and pooling systems.
/// </summary>
[DisallowMultipleComponent]
public sealed class SolidShapeObstacle : ObstacleBase
{
}
