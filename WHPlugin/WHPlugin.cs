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
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

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

    public int maxrounds;

    public int overtimeRounds;

    public CCSGameRules? gamerules;

    public Color glowColor = Color.FromArgb(255, 120, 0, 255);
    public Dictionary<int?, CBaseModelEntity?> playerGlows = [];

     public override void Load(bool hotReload)
    {
        
        RegisterEventHandler<EventPlayerSpawn>((@event, info)=>
        {
            
            
            if(wh == true && roundStarted == true)
            {
                if(@event.Userid.PlayerPawn.Value.TeamNum == teamGlow && @event.Userid.PlayerPawn.IsValid)
                   {
                    try
                   {
                    if(playerGlows.ContainsKey(@event.Userid.UserId) == true) 
                    {
                        if(playerGlows[@event.Userid.UserId].IsValid)playerGlows[@event.Userid.UserId].AcceptInput("kill");
                        playerGlows.Remove(@event.Userid.UserId);
                    }
                   }
                   catch(Exception e)
                   {
                    playerGlows.Remove(@event.Userid.UserId);
                   }

                    Console.WriteLine("Spawn Live glow "+@event.Userid.Team);
                    var _modelRelay = SetGlowing(@event.Userid.PlayerPawn.Value, teamwh);
                    playerGlows.Add(@event.Userid.UserId, _modelRelay);
                     
                }
            }
            return HookResult.Continue;
        },HookMode.Post);

        RegisterEventHandler<EventCsIntermission>((@event, info)=>
        {
            Console.WriteLine("Match fini");
            roundStarted = false;
            playerGlows.Clear();
            return HookResult.Continue;
         },HookMode.Post);

         RegisterEventHandler<EventSwitchTeam>((@event, info)=>
        {

            Console.WriteLine("Change team ?");
            return HookResult.Continue;
        },HookMode.Post);
        
        
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
                        try{
                        if(controller.PlayerPawn.Value.TeamNum == teamGlow && controller.IsValid && controller.PlayerPawn.IsValid)
                        {
                             if(playerGlows.ContainsKey(controller.UserId)== true) 
                                {
                                    if(playerGlows[controller.UserId].IsValid)playerGlows[controller.UserId].AcceptInput("kill");
                                    else Console.Write("Not valid");
                                    playerGlows.Remove(controller.UserId);                         
                                }
                            Console.WriteLine("Spawn ! glow "+controller.PlayerPawn.Value.Index+" Team : "+ controller.Team);
                            var _modelRelay = SetGlowing(controller.PlayerPawn.Value, teamwh);
                            playerGlows.Add(controller.UserId, _modelRelay);
                           
                        } 
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("Issue");
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
            var maxrounds = ConVar.Find("mp_maxrounds").GetPrimitiveValue<int>();
            var overtimeMaxrounds = ConVar.Find("mp_overtime_maxrounds").GetPrimitiveValue<int>();

            switch(rounds)
            {
                case var _ when (rounds == (maxrounds/2) && switchNextRound == true):
                invertTeams();
                break;
                case var _ when (rounds > maxrounds && (rounds - maxrounds) %(overtimeMaxrounds/2) == 0 && switchNextRound == true):
                invertTeams();
                break;
            }
            
            Console.Write("Played : "+rounds + "MaxRounds : "+maxrounds);

            //playerGlows.Clear();
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
            whOff();
            roundStarted = false;
            playerGlows.Clear();
            return HookResult.Continue;
        },HookMode.Post);

    }

    // Permissions can be added to commands using the `RequiresPermissions` attribute.
    // See the admin documentation for more information on permissions.
     [ConsoleCommand("css_color", "activates wallhack")]
     [CommandHelper(minArgs: 3)]
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
            commandInfo.ReplyToCommand("Plugin on");
            whOn(playerController.PlayerPawn.Value.TeamNum);
            Console.WriteLine("Wallhack on for team " + playerController.Team);

        }
        else
        {
            
            wh = false;
            foreach(int id in playerGlows.Keys)
            {

                try
                {
                    var player = Utilities.GetPlayerFromUserid(id);
                    if(player.IsValid)
                    {
                        if(playerGlows[id].IsValid)playerGlows[id].AcceptInput("kill");
                        playerGlows.Remove(id);
                        
                    }
                }
                catch(Exception e)
                {
                    playerGlows.Remove(id);
                }
                Console.WriteLine("Wallhack off");
            
            }

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
            try{
            if(controller.PlayerPawn.Value.TeamNum == teamGlow && controller.IsValid && controller.PawnIsAlive)
            {
                var _modelRelay = SetGlowing(controller.PlayerPawn.Value, teamID);
                if(playerGlows.ContainsKey(controller.UserId)) 
                    {playerGlows.Remove(controller.UserId);}
                else playerGlows.Add(controller.UserId, _modelRelay);
            }   
            }
            catch(Exception e)
            {Console.WriteLine("Issue modelglow");}
        }

    }

    public void whOff()
    {
        foreach(int id in playerGlows.Keys)
            {
                try
                {
                var player = Utilities.GetPlayerFromUserid(id);
                if(player.IsValid)
                {
                    playerGlows[id].AcceptInput("kill");
                    playerGlows.Remove(id);
                    
                }
                }
                catch (Exception e)
                {
                    playerGlows.Remove(id);
                }
            }
    }

    public void activateGlow()
    {

    }

    public void deactivateGlow()
    {

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
        
            modelRelay.SetModel(modelName);
            modelRelay.Spawnflags = 256u;
            modelRelay.RenderMode = RenderMode_t.kRenderNone;
            modelRelay.DispatchSpawn();
            

            modelGlow.SetModel(modelName);
            modelGlow.Spawnflags = 256u;
            modelGlow.Glow.Glowing = true;
            //modelGlow.Glow.Flashing =true;    
            modelGlow.DispatchSpawn();


            modelGlow.Glow.GlowColorOverride = glowColor;
            //modelGlow.Glow.GlowColorOverride = Color.Red;
            modelGlow.Glow.GlowRange = 5000;
            modelGlow.Glow.GlowTeam = team;
            modelGlow.Glow.GlowType = 3;
            modelGlow.Glow.GlowRangeMin = 15;

            modelRelay.AcceptInput("FollowEntity", pawn, modelRelay, "!activator");
            modelGlow.AcceptInput("FollowEntity", modelRelay, modelGlow, "!activator");
        });
        return modelRelay;
    }


    public static CCSGameRules GetGameRules()
    {
        return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
    }

 
}
