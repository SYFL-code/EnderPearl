using RWCustom;
using System.Diagnostics;
using System;
using Watcher;
using UnityEngine;
using System.Collections.Generic;


namespace EnderPearl
{
    public class ParticleEffect1 : CosmeticSprite
    {
        public static string[] ParticleName = { "Particle1", "Particle2", "Particle3", "Particle4", "Particle5", "Particle6", "Particle7", "Particle8", "Particle9" };
        public static string[] snowName = { "snow1", "snow2", "snow3", "snow4", "snow5", "snow6", "snow7" };

        private float lifeTime;
        private float lifeTime_max = 20;
        private float disappearTime = 0.2f;
        private LightSource? lightSource = null;

        private Vector2 CenterPos;
        private Vector2 direction;

        private bool reverse = false;//反转雪花运动 向目标中心汇聚

        //参数
        //public delegate Vector2 GetTracePos_Snow(Player self);
        //private GetTracePos_Snow? getTracePos = null;
        //private Player? getTracePos_arg1;

        public static void HookTexture()
        {
            //加载雪花贴图
            Futile.atlasManager.LoadAtlas("atlases/snow1");
            Futile.atlasManager.LoadAtlas("atlases/snow2");
            Futile.atlasManager.LoadAtlas("atlases/snow3");
            Futile.atlasManager.LoadAtlas("atlases/snow4");
            Futile.atlasManager.LoadAtlas("atlases/snow5");
            Futile.atlasManager.LoadAtlas("atlases/snow6");
            Futile.atlasManager.LoadAtlas("atlases/snow7");
            //加载贴图
            /*Futile.atlasManager.LoadAtlas("atlases/Particle1");
            Futile.atlasManager.LoadAtlas("atlases/Particle2");
            Futile.atlasManager.LoadAtlas("atlases/Particle3");
            Futile.atlasManager.LoadAtlas("atlases/Particle4");
            Futile.atlasManager.LoadAtlas("atlases/Particle5");
            Futile.atlasManager.LoadAtlas("atlases/Particle6");
            Futile.atlasManager.LoadAtlas("atlases/Particle7");
            Futile.atlasManager.LoadAtlas("atlases/Particle8");
            Futile.atlasManager.LoadAtlas("atlases/Particle9");*/
        }

        public ParticleEffect1(Vector2 setPos, bool reverse)
        {
            this.reverse = reverse;
            this.CenterPos = setPos;

            if (reverse)
            {
                // 修改1：在远处生成粒子（离中心位置较远）
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2);
                float distance = UnityEngine.Random.Range(100f, 300f); // 加大距离确保从外向内

                pos = setPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;

                // 修改2：反转方向计算逻辑（使用 pos - setPos 来获得正确的向内速度）
                direction = (CenterPos - pos).normalized;
                vel = direction * UnityEngine.Random.Range(15f, 25f); // 适当提高速度
            }
            else
            {
                // 保持原来的放射状逻辑
                Vector2 vector;
                vector.x = UnityEngine.Random.Range(-1.5f, 0f) * (UnityEngine.Random.Range(0, 2) * 2 - 1);
                vector.y = UnityEngine.Random.Range(-1.5f, 0f) * (UnityEngine.Random.Range(0, 2) * 2 - 1);

                pos = setPos + vector;
                direction = (CenterPos - pos).normalized;
                vel = -direction * UnityEngine.Random.Range(5f, 20f);
            }

            lifeTime = lifeTime_max;
            lastPos = pos;  // 修正：初始化 lastPos 为当前 pos

            /*this.reverse = reverse;

            Vector2 vector;
            if (reverse)
            {
                vector.x = UnityEngine.Random.Range(-20f, 20f);
                vector.y = UnityEngine.Random.Range(-20f, 20f);
            }
            else
            {
                vector.x = UnityEngine.Random.Range(-1.5f, 0f) * (UnityEngine.Random.Range(0, 2) * 2 - 1);
                vector.y = UnityEngine.Random.Range(-1.5f, 0f) * (UnityEngine.Random.Range(0, 2) * 2 - 1);
            }

            //雪花
            CenterPos = setPos;
            //从周围随机半径角度产生雪花
            pos = setPos;
            pos += vector;

            direction = (setPos - pos).normalized;

            if (reverse)
            {
                vel = direction * UnityEngine.Random.Range(5f, 20f);
            }
            else
            {
                vel = -direction * UnityEngine.Random.Range(5f, 20f);
            }

            lifeTime = lifeTime_max;
            lastPos = setPos;*/
        }

        /*public ParticleEffect(Vector2 pos, Vector2 decVel, bool reverse = false)
        {
            this.reverse = reverse;
            lifeTime = lifeTime_max;
            this.pos = pos;
            lastPos = pos;
            vel.x = UnityEngine.Random.Range(-10f, 10f);
            vel.y = UnityEngine.Random.Range(-10f, 10f);
            vel -= decVel;
        }*/

        public static string GetParticleStringRandom()
        {
            return snowName[UnityEngine.Random.Range(0, 6)];
            //return ParticleName[UnityEngine.Random.Range(0, 9)];
        }

        private readonly CustomAtlases customAtlases;

        private FAtlas ParticleAtlas;
        private FSprite[] sprites;

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            string ParticleString = GetParticleStringRandom();

            this.ParticleAtlas = this.customAtlases.LoadAtlases(ParticleString);

            sLeaser.sprites = new FSprite[1];
            this.sprites = sLeaser.sprites;

            sLeaser.sprites[0] = (this.sprites[0] = new FSprite(ParticleString, true));
            //sLeaser.sprites[0] = new FSprite(GetParticleStringRandom());
            FContainer fcontainer = rCam.ReturnFContainer("HUD");
            fcontainer.AddChild(sLeaser.sprites[0]);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            Vector2 v = Vector2.Lerp(lastPos, pos, timeStacker);
            v -= Vector2.Lerp(rCam.lastPos, rCam.pos, timeStacker);
            sLeaser.sprites[0].x = v.x;
            sLeaser.sprites[0].y = v.y;
            if (reverse)
                sLeaser.sprites[0].alpha = (lifeTime_max - lifeTime) / (lifeTime_max * disappearTime);
            else
                sLeaser.sprites[0].alpha = lifeTime / (lifeTime_max * disappearTime);
        }

        /*private bool DisappearDistance()
        {
            //Vector2 dstPos = getTracePos(getTracePos_arg1);
            Vector2 dstPos = CenterPos;
            return (dstPos - pos).sqrMagnitude <= Mathf.Max(1.0f, vel.sqrMagnitude);
        }*/

        private void Disappear()
        {
            if (lightSource != null)
                lightSource.Destroy();
            Destroy();
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            /*bool ad = reverse && getTracePos != null;
            if (ad)
            {
                //改变能量方向
                Vector2 dstPos = getTracePos(getTracePos_arg1);
                //追踪坐标
                Vector2 v1 = (dstPos - pos).normalized;
                vel = vel.magnitude * v1;
            }*/

            //vel *= 0.75f * lifeTime / lifeTime_max;

            if (lightSource == null)
            {
                lightSource = new LightSource(pos, false, new Color(209f / 255f, 69f / 255f, 247f / 255f), null);
                room.AddObject(lightSource);
            }
            if (lightSource != null)
            {
                lightSource.requireUpKeep = true;
                lightSource.setPos = pos;
                if (reverse)
                    lightSource.setRad = (lifeTime_max - lifeTime) * 50f / lifeTime_max;
                else
                    lightSource.setRad = lifeTime * 50f / lifeTime_max;
                lightSource.stayAlive = true;
                lightSource.setAlpha = 1f;
                if (lightSource.slatedForDeletetion || lightSource.room != room)
                    lightSource = null;
            }

            // 修改3：优化消失条件，添加距离检测
            float distanceToCenter = Vector2.Distance(pos, CenterPos);

            if (reverse)
            {
                // 当粒子接近中心时消失
                if (distanceToCenter < 10f || lifeTime <= 0) // 适当增加消失半径
                {
                    Disappear();
                }
                else
                {
                    // 增加向中心加速效果
                    vel += (CenterPos - pos).normalized * 0.5f;
                }
            }
            else
            {
                if (lifeTime <= 0)
                    Disappear();
            }

            lifeTime--;

            /*if (lifeTime > 0)
                lifeTime--;
            //没到时间但是已经在距离内
            if (reverse && Vector2.Distance(pos, CenterPos) < 4)
                Disappear();
            if (lifeTime <= 0)
            {
                if (reverse)
                {
                    //Disappear();
                    if (Vector2.Distance(pos, CenterPos) < 4)
                        Disappear();
                    else
                        vel *= 1.1f;
                }
                else
                    Disappear();
            }*/
        }





    }
}
