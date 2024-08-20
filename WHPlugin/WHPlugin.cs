using System.Drawing;
using System.Linq.Expressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using Serilog;

namespace WHPlugin;
public class WHPlugin : BasePlugin
{
    public override string ModuleName => "WHPlugin";
    public override string ModuleVersion => "1.0.1";

    public bool wh = false;
    public bool roundStarted = false;
    public int teamGlow = 0;
    public int teamwh =0;
    public bool switchNextRound = false;

    public Color glowColor = Color.FromArgb(255, 120, 0, 255);
    public Dictionary<int?, CBaseModelEntity?> playerGlows = [];
    //CheckTransmitPlayerSlot
    //private MemoryFunctionWithReturn <CBaseEntity, int, bool> CCSCheckTransmit_CanUserFunc = new(GameData.GetOffset("CheckTransmitPlayerSlot"));
     public override void Load(bool hotReload)
    {
        //CCSCheckTransmit_CanUserFunc.Hook(CheckTransmitPre, HookMode.Pre);

            HookEntityOutput("*", "settransmit", (CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay) =>
        {
            Console.WriteLine(name);
            return HookResult.Continue;
        });
        //CCSCheckTransmit_CanUserFunc.Hook(CheckTransmitPre, HookMode.Pre);
        


        RegisterEventHandler<EventPlayerSpawn>((@event, info)=>
        {
            var player = @event.Userid;
            
            var playerPawn = player?.PlayerPawn.Get();
            if (player == null || playerPawn == null) return HookResult.Continue;

    
            if(wh == true && roundStarted == true)
            {
                                  
                        playerPawn = player?.PlayerPawn.Get();
                        if(player!.TeamNum == teamGlow && playerPawn!.IsValid && player.PawnIsAlive == true && player.IsValid)
                        {
                            try
                        {
                            if(playerGlows.ContainsKey(player.UserId) == true) 
                            {
                                
                                if(playerGlows[player.UserId]!.IsValid)playerGlows[player.UserId]!.AcceptInput("kill");
                                
                                playerGlows.Remove(player.UserId);
                            }
                        
                            Logger.LogInformation("Spawn Live {team} glow Pawn id {id}", player.Team,  playerPawn.Index);
                            
                            var _modelRelay = SetGlowing(playerPawn, teamwh);
                            playerGlows.Add(player.UserId, _modelRelay);
                            }
                        catch(Exception e)
                        {
                            Logger.LogError("Error in livespawn : {logerror}", e.ToString());
                        }
                        }
                   
            }
        return HookResult.Continue;
        },HookMode.Post);
            

        RegisterEventHandler<EventRoundPrestart>((@event, info)=>
        {
            roundStarted = false;
            return HookResult.Continue;
         },HookMode.Post);

         RegisterListener<Listeners.OnMapStart>(mapName =>
         {
            playerGlows.Clear();
            roundStarted = false;
            switchNextRound = false;
            Logger.LogInformation("New map started, var reset");
         });
        
        
        RegisterEventHandler<EventCsIntermission>((@event, info)=>
        {
            //Console.WriteLine("Match fini");
            roundStarted = false;
            playerGlows.Clear();
            return HookResult.Continue;
         },HookMode.Post);

         /*
        //Triggered when player is switching team
         RegisterEventHandler<EventPlayerTeam>((@event, info)=>
        {
            Console.WriteLine("Teamswap");
            var player = @event.Userid;
            var playerPawn = player?.PlayerPawn.Get();
            if (player == null || playerPawn == null) return HookResult.Continue;
            if(playerGlows.ContainsKey(player.UserId) == true)
            {
                try
                {
                    if(playerGlows[player.UserId]!.IsValid)playerGlows[player.UserId]!.AcceptInput("kill");
                    playerGlows.Remove(player.UserId);
                    Console.WriteLine("Remove player => Swap team");
                }
                catch(Exception e)
                {
                    playerGlows.Remove(player.UserId);
                    Console.WriteLine("Error when switching team : " + e.Message);
                }
            }
            return HookResult.Continue;
        },HookMode.Post);*/
        
        
        RegisterEventHandler<EventPlayerDeath>((@event, info) =>
        {
            try
            {
            if(playerGlows.ContainsKey(@event.Userid!.UserId) == true)
            {
            if(playerGlows[@event.Userid.UserId]!.IsValid)playerGlows[@event.Userid.UserId]!.AcceptInput("kill");
            playerGlows.Remove(@event.Userid.UserId);
            }
            }
            catch (Exception e)
            {
                Logger.LogError("Error when player is killed : {exceptionmessage} ", e.ToString());
            }
            
            return HookResult.Continue;
        },HookMode.Post);


        RegisterEventHandler<EventRoundPoststart>((@event, info)=>{
            
            if(wh == true)
            {
                
                IEnumerable<CCSPlayerController> controllers = Utilities.GetPlayers();
                foreach(CCSPlayerController controller in controllers)
                    {
                        try{
                        if(controller.TeamNum == teamGlow && controller.IsValid && controller.PlayerPawn.IsValid && controller.PawnIsAlive == true)
                        {
                             if(playerGlows.ContainsKey(controller.UserId)== true) 
                                {
                                    if(playerGlows[controller.UserId]!.IsValid)playerGlows[controller.UserId]!.AcceptInput("kill");
                                    else Logger.LogError("Entity not valid during EventRoundPostStart");
                                    playerGlows.Remove(controller.UserId);                         
                                }
                            Logger.LogInformation("Spawn ! glow {ID} Team : {teamNumber}", controller.PlayerPawn.Value!.Index, controller.Team);
                            var _modelRelay = SetGlowing(controller.PlayerPawn.Value, teamwh);
                            playerGlows.Add(controller.UserId, _modelRelay);
                           
                        } 
                        }
                        catch(Exception e)
                        {
                            Logger.LogError("Issue on roundRestart : {exceptionmessage}", e.ToString());
                        }

                    }
            
                /*if(switchNextRound == true) 
                {   invertTeams();
                    switchNextRound = false;
                }*/
            }
            roundStarted = true;
            return HookResult.Continue;
        },HookMode.Pre);


        
        RegisterEventHandler<EventRoundOfficiallyEnded>((@event, info)=>{

            var rules = GetGameRules();
            var rounds = rules.TotalRoundsPlayed;
            var maxrounds = ConVar.Find("mp_maxrounds")!.GetPrimitiveValue<int>();
            var overtimeMaxrounds = ConVar.Find("mp_overtime_maxrounds")!.GetPrimitiveValue<int>();

            switch(rounds)
            {
                case var _ when (rounds == (maxrounds/2) && switchNextRound == true):
                invertTeams();
                break;
                case var _ when (rounds > maxrounds && (rounds - maxrounds) %(overtimeMaxrounds/2) == 0 && switchNextRound == true):
                invertTeams();
                break;
            }
            
            //Console.Write("Played : "+rounds + "MaxRounds : "+maxrounds);

            playerGlows.Clear();
            switchNextRound = false;
            roundStarted = false;
            return HookResult.Continue;
        },HookMode.Post);

        RegisterEventHandler<EventRoundAnnounceLastRoundHalf>((@event, info)=>{
            //Console.WriteLine("LAST ROUND !!!!");
            switchNextRound = true;
            return HookResult.Continue;
        },HookMode.Post);


        RegisterEventHandler<EventWarmupEnd>((@event, info)=>{
            //Console.WriteLine("Warmup ?");
            //whOff();
            roundStarted = false;
            playerGlows.Clear();
            return HookResult.Continue;
        },HookMode.Post);

    }


    /*[ConsoleCommand("css_read", "readings")]
    public void OnCommandRead(CCSPlayerController? caller, CommandInfo command)
    {
        var entitites = Utilities.FindAllEntitiesByDesignerName<CDynamicProp>("prop_dynamic");

        foreach (var entity in entitites)
        {
            var entname = entity.Entity!.Name;
            Server.PrintToChatAll($"{entity.Index}");

            Server.PrintToChatAll($"{entity.DesignerName}");

            Server.PrintToChatAll($"{entname}");

            //entity.AcceptInput("break");
        }
    }*/
 
     [ConsoleCommand("css_color", "change wallhack color")]
     [CommandHelper(minArgs: 3)]
     [RequiresPermissions("@css/admin")]
     public void onColorCommand(CCSPlayerController? caller, CommandInfo command)
    {
        glowColor = Color.FromArgb(255, Int32.Parse(command.ArgByIndex(1)),Int32.Parse(command.ArgByIndex(2)), Int32.Parse(command.ArgByIndex(3)));
    }
    [ConsoleCommand("css_wh", "activates wallhack")]
    [RequiresPermissions("@css/admin")]
    public void OnCommand(CCSPlayerController? playerController, CommandInfo commandInfo)
    {
        
        if(wh == false)
        {
            int teamNum = 0;
            if(commandInfo.ArgCount > 1)
            {
                var argTeam = commandInfo.ArgByIndex(1);
                switch(argTeam)
                {
                    case var _ when argTeam == "T" | argTeam == "t":
                    teamNum = 2; 
                    break;
                    case var _ when argTeam == "CT" | argTeam == "ct" | argTeam == "Ct" | argTeam == "cT":
                    teamNum = 3;
                    break;
                    default:
                    Logger.LogInformation("Plugin error when using command css_wh :  T or CT expected");
                    
                    return;
                }

            }
           else
           {
            teamNum = playerController!.TeamNum;
           }
            whOn(teamNum);
            if(teamNum == 2)
            {
                Logger.LogInformation("Wallhack on for team Terrorist.");
            }
            else if (teamNum == 3)
            {
                Logger.LogInformation("Wallhack on for team CounterTerrorist");
            }
            else
            {
                Logger.LogError("Error using wh command");
            }
            

        }
        else
        {
            
            wh = false;
            foreach(int id in playerGlows.Keys)
            {

                try
                {
                    var player = Utilities.GetPlayerFromUserid(id)!;
                    if(player.IsValid)
                    {
                        if(playerGlows[id]!.IsValid)playerGlows[id]!.AcceptInput("kill");
                        playerGlows.Remove(id);
                        
                    }
                }
                catch(Exception e)
                {
                    Logger.LogError("Error during WH command : {exceptionmessage}",e.ToString());
                    playerGlows.Remove(id);
                }
                
            
            }
            Logger.LogInformation("Wallhack off");

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
            var player = controller;
            var playerPawn = player?.PlayerPawn.Get();
            if (player != null || playerPawn != null)
                {
                try{
                if(playerPawn!.TeamNum == teamGlow && player!.IsValid && player.PawnIsAlive && playerPawn.IsValid)
                {
                    var _modelRelay = SetGlowing(playerPawn, teamID);
                    if(playerGlows.ContainsKey(player.UserId)) 
                        {playerGlows.Remove(player.UserId);}
                    else playerGlows.Add(player.UserId, _modelRelay);
                }   
                }
                catch(Exception e)
                    {Logger.LogError("Issue modelglow : {exceptionmessage}", e.ToString());}
                }
            }
    }


    private HookResult CheckTransmitPre(DynamicHook hook)
    {
    
    var player = hook.GetParam<CCSPlayerController>(0);
    Log.Debug("{player}", player);
    var slot = hook.GetParam<int>(0);
    Log.Debug("{slot}", slot);

    return HookResult.Continue;
    }
    public void whOff()
    {
        foreach(int id in playerGlows.Keys)
            {
                try
                {
                var player = Utilities.GetPlayerFromUserid(id)!;
                if(player!.IsValid)
                {
                    playerGlows[id]!.AcceptInput("kill");
                    playerGlows.Remove(id);
                    
                }
                }
                catch (Exception e)
                {
                    Logger.LogError("Issue when disabling glows : {exceptionmessage}",e.ToString());
                    playerGlows.Remove(id);
                }
            }
    }

    public void ActivateGlow(CCSPlayerController controller, int TeamID)
    {
        try{
            if(controller.PlayerPawn.Value!.TeamNum == teamGlow && controller.IsValid && controller.PawnIsAlive)
            {
                var _modelRelay = SetGlowing(controller.PlayerPawn.Value, TeamID);
                if(playerGlows.ContainsKey(controller.UserId)) 
                {playerGlows.Remove(controller.UserId);}
                else playerGlows.Add(controller.UserId, _modelRelay);
            }   
            }
            catch(Exception e)
            {
                Logger.LogError("Issue during ActivateGlow : {exceptionmessage}", e.ToString());
            }

    }

    public void deactivateGlow(int playerId)
    {
        try
                {
                var player = Utilities.GetPlayerFromUserid(playerId)!;
                if(player!.IsValid)
                {
                    playerGlows[playerId]!.AcceptInput("kill");
                    playerGlows.Remove(playerId);
                    
                }
                }
                catch (Exception e)
                {
                   Logger.LogError("Issue when disabling glows : {exceptionmessage} ", e.ToString());
                    playerGlows.Remove(playerId);
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
       
        
        
        //Console.Write(modelName);
        Server.NextFrame(() => {
            try{
            
            modelRelay.SetModel(modelName);
           
            
            modelRelay.Entity!.Name = "model";
            modelRelay.Spawnflags = 256u;
            modelRelay.RenderMode = RenderMode_t.kRenderNone;
            modelRelay.DispatchSpawn();
            
            

            modelGlow.SetModel(modelName);
            modelGlow.Spawnflags = 256u;
            modelGlow.Glow.Glowing = true;
            //modelGlow.Glow.Flashing =true;    
            modelGlow.DispatchSpawn();
            modelGlow.Entity!.Name = "glow";
    
            modelGlow.Glow.GlowColorOverride = glowColor;
            //modelGlow.Glow.GlowColorOverride = Color.Red;
            modelGlow.Glow.GlowRange = 5000;
            modelGlow.Glow.GlowTeam = team;
            modelGlow.Glow.GlowType = 3;
            modelGlow.Glow.GlowRangeMin = 15;

            modelRelay.AcceptInput("FollowEntity", pawn, modelRelay, "!activator");
            modelGlow.AcceptInput("FollowEntity", modelRelay, modelGlow, "!activator");
            
             }
            catch (Exception e)
            {
            Logger.LogError("Error during glow setup {exceptionmessage}: ", e.ToString());
            return;
            }
        });
        
        return modelRelay;
        
        
    }


    public static CCSGameRules GetGameRules()
    {
        return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
    }

    
}
