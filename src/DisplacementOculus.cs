using RWCustom;
using UnityEngine;

namespace DisplacementOculuses;

sealed class DisplacementOculus : Weapon
{
    private static float Rand => Random.value;

    public static float scale = 0.4f;

    public DisplacementOculusAbstract Abstr { get; }

    public static void HookTexture()
    {
        Futile.atlasManager.LoadAtlas("icon_DisplacementOculus");
    }

    public DisplacementOculus(DisplacementOculusAbstract abstr, Vector2 pos) : base(abstr, abstr.world)
    {
        Abstr = abstr;

        //Abstr.scaleX = scale;
        //Abstr.scaleY = scale;

        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, pos, 6f * (scale + scale), 0.07f);
        //bodyChunks = new[] { new BodyChunk(this, 0, pos + vel, 4 * (Abstr.scaleX + Abstr.scaleY), 0.35f) { goThroughFloors = true } };
        bodyChunks[0].lastPos = bodyChunks[0].pos;

        firstChunk.loudness = 9f; // 响度
        firstChunk.pos = pos;

        bodyChunkConnections = new BodyChunkConnection[0];
        airFriction = 0.999f; // 空气摩擦
        gravity = 0.3f; // 重力
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

    public override void Update(bool eu)
    {
        base.Update(eu);
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
        if (mode == Mode.Thrown)
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




    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("icon_DisplacementOculus", false);
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

    /*public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];
        sLeaser.sprites[0] = new FSprite("CentipedeBackShell", true);
        sLeaser.sprites[1] = new FSprite("CentipedeBackShell", true);
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        float num = Mathf.InverseLerp(305f, 380f, timeStacker);
        pos.y -= 20f * Mathf.Pow(num, 3f);
        float num2 = Mathf.Pow(1f - num, 0.25f);
        lastDarkness = darkness;
        darkness = rCam.room.Darkness(pos);
        darkness *= 1f - 0.5f * rCam.room.LightSourceExposure(pos);

        for (int i = 0; i < 2; i++)
        {
            sLeaser.sprites[i].x = pos.x - camPos.x;
            sLeaser.sprites[i].y = pos.y - camPos.y;
            sLeaser.sprites[i].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
            sLeaser.sprites[i].scaleY = num2 * Abstr.scaleY;
            sLeaser.sprites[i].scaleX = num2 * Abstr.scaleX;
        }

        sLeaser.sprites[0].color = blackColor;
        sLeaser.sprites[0].scaleY *= 1.175f - Abstr.damage * 0.2f;
        sLeaser.sprites[0].scaleX *= 1.175f - Abstr.damage * 0.2f;

        sLeaser.sprites[1].color = Color.Lerp(Custom.HSL2RGB(Abstr.hue, Abstr.saturation, 0.55f), blackColor, darkness);

        if (blink > 0 && Rand < 0.5f)
        {
            sLeaser.sprites[0].color = blinkColor;
        }
        else if (num > 0.3f)
        {
            for (int j = 0; j < 2; j++)
            {
                sLeaser.sprites[j].color = Color.Lerp(sLeaser.sprites[j].color, earthColor, Mathf.Pow(Mathf.InverseLerp(0.3f, 1f, num), 1.6f));
            }
        }

        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }*/

    /*public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
        earthColor = Color.Lerp(palette.fogColor, palette.blackColor, 0.5f);
    }*/

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");

        foreach (FSprite fsprite in sLeaser.sprites) {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);
        }
    }
}