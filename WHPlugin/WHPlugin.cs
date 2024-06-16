using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;

namespace WHPlugin;
public class WHPlugin : BasePlugin
{
    public override string ModuleName => "WHPlugin";
    public override string ModuleVersion => "1.0.0";

    public bool wh = false;
    public bool roundStarted = false;
    public int teamGlow = 0;
    public int teamwh =0;
    public bool switchNextRound = false;

    public CCSGameRules? gamerules;
    public Dictionary<int?, CBaseModelEntity?> playerGlows = [];
    public int halves;


     public override void Load(bool hotReload)
    {

        /*RegisterEventHandler<EventPlayerSpawn>((@event, info)=>
        {
            
            
            if(wh == true && roundStarted == true)
            {
                if(@event.Userid.PlayerPawn.Value.TeamNum == teamGlow && @event.Userid.PlayerPawn.IsValid)
                   {
                    if(playerGlows.ContainsKey(@event.Userid.UserId)) 
                    {
                        playerGlows[@event.Userid.UserId].AcceptInput("kill");
                        playerGlows.Remove(@event.Userid.UserId);
                    }

                    Console.WriteLine("Spawn ! glow "+@event.Userid.Team);
                    
                    var _modelRelay = SetGlowing(@event.Userid.PlayerPawn.Value, teamwh);
                    playerGlows.Add(@event.Userid.UserId, _modelRelay);
                    


                }
            
            }
             
            
            return HookResult.Continue;
        },HookMode.Post);*/

        RegisterEventHandler<EventCsIntermission>((@event, info)=>
        {
            Console.WriteLine("Match fini");
            return HookResult.Continue;
         });

        
        RegisterEventHandler<EventWarmupEnd>((@event, info)=>
        {
            roundStarted = false;
            return HookResult.Continue;
         });
        
        RegisterEventHandler<EventPlayerDeath>((@event, info) =>
        {
            if(playerGlows.ContainsKey(@event.Userid.UserId) == true)
            {
            if(playerGlows[@event.Userid.UserId].IsValid)playerGlows[@event.Userid.UserId].AcceptInput("kill");
            playerGlows.Remove(@event.Userid.UserId);
            }
            return HookResult.Continue;
        },HookMode.Post);

        RegisterEventHandler<EventRoundEnd>((@event, info)=>{
            //playerGlows.Clear();
            roundStarted = false;
            return HookResult.Continue;
        },HookMode.Post);

        RegisterEventHandler<EventRoundPoststart>((@event, info)=>{
            if(wh == true)
            {
                
                IEnumerable<CCSPlayerController> controllers = Utilities.GetPlayers();
                foreach(CCSPlayerController controller in controllers)
                    {
                    
                        if(controller.PlayerPawn.Value.TeamNum == teamGlow && controller.IsValid && controller.PlayerPawn.IsValid)
                        {
                             if(playerGlows.ContainsKey(controller.UserId)) 
                                {
                                    //playerGlows[controller.UserId].AcceptInput("kill");
                                    playerGlows.Remove(controller.UserId);
                                                                    
                                }
                            Console.WriteLine("Spawn ! glow "+controller.PlayerPawn.Value.Index+" Team : "+ controller.Team);
                            var _modelRelay = SetGlowing(controller.PlayerPawn.Value, teamwh);
                            playerGlows.Add(controller.UserId, _modelRelay);
                           
                        }   

                    }
                if(switchNextRound == true) 
                {   invertTeams();
                    switchNextRound = false;
                }
            }
            roundStarted = true;
            return HookResult.Continue;
        },HookMode.Pre);

        RegisterEventHandler<EventRoundOfficiallyEnded>((@event, info)=>{
            playerGlows.Clear();
            return HookResult.Continue;
        },HookMode.Post);

        RegisterEventHandler<EventRoundAnnounceLastRoundHalf>((@event, info)=>{
            //Console.WriteLine("LAST ROUND !!!!");
            switchNextRound = true;
            return HookResult.Continue;
        },HookMode.Post);

        RegisterEventHandler<EventRoundAnnounceMatchStart>((@event, info)=>{
            Console.WriteLine("Match Start !!!!");
            return HookResult.Continue;
        },HookMode.Post);
        
        
        RegisterEventHandler<EventWarmupEnd>((@event, info)=>{
            Console.WriteLine("Warmup ?");
            playerGlows.Clear();
            whOff();
            return HookResult.Continue;
        },HookMode.Post);

    }

    // Permissions can be added to commands using the `RequiresPermissions` attribute.
    // See the admin documentation for more information on permissions.
    
    [ConsoleCommand("css_wh", "activates wallhack")]
    [RequiresPermissions("@css/admin")]
    public void OnCommand(CCSPlayerController? playerController, CommandInfo commandInfo)
    {
        if(wh == false)
        {
            
            whOn(playerController.PlayerPawn.Value.TeamNum);
            Console.WriteLine("Wallhack on for team " + playerController.Team);

        }
        else
        {
            
            wh = false;
            foreach(int id in playerGlows.Keys)
            {
                var player = Utilities.GetPlayerFromUserid(id);
                if(player.IsValid)
                {
                    if(playerGlows[id].IsValid)playerGlows[id].AcceptInput("kill");
                    playerGlows.Remove(id);
                    
                }
            }
            Console.WriteLine("Wallhack off");

        }
    }
    public void invertTeams()
    {
        if(teamwh == 2)
        {
            teamwh =3;
            teamGlow =2;
        }
        else
        {
            teamwh = 2;
            teamGlow =3;
        }
    }
    public void whOn(int teamID)
    {
        wh = true;
        teamwh = teamID;
        if (teamwh == 2)
        {
            teamGlow =3;
        } 
        else 
        {
            teamGlow = 2;
        }
        IEnumerable<CCSPlayerController> controllers = Utilities.GetPlayers();
        foreach(CCSPlayerController controller in controllers)
        {
            
            if(controller.PlayerPawn.Value.TeamNum == teamGlow && controller.IsValid && controller.PawnIsAlive)
            {
                var _modelRelay = SetGlowing(controller.PlayerPawn.Value, teamID);
                if(playerGlows.ContainsKey(controller.UserId)) 
                    {playerGlows.Remove(controller.UserId);}
                else playerGlows.Add(controller.UserId, _modelRelay);
            }   
        }

    }

    public void whOff()
    {
        foreach(int id in playerGlows.Keys)
            {
                var player = Utilities.GetPlayerFromUserid(id);
                if(player.IsValid)
                {
                    playerGlows[id].AcceptInput("kill");
                    playerGlows.Remove(id);
                    
                }
            }
    }
    public CBaseModelEntity SetGlowing(CCSPlayerPawn pawn, int team)
    {
        CBaseModelEntity? modelGlow = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic");
        CBaseModelEntity? modelRelay = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic");

        if (modelGlow == null || modelRelay == null)
        {
            return null;
        }
        string modelName = pawn.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;
        Console.Write(modelName);
        modelRelay.SetModel(modelName);
        modelRelay.Spawnflags = 256u;
        modelRelay.RenderMode = RenderMode_t.kRenderNone;
        modelRelay.DispatchSpawn();

        modelGlow.SetModel(modelName);
        modelGlow.Spawnflags = 256u;
        modelGlow.Glow.Glowing = true;
        //modelGlow.Glow.Flashing =true;    
        modelGlow.DispatchSpawn();

        modelGlow.Glow.GlowColorOverride = Color.Red;
        modelGlow.Glow.GlowRange = 5000;
        modelGlow.Glow.GlowTeam = team;
        modelGlow.Glow.GlowType = 3;
        modelGlow.Glow.GlowRangeMin = 15;

        modelRelay.AcceptInput("FollowEntity", pawn, modelRelay, "!activator");
        modelGlow.AcceptInput("FollowEntity", modelRelay, modelGlow, "!activator");
        return modelRelay;
    }
    /*public CBaseModelEntity SetGlowing(CCSPlayerPawn pawn, int team)
    {
    AddTimer(0.005f, () =>
                {
                    var prop = Utilities.CreateEntityByName<CCSPlayerPawn>("prop_dynamic");
                    prop.SetModel(pawn.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName);
                    prop!.Teleport(pawn.AbsOrigin, new QAngle(0, 0, 0), new Vector(0, 0, 0));
                    prop.AcceptInput("FollowEntity", caller: prop, activator: pawn, value: "!activator");
                    prop.DispatchSpawn();

                    prop.Render = Color.FromArgb(1, 255, 255, 255);
                    prop.Glow.GlowColorOverride = Color.Red;
                    prop.Spawnflags = 256U;
                    prop.RenderMode = RenderMode_t.kRenderGlow;
                    prop.Glow.GlowRange = 5000;
                    prop.Glow.GlowTeam = team;
                    prop.Glow.GlowType = 3;
                    prop.Glow.GlowRangeMin = 3;
                    AddTimer(0.05f, () =>
                    {
                        prop.Remove();
                    });


                },TimerFlags.REPEAT);
                return null;
    }*/


    public static CCSGameRules GetGameRules()
    {
        return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
    }

 
}
