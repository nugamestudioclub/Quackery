using UnityEngine;

[CreateAssetMenu(
    fileName = "PrefabDB",
    menuName = "Game/Prefab Database"
)]
public class PrefabDatabase : ScriptableObject
{
    public GameObject rocketLauncher;
    public GameObject rocket;
    public GameObject smoke;
    public GameObject propellerHat;
    public GameObject grapplingHook;
}
