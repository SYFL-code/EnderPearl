using BepInEx;
using Fisobs.Core;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Security.Permissions;
using UnityEngine;
using On;
using IL;
//using Mono.Cecil;
using MoreSlugcats;
using RWCustom;
using Smoke;
using static PhysicalObject;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections;
using SlugBase.Features;
using System.Diagnostics;
using RewiredConsts;
using Menu.Remix;
using MonoMod.RuntimeDetour;
using Watcher;



#pragma warning disable CS0618 // Type or member is obsolete 类型或成员已过时
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace EnderPearl;

[BepInPlugin("org.dual.EnderPearl", nameof(EnderPearl), "1.1.0")]
sealed class Plugin : BaseUnityPlugin
{
	public void OnEnable()
	{
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;

        Content.Register(new EnderPearlFisob());

		// Create centi shields when centipedes lose their shells
		// 当蜈蚣失去壳时创建蜈蚣盾牌
		On.Room.AddObject += RoomAddObject;
        On.RainWorld.Update += EnderPearl.RainWorld_Update;

        // Protect the player from grabs while holding a shield
        // 手持盾牌时保护玩家免受抓取
        //On.Creature.Grab += CreatureGrab;
        EnderPearl.HookTexture();

    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
	{
        orig.Invoke(self);

        EnderPearl.HookSound();
        ParticleEffect1.HookTexture();
    }


    void RoomAddObject(On.Room.orig_AddObject orig, Room self, UpdatableAndDeletable obj)
	{
		/*if (obj is CentipedeShell shell && shell.scaleX > 0.9f && shell.scaleY > 0.9f && Random.value < 0.25f) {
			var tilePos = self.GetTilePosition(shell.pos);
			var pos = new WorldCoordinate(self.abstractRoom.index, tilePos.x, tilePos.y, 0);
			var abstr = new EnderPearlAbstract(self.world, pos, self.game.GetNewID()) {
				hue = shell.hue,
				saturation = shell.saturation,
				scaleX = shell.scaleX,
				scaleY = shell.scaleY
			};
			obj = new EnderPearl(abstr, shell.pos);

			self.abstractRoom.AddEntity(abstr);
		}*/

		if (obj is Spear spear && UnityEngine.Random.value < 0.01f)
		{
			var tilePos = self.GetTilePosition(spear.firstChunk.pos);
			var pos = new WorldCoordinate(self.abstractRoom.index, tilePos.x, tilePos.y, 0);
			var abstr = new EnderPearlAbstract(self.world, pos, self.game.GetNewID());
			obj = new EnderPearl(abstr, spear.firstChunk.pos);

			self.abstractRoom.AddEntity(abstr);
		}

		orig(self, obj);
	}

	/*bool CreatureGrab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int _, int _2, Creature.Grasp.Shareability _3, float dominance, bool _4, bool _5)
	{
		const float maxDistance = 8;

		if (obj is Creature grabbed && self is not DropBug) {
			var grasp = grabbed.grasps?.FirstOrDefault(g => g?.grabbed is EnderPearl);
			if (grasp?.grabbed is EnderPearl shield && self.bodyChunks.Any(b => Vector2.Distance(b.pos, shield.firstChunk.pos) - (b.rad + shield.firstChunk.rad) < maxDistance)) {
				shield.AllGraspsLetGoOfThisObject(true);
				shield.Forbid();
				shield.HitEffect((shield.firstChunk.pos - self.firstChunk.pos).normalized);
				shield.AddDamage(Mathf.Clamp(dominance, 0.2f, 1f));
				self.Stun(20); return false;
			}
		}

		return orig(self, obj, _, _2, _3, dominance, _4, _5);
	}*/

	// 随机查找当前房间的生物
	public static Creature? RandomlySelectedCreature(Room room, bool IncludePlayer, Creature? creature, bool IncludeDeadCreature)
	{
		List<Creature> creatures = new List<Creature>();

		if (!(room.abstractRoom.creatures.Count > 0))
		{
			return null;
		}

		// 遍历当前房间所有生物
		foreach (AbstractCreature abstractCreature in room.abstractRoom.creatures)
		{

			Creature c = abstractCreature.realizedCreature;
			// 排除检查：玩家、无效引用、自身、或没有身体部位的对象
			if (!IncludePlayer)
			{
				var player1 = c as Player;
				if (player1 != null)
				{
					continue; // 跳过无效项，继续检查下一个
				}
			}
			if (c == null ||             // 确保生物存在
				c == creature ||             // 排除自身
				c.mainBodyChunk == null ||     // 确保有有效的mainBodyChunk
				c.inShortcut)
			{
				continue; // 跳过无效项，继续检查下一个
			}
			if (DisabledCreature(c))// 禁用生物
			{
				continue; // 跳过无效项，继续检查下一个
			}
			if (c.dead == true && !IncludeDeadCreature)// 死亡的生物
			{
				continue; // 跳过无效项，继续检查下一个
			}

			creatures.Add(c);
		}
		if (creatures.Count == 0)
		{
			Console.WriteLine("MySlugcat:RandomlySelectedCreature: No valid creatures found");
			return null;
		}
		return creatures[UnityEngine.Random.Range(0, creatures.Count)];
	}

	// 禁用的生物
	public static bool DisabledCreature(Creature creature)
	{
		if (creature == null || creature is Fly || creature is SandGrub || creature is TentaclePlant ||
			creature is Leech || creature is BigEel || creature is DaddyLongLegs || creature is Overseer ||
			creature is GarbageWorm || creature is Deer || creature is Inspector || creature is PoleMimic ||
			creature is BigJellyFish || creature is StowawayBug || creature is Loach || creature is Frog ||
			creature is SkyWhale || creature is BoxWorm || creature is FireSprite || creature is DrillCrab)
		{
			return true;
		}
		return false;
	}


}
