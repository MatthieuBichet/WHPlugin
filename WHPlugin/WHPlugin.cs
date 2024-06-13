using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;

namespace WHPlugin;
public class WHPlugin : BasePlugin
{
    public override string ModuleName => "WHPlugin";
    public override string ModuleVersion => "1.0.0";

    
    public bool wh = false;
    public int teamGlow = 0;
    public int teamwh =0;


     public override void Load(bool hotReload)
    {


        RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            Console.WriteLine("spawn ! wh : "+teamwh+" glow "+teamGlow);
            if(wh == true)
            {   
                IEnumerable<CCSPlayerController> controllers = Utilities.GetPlayers();
                foreach(CCSPlayerController controller in controllers)
                {
                    Console.WriteLine(controller.UserId);
                    if (controller.PlayerPawn.Value.TeamNum == teamGlow)
                        {SetGlowing(controller.PlayerPawn.Value, teamwh);}
                }
            }
            return HookResult.Continue;
        }, HookMode.Post);
        RegisterEventHandler<EventStartHalftime>((@event, info)=>
        {
            if(teamGlow == 2)
            {
                teamGlow =3;
                teamwh =2;
            }
            else
            {
                teamGlow = 2;
                teamwh = 3;
            }
            Console.WriteLine("Halftime ! wh "+teamwh+" glow "+teamGlow);
            return HookResult.Continue;
        },HookMode.Post);
    }

    // Permissions can be added to commands using the `RequiresPermissions` attribute.
    // See the admin documentation for more information on permissions.
    [ConsoleCommand("css_wh", "activates wallhack")]
    public void OnCommand(CCSPlayerController? playerController, CommandInfo commandInfo)
    {
        if(wh == false)
        {
            initWH(playerController.PlayerPawn.Value.TeamNum);

        }
        else
        {
            IEnumerable<CBaseModelEntity> entities = Utilities.FindAllEntitiesByDesignerName<CBaseModelEntity>("prop_dynamic");
            foreach(var entity in entities)
            {
                entity.AcceptInput("kill");
            }
            wh = false;
        }
    }

    public void initWH(int teamID)
    {
        wh = true;
        Console.WriteLine("Ok 1!");
        int playerNum = teamID;
        teamwh = teamID;
        Console.WriteLine(playerNum);
        int enemyNum;
        if (playerNum == 2)
        {
            enemyNum = 3;
            teamGlow =3;
        } 
        else 
        {
            enemyNum = 2;
            teamGlow = 2;
        }

        /*
        Console.WriteLine("Ok 2!");
        IEnumerable<CCSPlayerController> controllers = Utilities.GetPlayers();
        Console.WriteLine("Ok 3! ");
        foreach(CCSPlayerController controller in controllers)
        {
            Console.WriteLine(controller.PlayerPawn.Value.TeamNum);
            if (controller.PlayerPawn.Value.TeamNum == enemyNum && controller.IsValid)
                {SetGlowing(controller.PlayerPawn.Value, playerNum);}
            else Console.WriteLine("Error");
        }*/

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
        modelGlow.Glow.GlowRangeMin = 50;

        modelRelay.AcceptInput("FollowEntity", pawn, modelRelay, "!activator");
        modelGlow.AcceptInput("FollowEntity", modelRelay, modelGlow, "!activator");
    }

 
}
