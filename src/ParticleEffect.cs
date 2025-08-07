using UnityEngine;
using System.Collections.Generic;
using RWCustom;
using HUD;

namespace EnderPearl
{
    public class ParticleEffect
    {
        private class Particle
        {
            public FContainer container;
            public Vector2 velocity;
            public Vector2 worldPosition;
            public LightSource lightSource;
            public float timer = 0f;
            public bool isActive = true;
        }

        private List<Particle> particles = new List<Particle>();
        private FContainer masterContainer;

        public float pixelSize = 2f;
        private const float maxLifetime = 480f; // 480帧 
        private float currentLifetime = 0f;
        public bool Destroyed = false;

        private Vector2 targetWorldPos;
        private bool reverse;
        private Room room;

        public ParticleEffect(Room room, Vector2 targetWorldPos, bool reverse)
        {
            this.targetWorldPos = targetWorldPos;
            this.reverse = reverse;
            this.room = room;

            try
            {
                // 创建主容器
                masterContainer = new FContainer();
                Futile.stage.AddChild(masterContainer);

                int particleCount = UnityEngine.Random.Range(15, 30);

                for (int i = 0; i < particleCount; i++)
                {
                    CreateParticle();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"粒子效果初始化失败: {e.Message}");
                Destroy();
                throw;
            }
        }

        private void CreateParticle()
        {
            Particle particle = new Particle();
            particle.container = new FContainer();
            masterContainer.AddChild(particle.container);
            particle.container.alpha = 0f; // 初始透明
            particle.container.isVisible = true;

            // 生成随机粒子形状
            GenerateRandomParticle(particle.container);
            particle.container.rotation = 90 * UnityEngine.Random.Range(0, 4);

            // 初始化位置和速度
            InitializeParticlePosition(particle);

            particles.Add(particle);
        }

        private void InitializeParticlePosition(Particle particle)
        {
            Vector2 cameraPos = room.game.cameras[0].pos;

            float TWO_PT = Mathf.PI * 2f;
            if (reverse)
            {
                // 反向粒子：从远处向目标汇聚
                float angle = UnityEngine.Random.Range(0f, TWO_PT);
                float radius = Mathf.Sqrt(UnityEngine.Random.Range(30f * 30f, 300f * 300f));

                // 世界坐标位置
                particle.worldPosition = targetWorldPos + new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle)
                ) * radius;

                // 速度方向指向目标
                particle.velocity = (targetWorldPos - particle.worldPosition).normalized *
                                   UnityEngine.Random.Range(radius / 100, radius / 50); // 初始较快速度
            }
            else
            {
                // 正向粒子：从目标向外扩散
                float angle = UnityEngine.Random.Range(0f, TWO_PT);
                float radius = UnityEngine.Random.Range(0f, 30f);

                // 世界坐标位置
                particle.worldPosition = targetWorldPos + new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle)
                ) * radius;

                // 速度方向向外扩散
                particle.velocity = (particle.worldPosition - targetWorldPos).normalized *
                                   UnityEngine.Random.Range(1f, 5f); // 初始较快速度
            }

            // 设置屏幕位置
            UpdateScreenPosition(particle, cameraPos);
        }

        public void Update()
        {
            if (Destroyed) return;

            Vector2 cameraPos = room.game.cameras[0].pos;
            Rect cameraRect = new Rect(
                cameraPos.x - 1000f,
                cameraPos.y - 1000f,
                Custom.rainWorld.options.ScreenSize.x + 2000f,
                Custom.rainWorld.options.ScreenSize.y + 2000f
            );

            bool anyActive = false;
            currentLifetime += 1f;

            for (int i = particles.Count - 1; i >= 0; i--)
            {
                Particle particle = particles[i];

                if (!particle.isActive) continue;

                // 更新粒子计时器
                particle.timer += 1f;

                // 屏幕外检测
                if (!cameraRect.Contains(particle.worldPosition))
                {
                    RemoveParticle(particle);
                    continue;
                }

                // 更新位置和速度
                UpdateParticlePosition(particle);

                // 更新屏幕位置
                UpdateScreenPosition(particle, cameraPos);

                // 更新光源
                UpdateLightSource(particle);

                // 更新透明度
                UpdateParticleAlpha(particle);

                anyActive = true;
            }

            // 检查是否所有粒子都已销毁
            if (!anyActive || currentLifetime >= maxLifetime)
            {
                Destroy();
            }
        }

        private void UpdateParticlePosition(Particle particle)
        {
            // 粒子生命周期阶段划分（基于计时器）
            if (particle.timer < 100f)
            {
                // 第一阶段：0-100帧 - 快速移动
                // 保持初始速度
            }
            else if (particle.timer < 120f)
            {
                // 第二阶段：100-120帧 - 减速
                float slowdownFactor = 1f - (particle.timer - 100f) / 20f; // 从1到0线性减速
                particle.velocity *= slowdownFactor;
            }
            else
            {
                // 第三阶段：120+帧 - 停止
                particle.velocity = Vector2.zero;
            }

            // 更新世界位置
            particle.worldPosition += particle.velocity;
        }

        private void UpdateScreenPosition(Particle particle, Vector2 cameraPos)
        {
            // 世界坐标转屏幕坐标
            Vector2 screenPos = new Vector2(
                particle.worldPosition.x - cameraPos.x,
                particle.worldPosition.y - cameraPos.y
            );

            particle.container.SetPosition(screenPos);
        }

        private void UpdateLightSource(Particle particle)
        {
            Vector2 cameraPos = room.game.cameras[0].pos;
            Vector2 screenPos = new Vector2(
                particle.worldPosition.x - cameraPos.x,
                particle.worldPosition.y - cameraPos.y
            );

            if (particle.lightSource == null)
            {
                // 使用正确的世界坐标创建光源
                particle.lightSource = new LightSource(
                    screenPos,
                    false,
                    new Color(209f / 255f, 69f / 255f, 247f / 255f),
                    null
                );

                if (room != null)
                {
                    room.AddObject(particle.lightSource);
                }
            }
            else
            {
                // 更新光源位置（使用世界坐标）
                particle.lightSource.setPos = screenPos;
                particle.lightSource.requireUpKeep = true;

                // 设置光源半径（根据生命周期调整）
                float radius = 0f;
                if (particle.timer < 100f)
                {
                    // 0-100帧：光源半径逐渐增大
                    radius = 50f * (particle.timer / 100f);
                }
                else if (particle.timer < 120f)
                {
                    // 100-120帧：保持最大半径
                    radius = 50f;
                }
                else
                {
                    // 120+帧：光源半径随透明度减小
                    radius = 50f * particle.container.alpha;
                }

                particle.lightSource.setRad = radius;
                particle.lightSource.stayAlive = true;

                // 更新光源透明度
                particle.lightSource.setAlpha = particle.container.alpha;

                // 检查光源是否需要销毁
                if (particle.lightSource.slatedForDeletetion || particle.lightSource.room != room)
                {
                    particle.lightSource.Destroy();
                    particle.lightSource = null;
                }
            }
        }

        private void UpdateParticleAlpha(Particle particle)
        {
            if (particle.timer < 30f)
            {
                // 0-30帧：快速显示（淡入）
                particle.container.alpha = particle.timer / 30f;
            }
            else if (particle.timer < 120f)
            {
                // 30-120帧：保持完全可见
                particle.container.alpha = 1f;
            }
            else
            {
                // 120+帧：缓慢消失（淡出）
                float fadePhase = (particle.timer - 120f) / 360f;
                particle.container.alpha = Mathf.Max(0f, 1f - fadePhase);

                // 完全透明后标记为不活跃
                if (particle.container.alpha <= 0f)
                {
                    particle.isActive = false;
                }
            }
        }

        private void RemoveParticle(Particle particle)
        {
            if (particle.container != null)
            {
                masterContainer.RemoveChild(particle.container);
                particle.container = null;
            }

            if (particle.lightSource != null)
            {
                particle.lightSource.Destroy();
                particle.lightSource = null;
            }

            particles.Remove(particle);
        }

        public void Destroy()
        {
            if (Destroyed) return;

            Destroyed = true;

            // 销毁所有粒子
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                RemoveParticle(particles[i]);
            }

            // 销毁主容器
            if (masterContainer != null)
            {
                Futile.stage.RemoveChild(masterContainer);
                masterContainer = null;
            }
        }

        private void GenerateRandomParticle(FContainer container)
        {
            int shapeType = UnityEngine.Random.Range(0, 4);

            switch (shapeType)
            {
                case 0:
                    CreatePixel(container, Vector2.zero);
                    break;

                case 1:
                    CreateShape1(container);
                    break;

                case 2:
                    CreateShape2(container);
                    break;

                default:
                    CreatePixel(container, Vector2.zero);
                    break;
            }
        }

        private void CreatePixel(FContainer container, Vector2 position)
        {
            FSprite pixel = new FSprite("pixel")
            {
                width = pixelSize,
                height = pixelSize,
                color = new Color(209f / 255f, 69f / 255f, 247f / 255f, 1f),
                x = position.x,
                y = position.y
            };
            container.AddChild(pixel);
        }

        private void CreateShape1(FContainer container)
        {
            Vector2[] positions = {
                new Vector2(0, 0),
                new Vector2(pixelSize, 0),
                new Vector2(pixelSize, pixelSize),
                new Vector2(pixelSize, pixelSize * 2),
                new Vector2(0, pixelSize * 2),
                new Vector2(-pixelSize, pixelSize * 2),
                new Vector2(-pixelSize * 2, pixelSize * 2),
                new Vector2(-pixelSize * 2, pixelSize),
                new Vector2(-pixelSize * 2, 0),
                new Vector2(-pixelSize * 2, -pixelSize),
                new Vector2(-pixelSize * 2, -pixelSize * 2),
                new Vector2(-pixelSize, -pixelSize * 2),
                new Vector2(0, -pixelSize * 2),
                new Vector2(pixelSize, -pixelSize * 2)
            };

            foreach (Vector2 pos in positions)
            {
                CreatePixel(container, pos);
            }
        }

        private void CreateShape2(FContainer container)
        {
            Vector2[] positions = {
                new Vector2(0, 0),
                new Vector2(pixelSize * 2, 0),
                new Vector2(-pixelSize * 2, 0),
                new Vector2(pixelSize, pixelSize),
                new Vector2(-pixelSize, pixelSize),
                new Vector2(pixelSize * 2, pixelSize * 2),
                new Vector2(-pixelSize * 2, pixelSize * 2),
                new Vector2(0, pixelSize * 2),
                new Vector2(pixelSize, -pixelSize),
                new Vector2(-pixelSize, -pixelSize),
                new Vector2(pixelSize * 2, -pixelSize * 2),
                new Vector2(-pixelSize * 2, -pixelSize * 2),
                new Vector2(0, -pixelSize * 2)
            };

            foreach (Vector2 pos in positions)
            {
                CreatePixel(container, pos);
            }
        }
    }
}



/*using On;
using IL;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Mono.Cecil;
using MoreSlugcats;
using RWCustom;
using HUD;
using Smoke;
using static PhysicalObject;
using UnityEngine;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;
using SlugBase.Features;
using System.Diagnostics;
using ImprovedInput;
using System.Linq;
using RewiredConsts;
using Menu.Remix;
using MonoMod.RuntimeDetour;
using Watcher;
using static MonoMod.InlineRT.MonoModRule;
using Kittehface.Framework20;
using System.Threading;
using System.Linq.Expressions;


namespace EnderPearl
{
	public class ParticleEffect
	{

		private FContainer[] fContainer;
		private Vector2[] newvel;
		private LightSource[] lightSource;
		//private readonly FSprite pointerSprite;
		//private readonly FSprite circleSprite;

		public float pixelSize = 5; // 每个"像素"的大小

		private float camX = 300f;
		private float camY = 300f;

		private float lifeTime;
		private float lifeTime_max = 20;
		private float disappearTime = 0.2f;

		public bool Destroy = false;

		private Vector2 targetWorldPos;
		private bool reverse;
		private Room newroom;

		public ParticleEffect(Room room1, Vector2 targetworldpos, bool reverse1)
		{
			targetWorldPos = targetworldpos;
			reverse = reverse1;
			newroom = room1;
			lifeTime = lifeTime_max;
			// 预计算π值避免重复计算
			const float TWO_PI = Mathf.PI * 2f;

			try
			{
				// 1. 创建显示容器
				int random = Random(15, 30);
				fContainer = new FContainer[random];
				newvel = new Vector2[random];
				lightSource = new LightSource[random];

				for (int i = 0; i < fContainer.Length; i++)
				{
					Futile.stage.AddChild(fContainer[i]);
					fContainer[i].alpha = 1f; // 初始透明
					fContainer[i].isVisible = true;
					RandomParticle(fContainer[i]);
					fContainer[i].rotation = 90 * Random(0, 4);

					camX = newroom.game.cameras[0].pos.x;
					camY = newroom.game.cameras[0].pos.y;
					if (reverse)
					{
						float angle = UnityEngine.Random.Range(0f, TWO_PI);  // 1. 生成随机角度
						float radius = Mathf.Sqrt(UnityEngine.Random.Range(100f * 100f, 300f * 300f));
						Vector2 ownerWorldPos = targetWorldPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
						Vector2 direction = (targetWorldPos - ownerWorldPos).normalized;
						newvel[i] = direction * UnityEngine.Random.Range(5f, 10f); // 适当提高速度

						Vector2 ownerScreenPos = new Vector2(ownerWorldPos.x - camX, ownerWorldPos.y - camY);
						fContainer[i].SetPosition(ownerScreenPos);
					}
					else
					{
						float randomAngle = UnityEngine.Random.Range(0f, TWO_PI);
						float radius = UnityEngine.Random.Range(0f, 30f); // 添加径向偏移

						Vector2 spawnOffset = new Vector2(
							Mathf.Cos(randomAngle) * radius,
							Mathf.Sin(randomAngle) * radius
						);

						Vector2 ownerWorldPos = targetWorldPos + spawnOffset;
						Vector2 ownerScreenPos = new Vector2(ownerWorldPos.x - camX, ownerWorldPos.y - camY);

						Vector2 direction = (targetWorldPos - ownerWorldPos).normalized;
						newvel[i] = -direction * UnityEngine.Random.Range(5f, 20f);

						*//*Vector2 vector;
						vector.x = Random(-1.5f, 0f) * (Random(0, 2) * 2 - 1);
						vector.y = Random(-1.5f, 0f) * (Random(0, 2) * 2 - 1);
						Vector2 ownerWorldPos = targetWorldPos + vector;
						Vector2 direction = (targetWorldPos - ownerWorldPos).normalized;
						newvel[i] = -direction * UnityEngine.Random.Range(5f, 20f);

						Vector2 ownerScreenPos = new Vector2(targetWorldPos.x - camX, targetWorldPos.y - camY);*//*
						fContainer[i].SetPosition(ownerScreenPos);
					}
				}

			}
			catch(Exception e)
			{
				Debug.LogError($"ParticleEffect初始化失败: {e.Message}");
				// 失败时确保清理资源
				if (fContainer != null && fContainer.Length > 0)
				{
					for (int i = 0; i < fContainer.Length; i++)
					{
						if (fContainer[i] != null)
						{
							fContainer[i].RemoveFromContainer();
						}
					}
				}
				throw; // 重新抛出异常让上层处理
			}

		}

		public void Update()
		{
			camX = newroom.game.cameras[0].pos.x;
			camY = newroom.game.cameras[0].pos.y;

			if (Destroy)
			{
				return;
			}

			if (lifeTime < -10 && fContainer != null)
			{
				for (int i = 0; i < fContainer.Length; i++)
				{
					Disappear(i);
				}
				fContainer = null;
				Destroy = true;
				return;
			}

			if (fContainer != null)
			{
				for (int i = 0; i < fContainer.Length; i++)
				{
					if (fContainer[i] == null)
					{
						continue;
					}

					*//*Vector2 ownerScreenPos = new Vector2(fContainer[i].x, fContainer[i].x) + newvel[i];
					fContainer[i].SetPosition(ownerScreenPos);*//*
					// 修改1：始终使用世界坐标计算
					Vector2 worldPos = new Vector2(
						fContainer[i].x + camX + newvel[i].x,
						fContainer[i].y + camY + newvel[i].y
					);

					// 修改2：直接更新世界坐标
					fContainer[i].SetPosition(new Vector2(
                        worldPos.x - camX,
                        worldPos.y - camY
					));
					Vector2 ownerScreenPos = new Vector2(fContainer[i].x, fContainer[i].y);

					if (lightSource[i] == null)
					{
						lightSource[i] = new LightSource(ownerScreenPos, false, new UnityEngine.Color(209f / 255f, 69f / 255f, 247f / 255f), null);
						if (newroom != null)
						{
							newroom.AddObject(lightSource[i]);
						}
					}
					if (lightSource[i] != null)
					{
						lightSource[i].requireUpKeep = true;
						lightSource[i].setPos = ownerScreenPos;
						if (reverse)
							lightSource[i].setRad = (lifeTime_max - lifeTime) * 50f / lifeTime_max;
						else
							lightSource[i].setRad = lifeTime * 50f / lifeTime_max;
						lightSource[i].stayAlive = true;
						lightSource[i].setAlpha = 1f;
						if (lightSource[i].slatedForDeletetion || lightSource[i].room != newroom)
							lightSource[i] = null;
					}

					//Vector2 ownerScreenPos = new Vector2(ownerWorldPos.x - camX, ownerWorldPos.y - camY);
					Vector2 ownerWorldPos = new Vector2(ownerScreenPos.x + camX, ownerScreenPos.y + camY);
					float distanceToCenter = Vector2.Distance(ownerWorldPos, targetWorldPos);
					if (reverse)
					{
						// 当粒子接近中心时消失
						if (distanceToCenter < 10f || lifeTime <= 0) // 适当增加消失半径
						{
							Disappear(i);
						}
						else
						{
							// 增加向中心加速效果
							newvel[i] += (targetWorldPos - ownerWorldPos).normalized * 0.5f;
						}
					}
					else
					{
						if (lifeTime <= 0)
							Disappear(i);
					}

					if (reverse)
						fContainer[i].alpha = (lifeTime_max - lifeTime) / (lifeTime_max * disappearTime);
					else
						fContainer[i].alpha = lifeTime / (lifeTime_max * disappearTime);

					// 在Update中增加：超出屏幕范围提前销毁
					*//*Rect cameraRect = new Rect(camX - 100, camY - 100,
											 Custom.rainWorld.options.ScreenSize.x + 200,
											 Custom.rainWorld.options.ScreenSize.y + 200);

					if (!cameraRect.Contains(worldPos))
					{
						Disappear(i);
						return;
					}*//*

				}
			}

			lifeTime--;
		}

		private void Disappear(int i)
		{
			if (fContainer[i] != null)
			{
				// 关键：从父容器移除
				fContainer[i].isVisible = false;
				fContainer[i].RemoveFromContainer();
				Futile.stage.RemoveChild(fContainer[i]);
				fContainer[i] = null;
			}
			if (lightSource[i] != null)
			{
				lightSource[i].Destroy();
				lightSource[i] = null;
			}
			fContainer[i] = null;
		}

		private void RandomParticle(FContainer Container)
		{
			FSprite[] pointerSprite = new FSprite[50];

			switch (Random(0, 4))
			{
				case 0:
					pointerSprite[0] = new FSprite("pixel")
					{
						width = pixelSize,
						height = pixelSize,
						color = new UnityEngine.Color(209 / 255, 69 / 255, 247 / 255, 1f),
						x = 0,
						y = 0
					};

					Container.AddChild(pointerSprite[0]);
					break;
				case 1:
					Vector2[] poss1 = {
						new Vector2(0, 0),
						new Vector2(pixelSize, 0),
						new Vector2(pixelSize, pixelSize),
						new Vector2(pixelSize, pixelSize * 2),
						new Vector2(0, pixelSize * 2),
						new Vector2(-pixelSize, pixelSize * 2),
						new Vector2(-pixelSize * 2, pixelSize * 2),
						new Vector2(-pixelSize * 2, pixelSize),
						new Vector2(-pixelSize * 2, 0),
						new Vector2(-pixelSize * 2, -pixelSize),
						new Vector2(-pixelSize * 2, -pixelSize * 2),
						new Vector2(-pixelSize, -pixelSize * 2),
						new Vector2(0, -pixelSize * 2),
						new Vector2(pixelSize, -pixelSize * 2)
					};
					for (int i = 0; i < poss1.Length; i++)
					{
						pointerSprite[i] = new FSprite("pixel")
						{
							width = pixelSize,
							height = pixelSize,
							color = new UnityEngine.Color(209 / 255, 69 / 255, 247 / 255, 1f),
							x = poss1[i].x,
							y = poss1[i].y
						};
						Container.AddChild(pointerSprite[i]);
					}
					break;
				case 2:
					Vector2[] poss2 = {
						new Vector2(0, 0),

						new Vector2(pixelSize * 2, 0),
						new Vector2(-pixelSize * 2, 0),
						new Vector2(pixelSize, pixelSize),
						new Vector2(-pixelSize, pixelSize),
						new Vector2(pixelSize * 2, pixelSize * 2),
						new Vector2(-pixelSize * 2, pixelSize * 2),
						new Vector2(0, pixelSize * 2),

						new Vector2(pixelSize, -pixelSize),
						new Vector2(-pixelSize, -pixelSize),
						new Vector2(pixelSize * 2, -pixelSize * 2),
						new Vector2(-pixelSize * 2, -pixelSize * 2),
						new Vector2(0, -pixelSize * 2),
					};
					for (int i = 0; i < poss2.Length; i++)
					{
						pointerSprite[i] = new FSprite("pixel")
						{
							width = pixelSize,
							height = pixelSize,
							color = new UnityEngine.Color(209 / 255, 69 / 255, 247 / 255, 1f),
							x = poss2[i].x,
							y = poss2[i].y
						};
						Container.AddChild(pointerSprite[i]);
					}
					break;

				default:
					pointerSprite[0] = new FSprite("pixel")
					{
						width = pixelSize,
						height = pixelSize,
						color = new UnityEngine.Color(209 / 255, 69 / 255, 247 / 255, 1f),
						x = 0,
						y = 0
					};

					Container.AddChild(pointerSprite[0]);
					break;
			}

		}

		private int Random(int min, int max)
		{
			return UnityEngine.Random.Range(min, max);
		}
		private float Random(float min, float max)
		{
			return UnityEngine.Random.Range(min, max);
		}

	}
}*/
