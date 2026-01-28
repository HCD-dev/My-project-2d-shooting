using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public float health = 50f;
    public float speed = 2f;
    public float damageToPlayer = 10f;
}