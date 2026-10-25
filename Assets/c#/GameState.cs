using UnityEngine;

// 跨场景保存的玩家数据。
// static 属于整个游戏进程，切场景不会清空，所以 room1 → room2 血量能接着走。
public static class GameState
{
    public static int PlayerHealth=-1;
    public static int PlayerMaxHealth=-1;

    public static void Clear(){
        PlayerHealth=-1;
        PlayerMaxHealth=-1;

    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay(){
        Clear();
    }

}
