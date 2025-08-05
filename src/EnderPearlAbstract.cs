using Fisobs.Core;
using UnityEngine;

namespace EnderPearl;

sealed class EnderPearlAbstract : AbstractPhysicalObject
{
    public EnderPearlAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, EnderPearlFisob.EnderPearl, null, pos, ID)
    {
        scaleX = 1;
        scaleY = 1;
        saturation = 0.5f;
        hue = 1f;
    }

    public override void Realize()
    {
        base.Realize();
        if (realizedObject == null)
            realizedObject = new EnderPearl(this, Room.realizedRoom.MiddleOfTile(pos.Tile));
    }

    public float hue;
    public float saturation;
    public float scaleX;
    public float scaleY;
    public float damage;

    /*public override string ToString()
    {
        return this.SaveToString($"{hue};{saturation};{scaleX};{scaleY};{damage}");
    }*/
}
