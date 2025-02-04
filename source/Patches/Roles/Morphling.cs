using Hazel;
using System;
using TownOfUs.Extensions;
using TownOfUs.Patches.Buttons;
using TownOfUs.Roles.Modifiers;
using TownOfUs.Utility;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Morphling : Role

    {
        public PlayerControl SampledPlayer;
        public ModdedButton SampleButton;
        public ModdedButton MorphButton;
        public ModdedButton VentButton;

        public Morphling(PlayerControl player) : base(player)
        {
            Name = "Morphling";
            ImpostorText = () => "Transform into crewmates";
            TaskText = () => "Morph into crewmates to be disguised";
            Color = Patches.Colors.Impostor;
            RoleType = RoleEnum.Morphling;
            Faction = Faction.Impostors;

            SampleButton = new ModdedButton(player);
            SampleButton.ButtonType = ButtonType.AbilityButton;
            SampleButton.ButtonTarget = ButtonTarget.Player;
            SampleButton.Sprite = TownOfUs.SampleSprite;
            SampleButton.UseDefault = false;
            SampleButton.SetPosition(HudAlignment.BottomLeft, 0);
            SampleButton.SetCooldown(button => CustomGameOptions.MorphlingCd);
            SampleButton.SetAction(button =>
            {
                SampledPlayer = button.ClosestPlayer;
                return false;
            });
            SampleButton.RegisterButton();

            MorphButton = new ModdedButton(player);
            MorphButton.ButtonType = ButtonType.AbilityButton;
            MorphButton.ButtonTarget = ButtonTarget.None;
            MorphButton.Sprite = TownOfUs.MorphSprite;
            MorphButton.UseDefault = false;
            MorphButton.SetPosition(HudAlignment.BottomLeft, 1);
            MorphButton.SetCooldown(button => CustomGameOptions.MorphlingCd);
            MorphButton.SetDuration(button => CustomGameOptions.MorphlingDuration);
            MorphButton.SetAction(MorphAction);
            MorphButton.SetActionEnd(MorphActionEnd);
            MorphButton.SetEnabled(MorphActionEnable);
            MorphButton.RegisterButton();

            GenerateKillButton();

            VentButton = new ModdedButton(player);
            VentButton.ButtonType = ButtonType.VentButton;
            VentButton.UseDefault = true;
            VentButton.SetShow(button => false);
            VentButton.RegisterButton();

        }

        public bool MorphAction(ModdedButton button)
        {
            RpcSetMorph(SampledPlayer);
            Morph(Player, SampledPlayer);
            return false;
        }

        public void MorphActionEnd(ModdedButton button)
        {
            RpcSetMorph(Player);
            Morph(Player, Player);
        }

        public bool MorphActionEnable(ModdedButton button)
        {
            if (!ModdedButton.DefaultEnabled(button)) return false;
            if (SampledPlayer == null) return false;
            return true;
        }

        public void RpcSetMorph(PlayerControl target)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
            (byte)CustomRPC.Morph,
            SendOption.Reliable, -1);
            writer.Write(Player.PlayerId);
            writer.Write(target.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void Morph(PlayerControl morphling, PlayerControl target)
        {
            var morphlingRole = Role.GetRole(morphling);
            var targetModifier = Modifier.GetModifier(target);

            if(morphling == target)
            {
                morphlingRole.OverrideSize = false;
                morphlingRole.OverrideSpeed = false;
            } else if(targetModifier != null)
            {
                morphlingRole.SpeedFactor = targetModifier.SpeedFactor;
                morphlingRole.SizeFactor = targetModifier.SizeFactor;
                morphlingRole.OverrideSize = true;
                morphlingRole.OverrideSpeed = true;
            } else
            {
                morphlingRole.SpeedFactor = 1;
                morphlingRole.SizeFactor = Modifier.DefaultSize;
                morphlingRole.OverrideSize = true;
                morphlingRole.OverrideSpeed = true;
            }
            Utils.Morph(morphling, target.Data.DefaultOutfit);
        }
    }
}