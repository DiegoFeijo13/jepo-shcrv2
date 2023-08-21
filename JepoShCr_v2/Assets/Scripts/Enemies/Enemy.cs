using UnityEngine;
using UnityEngine.Rendering;

#region REQUIRE COMPONENTS
[RequireComponent (typeof(EnemyMovementAI))]
[RequireComponent (typeof(MovementToPosition))]
[RequireComponent (typeof(MovementToPositionEvent))]
[RequireComponent (typeof(Idle))]
[RequireComponent (typeof(IdleEvent))]
[RequireComponent (typeof(SortingGroup))]
[RequireComponent (typeof(SpriteRenderer))]
[RequireComponent (typeof(Animator))]
[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent (typeof(CircleCollider2D))]
[RequireComponent (typeof(PolygonCollider2D))]
#endregion REQUIRE COMPONENTS

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour
{
    public EnemyDetailSO enemyDetails;    

    [HideInInspector] public SpriteRenderer[] spriteRendererArray;
    [HideInInspector] public Animator animator;
    [HideInInspector] public MovementToPositionEvent movementToPositionEvent;
    [HideInInspector] public IdleEvent idleEvent;

    private CircleCollider2D circleCollider2D;
    private PolygonCollider2D polygonCollider2D;
    private EnemyMovementAI enemyMovementAI;
    

    private void Awake()
    {
        spriteRendererArray = GetComponentsInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
        idleEvent = GetComponent<IdleEvent>();
        circleCollider2D = GetComponent<CircleCollider2D>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        enemyMovementAI = GetComponent<EnemyMovementAI>();
    }
}
