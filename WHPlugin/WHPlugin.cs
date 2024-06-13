﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

using System.Drawing;

namespace WHPlugin;
public class WHPlugin : BasePlugin
{
    public override string ModuleName => "WHPlugin";
    public override string ModuleVersion => "1.0.0";

     public override void Load(bool hotReload)
    {
        RegisterListener<CounterStrikeSharp.API.Core.Listeners.OnEntitySpawned()
    }
    [ListenerName("OnEntitySpawned")]
    public delegate void OnEntitySpawned(CEntityInstance entity)
    {
        Write.Console("Spawn !");
    }

      
    // Permissions can be added to commands using the `RequiresPermissions` attribute.
    // See the admin documentation for more information on permissions.
    [ConsoleCommand("css_wh", "activates wallhack")]
    public void OnCommand(CCSPlayerController? playerController, CommandInfo commandInfo)
    {
        Console.WriteLine("Ok 1!");
        int playerNum = playerController.PlayerPawn.Value.TeamNum;
        Console.WriteLine(playerNum);
        int enemyNum;
        if (playerNum == 2) enemyNum = 3;
        else enemyNum = 2;
        Console.WriteLine("Ok 2!");
        IEnumerable<CCSPlayerController> controllers = Utilities.GetPlayers();
        Console.WriteLine("Ok 3! ");
        foreach(CCSPlayerController controller in controllers)
        {
            Console.WriteLine(controller.PlayerPawn.Value.TeamNum);
            if (controller.PlayerPawn.Value.TeamNum == enemyNum && controller.IsValid)
                {SetGlowing(controller.PlayerPawn.Value, playerNum);}
            else Console.WriteLine("Error");
        }

    }


    public void SetGlowing(CCSPlayerPawn pawn, int team)
    {
        CBaseModelEntity? modelGlow = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic");
        CBaseModelEntity? modelRelay = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic");
        if (modelGlow == null || modelRelay == null)
        {
            return;
        }

        string modelName = pawn.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;

        modelRelay.SetModel(modelName);
        modelRelay.Spawnflags = 256u;
        modelRelay.RenderMode = RenderMode_t.kRenderNone;
        modelRelay.DispatchSpawn();

        modelGlow.SetModel(modelName);
        modelGlow.Spawnflags = 256u;
        modelGlow.DispatchSpawn();

        modelGlow.Glow.GlowColorOverride = Color.Red;
        modelGlow.Glow.GlowRange = 5000;
        modelGlow.Glow.GlowTeam = team;
        modelGlow.Glow.GlowType = 3;
        modelGlow.Glow.GlowRangeMin = 100;

        modelRelay.AcceptInput("FollowEntity", pawn, modelRelay, "!activator");
        modelGlow.AcceptInput("FollowEntity", modelRelay, modelGlow, "!activator");
    }

 
}
