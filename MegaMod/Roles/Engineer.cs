using HarmonyLib;
using Hazel;
using Essentials.CustomOptions;

public class Engineer : Role
{
    public static CustomToggleOption optShowEngineer = CustomOption.AddToggle("Show Engineer", false);
    public static CustomNumberOption optEngineerSpawnChance = CustomOption.AddNumber("Engineer Spawn Chance", 100, 0, 100, 5);

    public bool repairUsed = false;
    public bool showEngineer = false;
    public bool sabotageActive { get; set; }

    public Engineer(PlayerControl player) {
        this.player = player;
        this.name = "Engineer";
        this.color = new Color(255f / 255f, 165f / 255f, 10f / 255f, 1);
        this.startText = "Maintain important systems on the ship";
    }


    public static void ClearSettings()
    {
        this.player = null;
        repairUsed = false;
    }

    public static void SetConfigSettings()
    {
        showEngineer = HarmonyMain.optShowEngineer.GetValue();
    }
    

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowInfectedMap))]
    class EngineerMapOpen
    {
        static void Postfix(MapBehaviour __instance)
        {
            if (player != null)
            {
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    if (__instance.IsOpen)
                    {
                        __instance.ColorControl.baseColor = Engineer.color;
                        foreach (MapRoom room in __instance.infectedOverlay.rooms)
                        {
                            if (room.door != null)
                            {
                                room.door.enabled = false;
                                room.door.gameObject.SetActive(false);
                                room.door.gameObject.active = false;
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
    class EngineerMapUpdate
    {
        static void Postfix(MapBehaviour __instance)
        {
            if (player != null)
            {
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    if (__instance.IsOpen && __instance.infectedOverlay.gameObject.active)
                    {
                        if (!sabotageActive)
                            __instance.ColorControl.baseColor = Color.gray;
                        else
                            __instance.ColorControl.baseColor = color;
                        float perc = repairUsed ? 1f : 0f;
                        foreach (MapRoom room in __instance.infectedOverlay.rooms)
                        {
                            if (room.special != null)
                            {
                                if (!sabotageActive)
                                    room.special.material.SetFloat("_Desat", 1f);
                                else
                                    room.special.material.SetFloat("_Desat", 0f);
                                room.special.enabled = true;
                                room.special.gameObject.SetActive(true);
                                room.special.gameObject.active = true;
                                if (!PlayerControl.LocalPlayer.Data.IsDead)
                                    room.special.material.SetFloat("_Percent", perc);
                                else
                                    room.special.material.SetFloat("_Percent", 1f);
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.Method_41))]
    class SabotageButtonDeactivatePatch
    {
        static bool Prefix(MapRoom __instance, float DCEFKAOFGOG)
        {
            if (player != null)
            {
                if (player != null && player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageReactor))]
    class SabotageReactorPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            if (player != null && player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                if (!repairUsed && sabotageActive && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    repairUsed = true;
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageLights))]
    class SabotageLightsPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            if (player != null && player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                if (!repairUsed && sabotageActive && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    repairUsed = true;
                    SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FixLights, Hazel.SendOption.None, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageComms))]
    class SabotageCommsPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            if (player != null && player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                if (!repairUsed && sabotageActive && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    repairUsed = true;
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageOxygen))]
    class SabotageOxyPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            if (player != null && player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                if (!repairUsed && sabotageActive && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    repairUsed = true;
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageSeismic))]
    class SabotageSeismicPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            if (player != null && player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                if (!repairUsed && sabotageActive && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    repairUsed = true;
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);
                }
                return false;
            }
            return true;
        }
    }

    public void setIntro(IntroCutscene.CoBegin__d __instance)
    {
        if (PlayerControl.LocalPlayer == this.player)
        {
            __instance.__this.Title.Text = this.name;
            __instance.__this.Title.Color = this.color;
            __instance.__this.ImpostorText.Text = "Maintain important systems on the ship";
            __instance.__this.BackgroundBar.material.color = this.color;
        }
    }
}