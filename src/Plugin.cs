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
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using MonoMod.Cil;
using System.Security;
using Noise;
using Mono.Cecil.Cil;
using System.Reflection;
using BepInEx.Logging;
using System.IO;



#pragma warning disable CS0618 // Type or member is obsolete 类型或成员已过时
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace EnderPearl;

[BepInPlugin("org.dual.EnderPearl", nameof(EnderPearl), "1.1.0")]
sealed class Plugin : BaseUnityPlugin
{
	private static bool init; // 初始化标志

	private static ConditionalWeakTable<Player, StrongBox<Vector2>> LastThrowDirection = new ConditionalWeakTable<Player, StrongBox<Vector2>>();

	public void OnEnable()
	{
		init = false;

		On.RainWorld.OnModsInit += RainWorld_OnModsInit;

		/*if (init) return;
		init = true;*/

		Content.Register(new EnderPearlFisob());

		// Create centi shields when centipedes lose their shells
		// 当蜈蚣失去壳时创建蜈蚣盾牌
		On.Room.AddObject += RoomAddObject;
		On.Player.CanBeSwallowed += Player_CanBeSwallowed;
		On.RainWorld.Update += EnderPearl.RainWorld_Update;
        PlayerHooks.HookOn();
        IL.ScavengerAbstractAI.InitGearUp += IL_ScavengerAbstractAI_InitGearUp;//001
		IL.ScavengerTreasury.ctor += IL_ScavengerTreasury_ctor;//010
		IL.ScavengerAbstractAI.TradeItem += IL_ScavengerAbstractAI_TradeItem;//011
		//On.ItemSymbol.ColorForItem += On_ItemSymbol_ColorForItem;//100
		//On.ItemSymbol.SpriteNameForItem += On_ItemSymbol_SpriteNameForItem;//101
		On.SLOracleBehaviorHasMark.TypeOfMiscItem += On_SLOracleBehaviorHasMark_TypeOfMiscItem;//110
		On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += On_SLOracleBehaviorHasMark_MoonConversation_AddEvents;//111

		// Protect the player from grabs while holding a shield
		// 手持盾牌时保护玩家免受抓取
		//On.Creature.Grab += CreatureGrab;
		EnderPearl.HookTexture();

		//On.Player.TerrainImpact += Player_TerrainImpact;

        //Gives slugcat the ability to throw Ender Pearl in any direction.
        //赋予蛞蝓猫向任意方向投掷末影珍珠的能力。
        On.Player.ctor += Player_ctor;
		On.Player.Update += Player_Update;
		On.Player.ThrowObject += Player_ThrowObject;
	}

	private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
	{
		orig.Invoke(self);

        MachineConnector.SetRegisteredOI("Ender_Pearl.LH", Options.Instance);
        EnderPearl.HookSound();
		//ParticleEffect1.HookTexture();
	}

	public void OnDisable()
	{
		LastThrowDirection = null;
	}



	/*public static void Player_TerrainImpact(On.Player.orig_TerrainImpact orig, Player player, int chunk, IntVector2 direction, float speed, bool firstContact)
    {
		*//*if (EnderPearl.boo)
		{
            Logg($"start, player({player}), chunk({chunk}), direction({direction}), speed ({speed}), firstContact({firstContact})");

            orig(player, chunk, direction, speed, firstContact);

            Logg($"end, player({player}), chunk({chunk}), direction({direction}), speed ({speed}), firstContact({firstContact})");
        }
		else
		{
            orig(player, chunk, direction, speed, firstContact);
        }*//*

    }*/

    public static bool LogReset = true;
    public static void Logg(string newContent)
	{
        string filePath = "LH_MySlugcat_log.txt";

        try
        {
            Debug.Log(newContent);
            Console.WriteLine(newContent);
            // 如果文件不存在，直接创建并写入 或 新进游戏
            if (!File.Exists(filePath) || LogReset)
            {
                File.WriteAllText(filePath, newContent + "\n" + $"=== 新日志 {DateTime.Now.ToString()} ===\n");
                UnityEngine.Debug.Log($"已创建新文件,文件路径: {Path.GetFullPath(filePath)}");
                Console.WriteLine($"已创建新文件,文件路径: {Path.GetFullPath(filePath)}");
                LogReset = false;
            }
            else
            {
                // 读取现有内容
                string existingContent = File.ReadAllText(filePath);

                if (1 == 1)
                {
                    // 将新内容放在顶部 + 原有内容
                    File.WriteAllText(filePath, newContent + "\n" + existingContent);
                }
            }

            UnityEngine.Debug.Log($"内容已添加到文件顶部: {Path.GetFullPath(filePath)}");
            Console.WriteLine($"内容已添加到文件顶部: {Path.GetFullPath(filePath)}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"操作失败: {ex}");
        }

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

		//Arena
		if (self.world.game.IsStorySession && obj is Spear spear && UnityEngine.Random.value < 0.01f)
		{
			var tilePos = self.GetTilePosition(spear.firstChunk.pos);
			var pos = new WorldCoordinate(self.abstractRoom.index, tilePos.x, tilePos.y, 0);
			var abstr = new EnderPearlAbstract(self.world, pos, self.game.GetNewID());
			obj = new EnderPearl(abstr, spear.firstChunk.pos);

			self.abstractRoom.AddEntity(abstr);
		}

		orig(self, obj);
	}

	public static bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player player, PhysicalObject testObj)
	{
		if ((!ModManager.MSC || !(player.SlugCatClass == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Spear)) && testObj is EnderPearl)
		{
			return true;
		}
		else
		{
			return orig(player, testObj);
		}
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

	private void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
	{
		orig.Invoke(self, abstractCreature, world);
		StrongBox<Vector2> strongBox;
		if (!LastThrowDirection.TryGetValue(self, out strongBox))
		{
			LastThrowDirection.Add(self, new StrongBox<Vector2>());
		}
	}

	private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
	{
		Player.InputPackage[] input = self.input;
		if (input != null && input.Length != 0)
		{
			ref Player.InputPackage ptr = ref input[0];
			Vector2 normalized = new Vector2((float)ptr.x, (float)ptr.y).normalized;
			StrongBox<Vector2> strongBox;
			if (LastThrowDirection.TryGetValue(self, out strongBox) && normalized.magnitude > 0f)
			{
				strongBox.Value = normalized;
			}
		}
		orig.Invoke(self, eu);
	}

	private void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
	{
		Creature.Grasp[] grasps = self.grasps;
		object? obj;
		if (grasps == null)
		{
			obj = null;
		}
		else
		{
			Creature.Grasp grasp2 = grasps[grasp];
			obj = ((grasp2 != null) ? grasp2.grabbed : null);
		}
		orig.Invoke(self, grasp, eu);
		Weapon? weapon = obj as Weapon;
		StrongBox<Vector2> strongBox;
		if (weapon != null  && weapon is EnderPearl enderPearl && LastThrowDirection.TryGetValue(self, out strongBox))
		{
			Vector2 value = strongBox.Value;
			BodyChunk[] bodyChunks = enderPearl.bodyChunks;
			Vector2 pos = self.mainBodyChunk.pos + value * 10f;
			foreach (BodyChunk bodyChunk in bodyChunks)
			{
				bodyChunk.pos = pos;
				bodyChunk.vel = value * 40f;
			}
			enderPearl.setRotation = new Vector2?(value);
		}
	}

	/// <summary>
	/// 根据条件自然生成EnderPearl
	/// </summary>
	private static int SpawnEnderPearl(ScavengerAbstractAI self, int count)
	{
		if (count >= 0 && UnityEngine.Random.value < (self.parent.creatureTemplate.type == CreatureTemplate.Type.Scavenger ? 0.01f : 0.02f))
		{
			//var EnderPearl = new EnderPearlAbstract(self.world, self.parent.pos, self.world.game.GetNewID());

			var EnderPearl = new AbstractPhysicalObject(self.world, EnderPearlFisob.EnderPearl, null, self.parent.pos, self.world.game.GetNewID());
			self.world.GetAbstractRoom(self.parent.pos).AddEntity(EnderPearl);
			new AbstractPhysicalObject.CreatureGripStick(self.parent, EnderPearl, count, true);
			count--;
		}
		return count;
	}

    /// <summary>
    /// 修改初始化装备时的逻辑，尝试自然生成EnderPearl
    /// </summary>
    private static void IL_ScavengerAbstractAI_InitGearUp(ILContext il)
	{
		var c = new ILCursor(il);
		if (c.TryGotoNext(
			x => x.MatchLdloc(0),
			x => x.MatchLdcI4(0),
			x => x.MatchBlt(out var _),
			x => x.MatchCallOrCallvirt(out var _),
			x => x.MatchLdcR4(0.08f),
			x => x.MatchBgeUn(out var _)
		))
		{
			c.MoveAfterLabels();
			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Ldloc_0);
			c.EmitDelegate(SpawnEnderPearl);
			c.Emit(OpCodes.Stloc_0);
		}
	}

	/// <summary>
	/// 在交易时尝试生成EnderPearl
	/// </summary>
	private static AbstractPhysicalObject EnderPearlTrade(AbstractSpear orig, ScavengerAbstractAI self)
	{
		if (true)
		{
			float chance = 0.01f;
			if (!self.world.singleRoomWorld)
			{
				var region = self.world.region.name;
				/*if (region == "SI")
				{
					chance = 0.25f;
				}
				else if (region == "CC" || region == "UW")
				{
					chance = 0.15625f;
				}*/
			}
			if (UnityEngine.Random.value < chance)
			{
				//return new EnderPearlAbstract(self.world, self.parent.pos, self.world.game.GetNewID());
				return new AbstractPhysicalObject(self.world, EnderPearlFisob.EnderPearl, null, self.parent.pos, self.world.game.GetNewID());
			}
		}
		return orig;
	}

	/// <summary>
	/// 修改交易物品时的逻辑
	/// </summary>
	private static void IL_ScavengerAbstractAI_TradeItem(ILContext il)
	{
		var c = new ILCursor(il);
		if (c.TryGotoNext(
			MoveType.After,
			x => x.MatchNewobj<AbstractSpear>()
		))
		{
			c.MoveAfterLabels();
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate(EnderPearlTrade);
		}
	}

	/// <summary>
	/// 在宝库生成物品时尝试生成EnderPearl
	/// </summary>
	public static AbstractPhysicalObject TreasuryEnderPearl(AbstractPhysicalObject orig, ScavengerTreasury self, int i)
	{
		if (true)
		{
			var chance = 0.02f;
			if (!self.room.world.singleRoomWorld)
			{
				var region = self.room.world.region.name;
				/*if (region == "SI")
				{
					chance = 0.1f;
				}
				else if (region == "CC" || region == "UW")
				{
					chance = 0.7f;
				}*/
			}
			if (UnityEngine.Random.value < chance)
			{
				//return new EnderPearlAbstract(self.room.world, self.room.GetWorldCoordinate(self.tiles[i]), self.room.world.game.GetNewID());
				return new AbstractPhysicalObject(self.room.world, EnderPearlFisob.EnderPearl, null, self.room.GetWorldCoordinate(self.tiles[i]), self.room.game.GetNewID());
			}
		}
		return orig;
	}

	/// <summary>
	/// 修改宝库初始化时的逻辑
	/// </summary>
	private static void IL_ScavengerTreasury_ctor(ILContext il)
	{
		var c = new ILCursor(il);
		//var skip = c.DefineLabel();
		var skip2 = c.DefineLabel();
		if (c.TryGotoNext(
			x => x.MatchLdarg(0),
			x => x.MatchLdfld<ScavengerTreasury>("property"),
			x => x.MatchLdloc(8),
			x => x.MatchCallOrCallvirt(out var _)
		))
		{
			c.MoveAfterLabels();
			c.Emit(OpCodes.Ldloc, 8);
			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Ldloc, 7);
			c.EmitDelegate(TreasuryEnderPearl);
			c.Emit(OpCodes.Stloc, 8);
		}
	}

	/// <summary>
	/// 修改物品符号颜色的逻辑
	/// </summary>
	private static Color On_ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
	{
		if (itemType == EnderPearlFisob.EnderPearl)
			return new Color(209f / 255f, 69f / 255f, 247f / 255f, 1f);
		return orig(itemType, intData);
	}

	/// <summary>
	/// 修改物品符号名称的逻辑
	/// </summary>
	private static string On_ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
	{
		if (itemType == EnderPearlFisob.EnderPearl)
			return "Symbol_EnderPearl";
		return orig(itemType, intData);
	}

	/// <summary>
	/// 修改杂项物品类型的逻辑
	/// </summary>
	private static SLOracleBehaviorHasMark.MiscItemType On_SLOracleBehaviorHasMark_TypeOfMiscItem(On.SLOracleBehaviorHasMark.orig_TypeOfMiscItem orig, SLOracleBehaviorHasMark self, PhysicalObject testItem)
	{
		if (testItem is EnderPearl)
		{
			return EnderPearlFisob.MiscItemTypeEnderPearl;
		}
		return orig(self, testItem);
	}

	/// <summary>
	/// 在月之对话事件中添加EnderPearl的描述
	/// </summary>
	private static void On_SLOracleBehaviorHasMark_MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
	{
		orig(self);
		if (self.id == Conversation.ID.Moon_Misc_Item)
		{
			if (self.describeItem == EnderPearlFisob.MiscItemTypeEnderPearl)
			{
                // 这是某种生物的眼睛，含有少量的虚空流体。<LINE>
                // 当它受损时，它会将生物分解成量子态并在着陆点重新组装。
                // 文本事件（对话所属者，初始等待时间，文本内容，文本停留时长）
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("This is the eye of a certain being, containing a small amount of void fluid."), 0));
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("When damaged, it decomposes the creature into a quantum state and reassembles them at the landing point."), 0));
                return;
			}
		}
	}



	/// <summary>
	/// 随机查找当前房间的生物
	/// </summary>
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
