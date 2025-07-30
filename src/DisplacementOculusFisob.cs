using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;

namespace DisplacementOculuses;

sealed class DisplacementOculusFisob : Fisob
{
    public static readonly AbstractPhysicalObject.AbstractObjectType DisplacementOculus = new("DisplacementOculus", true);
    //public static readonly MultiplayerUnlocks.SandboxUnlockID RedDisplacementOculus = new("RedDisplacementOculus", true);
    public static readonly MultiplayerUnlocks.SandboxUnlockID ODisplacementOculus = new("ODisplacementOculus", true);

    public DisplacementOculusFisob() : base(DisplacementOculus)
    {
        // Fisobs auto-loads the `icon_DisplacementOculus` embedded resource as a texture.
        // Fisobs 将“icon_DisplacementOculus”嵌入资源作为纹理自动加载。
        // See `DisplacementOculuss.csproj` for how you can add embedded resources to your project.
        // 请参阅“DisplacementOculuss.csproj”，了解如何将嵌入式资源添加到项目中。

        // If you want a simple grayscale icon, you can omit the following line.
        // 如果您想要一个简单的灰度图标，可以省略以下行。

        Icon = new DisplacementOculusIcon();

        SandboxPerformanceCost = new(linear: 0.35f, exponential: 0f);

        RegisterUnlock(ODisplacementOculus, parent: MultiplayerUnlocks.SandboxUnlockID.BigCentipede, data: 70);
        //RegisterUnlock(RedDisplacementOculus, parent: MultiplayerUnlocks.SandboxUnlockID.RedCentipede, data: 0);
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
    {

        var result = new DisplacementOculusAbstract(world, saveData.Pos, saveData.ID);

        return result;
    }

    /*public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
    {
        // Centi shield data is just floats separated by ; characters.
        // Centi 盾牌数据只是浮点数，以 ;字符。
        string[] p = saveData.CustomData.Split(';');

        if (p.Length < 5) {
            p = new string[5];
        }

        var result = new DisplacementOculusAbstract(world, saveData.Pos, saveData.ID) {
            hue = float.TryParse(p[0], out var h) ? h : 0,
            saturation = float.TryParse(p[1], out var s) ? s : 1,
            scaleX = float.TryParse(p[2], out var x) ? x : 1,
            scaleY = float.TryParse(p[3], out var y) ? y : 1,
            damage = float.TryParse(p[4], out var r) ? r : 0
        };

        // If this is coming from a sandbox unlock, the hue and size should depend on the data value (see DisplacementOculusIcon below).
        // 如果这来自沙盒解锁，则色调和大小应取决于数据值（请参见下面的DisplacementOculusicon）。
        if (unlock is SandboxUnlock u) {
            result.hue = u.Data / 1000f;

            if (u.Data == 0) {
                result.scaleX += 0.2f;
                result.scaleY += 0.2f;
            }
        }

        return result;
    }*/

    /*private static readonly DisplacementOculusProperties properties = new();

    public override ItemProperties Properties(PhysicalObject forObject)
    {
        // If you need to use the forObject parameter, pass it to your ItemProperties class's constructor.
        // 如果您需要使用ForoBject参数，请将其传递到您的ItemProperties类的构造函数。
        // The Mosquitoes example demonstrates this.
        // 蚊子的例子证明了这一点。
        return properties;
    }*/
}
