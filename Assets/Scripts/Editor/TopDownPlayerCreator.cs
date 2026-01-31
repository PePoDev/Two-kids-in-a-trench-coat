using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor script to create Player prefabs with TopDownPlayer component.
/// Use from menu: GameObject > 2D Object > Create Top-Down Players
/// </summary>
public class TopDownPlayerCreator : MonoBehaviour
{
    [MenuItem("GameObject/2D Object/Create Top-Down Players", false, 10)]
    static void CreateTopDownPlayers()
    {
        // Create Player 1
        GameObject player1 = CreatePlayer("Player1_WASD", TopDownPlayer.PlayerNumber.Player1, 
            new Color(0.2f, 0.6f, 1f), new Vector3(-2f, 0f, 0f));
        
        // Create Player 2
        GameObject player2 = CreatePlayer("Player2_Arrow", TopDownPlayer.PlayerNumber.Player2,
            new Color(1f, 0.4f, 0.4f), new Vector3(2f, 0f, 0f));
        
        // Create Prefabs folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        // Save as prefabs
        string prefabPath1 = "Assets/Prefabs/Player1_WASD.prefab";
        string prefabPath2 = "Assets/Prefabs/Player2_Arrow.prefab";
        
        PrefabUtility.SaveAsPrefabAsset(player1, prefabPath1);
        PrefabUtility.SaveAsPrefabAsset(player2, prefabPath2);
        
        Debug.Log($"Created prefabs:\n- {prefabPath1}\n- {prefabPath2}");
        
        // Select the created objects
        Selection.objects = new Object[] { player1, player2 };
    }
    
    [MenuItem("GameObject/2D Object/Create Player 1 (WASD)", false, 11)]
    static void CreatePlayer1Only()
    {
        GameObject player = CreatePlayer("Player1_WASD", TopDownPlayer.PlayerNumber.Player1, 
            new Color(0.2f, 0.6f, 1f), Vector3.zero);
        Selection.activeGameObject = player;
    }
    
    [MenuItem("GameObject/2D Object/Create Player 2 (Arrow)", false, 12)]
    static void CreatePlayer2Only()
    {
        GameObject player = CreatePlayer("Player2_Arrow", TopDownPlayer.PlayerNumber.Player2,
            new Color(1f, 0.4f, 0.4f), Vector3.zero);
        Selection.activeGameObject = player;
    }
    
    static GameObject CreatePlayer(string name, TopDownPlayer.PlayerNumber playerNumber, Color color, Vector3 position)
    {
        // Create GameObject
        GameObject playerObj = new GameObject(name);
        playerObj.tag = "Player";
        playerObj.transform.position = position;
        
        // Add SpriteRenderer with default sprite
        SpriteRenderer spriteRenderer = playerObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        spriteRenderer.color = color;
        
        // Add Rigidbody2D
        Rigidbody2D rb = playerObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 5f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Add BoxCollider2D
        BoxCollider2D collider = playerObj.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.8f, 0.8f);
        
        // Add TopDownPlayer script
        TopDownPlayer controller = playerObj.AddComponent<TopDownPlayer>();
        controller.SetPlayerNumber(playerNumber);
        
        // Register undo
        Undo.RegisterCreatedObjectUndo(playerObj, "Create Top-Down Player");
        
        return playerObj;
    }
}
#endif
