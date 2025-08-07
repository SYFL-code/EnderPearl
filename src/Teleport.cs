using System;
using System.Collections.Generic;
using UnityEngine;


namespace EnderPearl
{
    public static class Teleport
    {

        public static void SetObjectPosition(PhysicalObject obj, Vector2 newPos)
        {
            if (!(obj is Creature))
            {
                ReleaseAllGrasps(obj);
            }
            if (obj is Player player)
            {
                if (player.tongue != null)
                {
                    player.tongue.resetRopeLength();
                    player.tongue.mode = Player.Tongue.Mode.Retracted;
                    player.tongue.rope.Reset();
                }
                for (int num12 = 0; num12 < 2; num12++)
                {
                    //player.bodyChunks[num12].vel = Custom.DegToVec(UnityEngine.Random.value * 360f) * 12f;
                    player.bodyChunks[num12].pos = newPos;
                    player.bodyChunks[num12].lastPos = newPos;
                }
                return;
            }
            int num = 0;
            for (; ; )
            {
                int num2 = num;
                int? num3;
                if (obj == null)
                {
                    num3 = null;
                }
                else
                {
                    BodyChunk[] bodyChunks = obj.bodyChunks;
                    num3 = ((bodyChunks != null) ? new int?(bodyChunks.Length) : null);
                }
                int? num4 = num3;
                if (!(num2 < num4.GetValueOrDefault() & num4 != null))
                {
                    break;
                }
                if (obj != null && obj.bodyChunks[num] != null)
                {
                    obj.bodyChunks[num].pos = newPos;
                    obj.bodyChunks[num].lastPos = newPos;
                    obj.bodyChunks[num].lastLastPos = newPos;
                    obj.bodyChunks[num].vel = default(Vector2);
                    if (obj is PlayerCarryableItem playerCarryableItem)
                    {
                        playerCarryableItem.lastOutsideTerrainPos = null;
                    }
                }
                num++;
            }
        }

        public static void ReleaseAllGrasps(PhysicalObject obj)
        {
            if (((obj != null) ? obj.grabbedBy : null) != null && obj != null)
            {
                for (int i = obj.grabbedBy.Count - 1; i >= 0; i--)
                {
                    Creature.Grasp grasp = obj.grabbedBy[i];
                    if (grasp != null)
                    {
                        grasp.Release();
                    }
                }
            }
            if (obj is Creature creature)
            {
                if (obj is Player player)
                {
                    Player.SlugOnBack slugOnBack = player.slugOnBack;
                    if (slugOnBack != null)
                    {
                        slugOnBack.DropSlug();
                    }
                    Player onBack = player.onBack;
                    if (onBack != null)
                    {
                        Player.SlugOnBack slugOnBack2 = onBack.slugOnBack;
                        if (slugOnBack2 != null)
                        {
                            slugOnBack2.DropSlug();
                        }
                    }
                    player.slugOnBack = null;
                    player.onBack = null;
                    Player.SpearOnBack spearOnBack = player.spearOnBack;
                    if (spearOnBack != null)
                    {
                        spearOnBack.DropSpear();
                    }
                }
                creature.LoseAllGrasps();
            }
        }


    }
}
