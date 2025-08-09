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
using System.IO;


namespace EnderPearl;
//spawn_raw EnderPearl
sealed class EnderPearl : Weapon
{
	private static float Rand => UnityEngine.Random.value;

	public static float scale = 1f;

	public bool ignited;

	public float burn;


	public EnderPearlAbstract Abstr { get; }

	public static SoundID? soundID_Teleport1;//音效
	public static SoundID? soundID_Teleport2;//音效

	/*public static SoundID soundID_iceExplode;//音效
	public static SoundID soundID_iceShieldCraft;//音效*/

	public static List<ParticleEffect> PE = new List<ParticleEffect>();

	public static void HookTexture()
	{
		if (!Futile.atlasManager.DoesContainElementWithName("icon_EnderPearl"))
		{
			Futile.atlasManager.LoadAtlas("icon_EnderPearl");
		}
	}
	public static void HookSound()
	{
		//加载音效
		soundID_Teleport1 = new SoundID("SNDTeleport1", true);
		soundID_Teleport2 = new SoundID("SNDTeleport2", true);
		//加载音效
		/*soundID_iceExplode = new SoundID("SNDiceExplode", true);
		soundID_iceShieldCraft = new SoundID("SNDiceShieldCraft", true);*/
	}

	public EnderPearl(EnderPearlAbstract abstr, Vector2 pos) : base(abstr, abstr.world)
	{
		Abstr = abstr;

		//Abstr.scaleX = scale;
		//Abstr.scaleY = scale;

		bodyChunks = new BodyChunk[1];
		bodyChunks[0] = new BodyChunk(this, 0, pos, 4 * (Abstr.scaleX + Abstr.scaleY), 0.07f);

		//bodyChunks = new[] { new BodyChunk(this, 0, pos + vel, 4 * (Abstr.scaleX + Abstr.scaleY), 0.35f) { goThroughFloors = true } };
		bodyChunks[0].lastPos = bodyChunks[0].pos;

		firstChunk.loudness = 9f; // 响度
		firstChunk.pos = pos;

		bodyChunkConnections = new BodyChunkConnection[0];
		airFriction = 0.999f; // 空气摩擦
		gravity = 0.65f; // 重力
		bounce = 0.1f; // 弹力
		surfaceFriction = 0.1f; // 表面摩擦力
		collisionLayer = 2; // 层碰撞
		waterFriction = 0.92f; // 水摩擦
		buoyancy = 0.35f; // 浮力

		lastRotation = rotation;
		//rotationOffset = Rand * 30 - 15; // 旋转偏移
		//ResetVel(vel.magnitude); // 重置速度(速度.向量的长度)

		/*// 初始化物理对象的身体块（BodyChunk）数组，这里只包含一个块
		base.bodyChunks = new BodyChunk[1];
		// 创建第一个身体块，设置位置为(0,0)，质量为5，弹性系数为0.07
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);

		// 初始化身体块之间的连接数组，这里为空（没有连接）
		this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];

		// 设置物理属性
		base.airFriction = 0.999f;  // 空气阻力系数
		base.gravity = 0.9f;        // 重力强度
		this.bounce = 0.4f;         // 反弹系数
		this.surfaceFriction = 0.4f; // 表面摩擦力
		this.collisionLayer = 2;     // 碰撞层级（2表示特定碰撞层）

		// 设置水中物理属性
		base.waterFriction = 0.98f;  // 水中阻力系数
		base.buoyancy = 0.4f;        // 浮力系数

		// 控制物理行为的布尔标志
		this.pivotAtTip = false;     // 是否以尖端为旋转支点
		this.lastPivotAtTip = false; // 上一帧的支点状态
		this.stuckBodyPart = -1;     // 当前未附着任何身体部位（-1表示无）

		// 设置第一个身体块的音量（可能用于音效）
		base.firstChunk.loudness = 7f;
		// 初始化尾部位置为第一个身体块的位置
		this.tailPos = base.firstChunk.pos;

		// 创建基于第一个身体块的动态声音循环
		this.soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);

		// 初始化水平光束状态数组（长度为3）
		this.wasHorizontalBeam = new bool[3];

		// 以下是与"Spearmaster"（可能为某种武器/角色）相关的属性
		this.spearmasterNeedle = false;  // 是否激活"Spearmaster"的针状物
		this.spearmasterNeedle_hasConnection = false; // 针状物是否有连接
		this.spearmasterNeedle_fadecounter_max = 400; // 针状物淡出时间的最大值
		this.spearmasterNeedle_fadecounter = this.spearmasterNeedle_fadecounter_max; // 当前淡出计时器
		this.spearmasterNeedleType = UnityEngine.Random.Range(0, 3); // 随机选择针状物类型（0-2）

		// 自定义颜色（未设置，默认为null）
		this.jollyCustomColor = null;*/

	}

	private void Shatter()
	{
		AllGraspsLetGoOfThisObject(true);
		abstractPhysicalObject.LoseAllStuckObjects();
		Destroy();
	}

	private static readonly object _locker = new object();

	public static void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
	{
		orig(self);

		lock (_locker)
		{
			if (PE != null && PE.Count > 0)
			{
				for (int i = 0; i < PE.Count; i++)
				{
					if (PE[i] != null)
					{
						PE[i].Update();
					}
				}

				for (int i = PE.Count - 1; i >= 0; i--)
				{
					if (PE[i] == null || (PE[i] != null && PE[i].Destroyed))
					{
						PE.RemoveAt(i);
					}
				}

			}
		}

	}

	// 修改方法签名，添加递归深度参数
	public bool Explode(int depth = 0)
	{
		// 添加深度限制
		const int MAX_RECURSION_DEPTH = 5;
		if (depth > MAX_RECURSION_DEPTH)
		{
			Shatter();
			return false;
		}

		if (thrownBy != null && (thrownBy.room != null && !thrownBy.inShortcut))
		{
			PE.Add(new ParticleEffect(thrownBy.room, thrownBy.mainBodyChunk.pos, true));

			thrownBy.mainBodyChunk.vel = Vector2.zero;
			if (thrownBy is Player player)
			{
				player.SuperHardSetPosition(firstChunk.pos);
                //player.standing = false;
                if (PlayerModuleManager.playerModules.TryGetValue(player, out var module))
                {
                    module.Teleport = true;
                }

            }
			else
			{
				SuperHardSetPosition(thrownBy, firstChunk.pos);
			}
			thrownBy.mainBodyChunk.vel = Vector2.zero;


			if (soundID_Teleport1 != null && soundID_Teleport2 != null)
			{
				if (1 == UnityEngine.Random.Range(1, 3))
					thrownBy.room.PlaySound(soundID_Teleport1, thrownBy.mainBodyChunk.pos);
				else
					thrownBy.room.PlaySound(soundID_Teleport2, thrownBy.mainBodyChunk.pos);
			}
			//thrownBy.room.PlaySound(soundID_iceShieldCraft, thrownBy.mainBodyChunk.pos);
			//thrownBy.room.PlaySound(soundID_Teleport1, thrownBy.mainBodyChunk.pos);

			PE.Add(new ParticleEffect(thrownBy.room, thrownBy.mainBodyChunk.pos, false));

			Shatter();
			return true;
		}
		else if (room != null)
		{
			Creature? creature = Plugin.RandomlySelectedCreature(room, true, null, false);
			if (creature != null && creature.room != null && !creature.inShortcut)
			{
				thrownBy = creature;
				return Explode(depth + 1);
			}
		}
		Shatter();
		return false;
	}

	public void SuperHardSetPosition(Creature creature, Vector2 pos)
	{
		for (int i = 0; i < creature.bodyChunks.Length; i++)
		{
			creature.bodyChunks[i].HardSetPosition(pos);
			/*for (int j = 0; j < 2; j++)
			{
				creature.graphicsModule.drawPositions[i, j] = pos;
			}*/
		}
		Teleport.SetObjectPosition(creature, firstChunk.pos);

		creature.bodyChunks[1].pos.x = creature.bodyChunks[0].pos.x - 1f;
		foreach (BodyPart bodyPart in creature.graphicsModule.bodyParts)
		{
			bodyPart.pos = pos;
			bodyPart.lastPos = pos;
		}
	}


	public override void Update(bool eu)
	{
		base.Update(eu);


		/*if (PE1 != null)
		{
			PE1.Update();
		}
		if (PE2 != null)
		{
			PE2.Update();
		}*/

		if (this.burn > 0f)
		{
			this.burn -= 0.033333335f;
			if (this.burn <= 0f)
			{
				Explode();

			}
		}
	}

	public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
	{
		if (result.obj == null)
		{
			return false;
		}

		vibrate = 20;
		ChangeMode(Mode.Free);
		/*if (result.obj is Creature creature)
		{
			//将生物击退
			creature.firstChunk.vel += firstChunk.vel.normalized;
		}*/
		if (result.obj != null)
		{
			Explode();
		}

		return true;
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		if (this.ignited)
		{
			Explode();
		}

		base.TerrainImpact(chunk, direction, speed, firstContact);
	}

	public override void WeaponDeflect(Vector2 inbetweenPos, Vector2 deflectDir, float bounceSpeed)
	{
		base.WeaponDeflect(inbetweenPos, deflectDir, bounceSpeed);
		if (UnityEngine.Random.value < 0.5f)
		{
			Explode();
			return;
		}
		this.ignited = true;
		this.InitiateBurn();
	}

	public override void HitByWeapon(Weapon weapon)
	{
		/*if (weapon.mode == Weapon.Mode.Thrown && this.thrownBy == null && weapon.thrownBy != null)
		{
			this.thrownBy = weapon.thrownBy;
		}*/
		base.HitByWeapon(weapon);
		this.InitiateBurn();
	}

	public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
	{
		base.HitByExplosion(hitFac, explosion, hitChunk);
		if (UnityEngine.Random.value < hitFac)
		{
			/*if (this.thrownBy == null && explosion != null)
			{
				this.thrownBy = explosion.killTagHolder;
			}*/
			this.InitiateBurn();
		}
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
		bodyChunks[0].collideWithSlopes = false;
		this.ignited = true;
	}

	public void InitiateBurn()
	{
		if (this.burn == 0f)
		{
			this.burn = UnityEngine.Random.value;
			//this.room.PlaySound(SoundID.Fire_Spear_Ignite, base.firstChunk, false, 0.5f, 1.4f);
			base.firstChunk.vel += Custom.RNV() * UnityEngine.Random.value * 6f;
			return;
		}
		this.burn = Mathf.Min(this.burn, UnityEngine.Random.value);
	}



	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{

		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("icon_EnderPearl", false);
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
		sLeaser.sprites[0].x = pos.x - camPos.x;
		sLeaser.sprites[0].y = pos.y - camPos.y;

		sLeaser.sprites[0].scaleX = scale;
		sLeaser.sprites[0].scaleY = scale;

		if (slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}


	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
	{
		newContainer ??= rCam.ReturnFContainer("Items");

		foreach (FSprite fsprite in sLeaser.sprites)
		{
			fsprite.RemoveFromContainer();
			newContainer.AddChild(fsprite);
		}
	}

}