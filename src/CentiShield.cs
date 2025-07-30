using RWCustom;
using UnityEngine;

namespace CentiShields;

sealed class CentiShield : Weapon
{
    private static float Rand => Random.value;

    public static float scale = 1f;

    public bool ignited;

    private float burn;

    public CentiShieldAbstract Abstr { get; }

    public static void HookTexture()
    {
        Futile.atlasManager.LoadAtlas("icon_CentiShield");
    }

    public CentiShield(CentiShieldAbstract abstr, Vector2 pos) : base(abstr, abstr.world)
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
    }

    private void Shatter()
    {
        AllGraspsLetGoOfThisObject(true);
        abstractPhysicalObject.LoseAllStuckObjects();
        Destroy();
    }

    public void Explode()
    {
        if (thrownBy != null && (thrownBy.room != null && !thrownBy.inShortcut))
        {
            Teleport.SetObjectPosition(thrownBy, firstChunk.pos);

        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (this.burn > 0f)
        {
            this.burn -= 0.033333335f;
            if (this.burn <= 0f)
            {
                if (thrownBy != null && (thrownBy.room != null && !thrownBy.inShortcut))
                {
                    Teleport.SetObjectPosition(thrownBy, firstChunk.pos);
                    //WindList.WindExplosioListAdd(room, thrownBy, firstChunk.pos, true);
                    //使武器消失
                    Shatter();
                }

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
        if (result.obj is Creature creature)
        {
            //将生物击退
            creature.firstChunk.vel += firstChunk.vel.normalized;
        }
        if (result.obj != null)
        {
            if (thrownBy != null && (thrownBy.room != null && !thrownBy.inShortcut))
            {
                Teleport.SetObjectPosition(thrownBy, firstChunk.pos);
            }
            //WindList.WindExplosioListAdd(room, thrownBy, firstChunk.pos, true);
            //使武器消失
            Shatter();
        }

        return true;
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        if (this.ignited)
        {
            if (thrownBy != null && (thrownBy.room != null && !thrownBy.inShortcut))
            {
                Teleport.SetObjectPosition(thrownBy, firstChunk.pos);
            }
            //WindList.WindExplosioListAdd(room, thrownBy, firstChunk.pos, true);
            //使武器消失
            Shatter();
        }

        base.TerrainImpact(chunk, direction, speed, firstContact);
    }

    public override void WeaponDeflect(Vector2 inbetweenPos, Vector2 deflectDir, float bounceSpeed)
    {
        base.WeaponDeflect(inbetweenPos, deflectDir, bounceSpeed);
        if (UnityEngine.Random.value < 0.5f)
        {
            if (thrownBy != null && (thrownBy.room != null && !thrownBy.inShortcut))
            {
                Teleport.SetObjectPosition(thrownBy, firstChunk.pos);
                //WindList.WindExplosioListAdd(room, thrownBy, firstChunk.pos, true);
                //使武器消失
                Shatter();
                return;
            }
        }
        this.ignited = true;
        this.InitiateBurn();
    }

    public override void HitByWeapon(Weapon weapon)
    {
        if (weapon.mode == Weapon.Mode.Thrown && this.thrownBy == null && weapon.thrownBy != null)
        {
            this.thrownBy = weapon.thrownBy;
        }
        base.HitByWeapon(weapon);
        this.InitiateBurn();
    }

    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
    {
        base.HitByExplosion(hitFac, explosion, hitChunk);
        if (UnityEngine.Random.value < hitFac)
        {
            if (this.thrownBy == null && explosion != null)
            {
                this.thrownBy = explosion.killTagHolder;
            }
            this.InitiateBurn();
        }
    }

    public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
        this.ignited = true;
    }

    public void InitiateBurn()
    {
        if (this.burn == 0f)
        {
            this.burn = UnityEngine.Random.value;
            this.room.PlaySound(SoundID.Fire_Spear_Ignite, base.firstChunk, false, 0.5f, 1.4f);
            base.firstChunk.vel += Custom.RNV() * UnityEngine.Random.value * 6f;
            return;
        }
        this.burn = Mathf.Min(this.burn, UnityEngine.Random.value);
    }



    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("icon_CentiShield", false);
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