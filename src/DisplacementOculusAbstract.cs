using Fisobs.Core;
using UnityEngine;

namespace DisplacementOculuses;

sealed class DisplacementOculusAbstract : AbstractPhysicalObject
{
    public DisplacementOculusAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, DisplacementOculusFisob.DisplacementOculus, null, pos, ID)
    {
        //scaleX = DisplacementOculus.scale;
        //scaleY = DisplacementOculus.scale;
        saturation = 0.5f;
        hue = 1f;
    }

    public override void Realize()
    {
        base.Realize();
        if (realizedObject == null)
            realizedObject = new DisplacementOculus(this, Room.realizedRoom.MiddleOfTile(pos.Tile));
    }

    public float hue;
    public float saturation;
    public float scaleX;
    public float scaleY;
    //public float damage;

    /*public override string ToString()
    {
        return this.SaveToString($"{hue};{saturation};{scaleX};{scaleY};{damage}");
    }*/
}
