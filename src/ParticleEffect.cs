using On;
using IL;
using System;
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

        private readonly FContainer[] fContainer;
        //private readonly FSprite pointerSprite;
        //private readonly FSprite circleSprite;

        public float pixelSize = 5; // 每个"像素"的大小

        public ParticleEffect()
        {
            try
            {
                // 1. 创建显示容器
                fContainer = new FContainer[Random(15, 30)];
                for (int i = 0; i < fContainer.Count(); i++)
                {
                    Futile.stage.AddChild(fContainer[i]);
                    fContainer[i].alpha = 1f; // 初始透明
                    fContainer[i].isVisible = true;
                    RandomParticle(fContainer[i]);
                }

            }
            catch(Exception e)
            {
                Debug.LogError($"ParticleEffect初始化失败: {e.Message}");
                // 失败时确保清理资源
                if (fContainer != null && fContainer.Count() > 0)
                {
                    for (int i = 0; i < fContainer.Count(); i++)
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

        private void RandomParticle(FContainer Container)
        {
            FSprite[] pointerSprite = new FSprite[20];

            switch (Random(0, 4))
            {
                case 0:
                    pointerSprite[0] = new FSprite("pixel")
                    {
                        width = pixelSize,
                        height = pixelSize,
                        color = new Color(209 / 255, 69 / 255, 247 / 255, 1f), // 初始半透明
                        x = 0,
                        y = 0
                    };

                    Container.AddChild(pointerSprite[0]);
                    break;
                case 1:
                    float[,] poss = {
                    {0, 0},
                    {pixelSize, 0},
                    {pixelSize, pixelSize},
                    {pixelSize, pixelSize * 2},
                    {0, pixelSize * 2},
                    {-pixelSize, pixelSize * 2}
                    ,{-pixelSize * 2, pixelSize * 2}
                    ,{-pixelSize * 2, pixelSize}
                    ,{-pixelSize * 2, 0}
                    ,{-pixelSize * 2, -pixelSize}
                    ,{-pixelSize * 2, -pixelSize * 2}
                    ,{-pixelSize, -pixelSize * 2}
                    ,{0, -pixelSize * 2}
                    ,{pixelSize, -pixelSize * 2}
                    };

                    for (int ID = 0; ID < poss; ID++)
                    {

                    }

                    //int ID = 0;
                    pointerSprite[ID] = new FSprite("pixel")
                    {
                        width = pixelSize,
                        height = pixelSize,
                        color = new Color(209 / 255, 69 / 255, 247 / 255, 1f), // 初始半透明
                        x = 0,
                        y = 0
                    };
                    Container.AddChild(pointerSprite[ID]);
                    ID += 1;

                    break;


                default:
                    pointerSprite[0] = new FSprite("pixel")
                    {
                        width = pixelSize,
                        height = pixelSize,
                        color = new Color(209 / 255, 69 / 255, 247 / 255, 1f), // 初始半透明
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
}
