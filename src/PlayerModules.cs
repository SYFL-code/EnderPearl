using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using EnderPearl;

internal static class PlayerModuleManager
{
    public static ConditionalWeakTable<Player, PlayerModule> playerModules = new ConditionalWeakTable<Player, PlayerModule>();

    internal class PlayerModule
    {
        WeakReference<Player> playerRef;

        public bool Teleport;

        public int coolDown;//冷却计时器

        public PlayerModule(Player player)
        {
            playerRef = new WeakReference<Player>(player);
            SetUp(player.SlugCatClass);
        }

        void SetUp(SlugcatStats.Name name)
        {
        }

        public void OnPlayerUpdate(Player player)
        {
            if (coolDown > 0)
                coolDown--;
        }

        public void OnPlayerMovementUpdate(Player player)
        {
            if (Teleport)
            {
                Vector2 almostZero = new Vector2(0.0001f, -0.00009f);
                player.bodyChunks[1].terrainCurveNormal = almostZero;
                if (EnderPearl.Options.Instance.OpCheckBoxStunUponLanding.Value != null && EnderPearl.Options.Instance.OpCheckBoxStunDuration.Value != null)
                {
                    if (EnderPearl.Options.Instance.OpCheckBoxStunUponLanding.Value && EnderPearl.Options.Instance.OpCheckBoxStunDuration.Value > 0)
                    {
                        player.Stun((int)Math.Round(40 * EnderPearl.Options.Instance.OpCheckBoxStunDuration.Value));
                    }
                }
                Teleport = false;
            }
        }

    }
}

internal static class PlayerHooks
{

    public static void HookOn()
    {
        On.Player.ctor += Player_ctor;
        On.Player.Update += Player_Update;
        On.Player.MovementUpdate += Player_MovementUpdate;
    }

    public static void Player_MovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
    {
        if (PlayerModuleManager.playerModules.TryGetValue(self, out var module))
        {
            module.OnPlayerMovementUpdate(self);
        }
        orig(self, eu);
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig.Invoke(self, eu);
        if (PlayerModuleManager.playerModules.TryGetValue(self, out var module))
        {
            module.OnPlayerUpdate(self);
        }
    }

    private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig.Invoke(self, abstractCreature, world);
        PlayerModuleManager.playerModules.Add(self, new PlayerModuleManager.PlayerModule(self));
    }
}
