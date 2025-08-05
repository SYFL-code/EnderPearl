using Fisobs.Properties;
using System.Linq;
using static Player;

namespace EnderPearl;

sealed class EnderPearlProperties : ItemProperties
{
    // TODO scavenger elite support TODO 清道夫精英支持
    public override void Throwable(Player player, ref bool throwable)
        => throwable = true;

    public override void ScavCollectScore(Scavenger scavenger, ref int score)
        => score = 6;

    public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
        => score = 6;

    // Don't throw shields 不要扔盾牌 => score = 0;
    public override void ScavWeaponUseScore(Scavenger scav, ref int score)
        => score = 1;

    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        grabability = Player.ObjectGrabability.OneHand;

        // BigOneHand 大一只手
        // CantGrab   无法抓取
        // Drag       拖拽
        // OneHand     一只手
        // TwoHands   两只手


        // The player can only grab one EnderPearl at a time,
        // 玩家一次只能抓取一个百分之盾，
        // but that shouldn't prevent them from grabbing a spear,
        // 但这不应该阻止他们抓住长矛，
        // so don't use Player.ObjectGrabability.BigOneHand
        // 所以不要使用 Player.ObjectGrabability.BigOneHand

        /*if (player.grasps.Any(g => g?.grabbed is EnderPearl)) {
            grabability = Player.ObjectGrabability.CantGrab;
        } else {
            grabability = Player.ObjectGrabability.OneHand;
        }*/
    }
}
