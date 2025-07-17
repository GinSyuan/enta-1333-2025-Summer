using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float playerX;
    public float playerY;

    public PlayerData(Transform playerTransform)
    {
        playerX = playerTransform.position.x;
        playerY = playerTransform.position.y;
    }
}
