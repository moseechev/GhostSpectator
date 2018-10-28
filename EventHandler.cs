using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Smod.GhostSpectatorPlugin
{
	class EventHandler : IEventHandlerSetRole, IEventHandlerTeamRespawn, IEventHandlerRoundStart, IEventHandlerPlayerHurt, IEventHandlerShoot, IEventHandlerFixedUpdate, IEventHandlerPlayerDropItem
	{
		private Plugin plugin;
		public static int maxRespawnAmount;
		public static bool allowGhostToDamage;

		public class ShowGhost
		{
			public int playerId;

			public float remainingTime;
		}

		public List<ShowGhost> ghostList = new List<ShowGhost>();

		public EventHandler(Plugin plugin)
		{
			this.plugin = plugin;
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			maxRespawnAmount = ConfigManager.Manager.Config.GetIntValue("maximum_MTF_respawn_amount", 15);
			allowGhostToDamage = ConfigManager.Manager.Config.GetBoolValue("gs_allow_ghost_to_dmg", false);
		}

		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			if (ev.Role == Role.SPECTATOR)
			{
				ev.Role = Role.TUTORIAL;
				ghostList.Add(new ShowGhost { playerId = ev.Player.PlayerId, remainingTime = 3f });
			}
			else if (ev.Role == Role.TUTORIAL)
			{
				ev.Player.SetGhostMode(true);
				ev.Player.SetGodmode(true);
				ev.Player.GiveItem(ItemType.RADIO);
				ev.Player.SetRadioBattery(999);
			}
			else
			{
				if (ev.Player.GetGhostMode())
					ev.Player.SetGhostMode(false);
				if (ev.Player.GetGodmode())
					ev.Player.SetGodmode(false);
			}
		}

		public void OnTeamRespawn(TeamRespawnEvent ev)
		{
			List<Player> list = (from item in plugin.pluginManager.Server.GetPlayers()
								 where (item.TeamRole.Role == Role.TUTORIAL || item.TeamRole.Role == Role.SPECTATOR) && !item.OverwatchMode
								 select item).ToList();

			Random random = new Random();
			while (list.Count > maxRespawnAmount)
			{
				list.RemoveAt(random.Next(0, list.Count));
			}

			ev.PlayerList = list;
		}

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (ev.Attacker.TeamRole.Role == Role.TUTORIAL)
			{
				if (!allowGhostToDamage)
				{
					ev.Damage = 0f;
				}
			}
		}

		public void OnShoot(PlayerShootEvent ev)
		{
			if (ev.Player.TeamRole.Team == Team.TUTORIAL)
			{
				foreach (ShowGhost ghost in ghostList)
				{
					if (ghost.playerId == ev.Player.PlayerId)
					{
						ghost.remainingTime = 3f;
						ev.Player.SetGhostMode(false);
						return;
					}
				}

				ghostList.Add(new ShowGhost { playerId = ev.Player.PlayerId, remainingTime = 3f });
				ev.Player.SetGhostMode(false);
			}
		}

		public void OnFixedUpdate(FixedUpdateEvent ev)
		{
			for (int i = 0; i < ghostList.Count; i++)
			{
				ghostList[i].remainingTime -= 0.02f;
				if (ghostList[i].remainingTime <= 0)
				{
					foreach (Player player in plugin.pluginManager.Server.GetPlayers())
					{
						if (player.PlayerId == ghostList[i].playerId && player.TeamRole.Team == Team.TUTORIAL)
						{
							player.SetGhostMode(true);
							player.SetGodmode(true);
							player.SetRadioBattery(999);
							bool hasRadio = false, hasCoin = false;
							foreach(Item item in player.GetInventory())
							{
								if (item.ItemType == ItemType.RADIO)
								{
									hasRadio = true;
								}
								if (item.ItemType == ItemType.COIN)
								{
									hasCoin = true;
								}
							}
							if (!hasRadio)
								player.GiveItem(ItemType.RADIO);
							if (!hasCoin)
								player.GiveItem(ItemType.COIN);
						}
					}
					ghostList.RemoveAt(i);
					return;
				}
			}
		}

		public void OnPlayerDropItem(PlayerDropItemEvent ev)
		{
			if (ev.Player.TeamRole.Role == Role.TUTORIAL && ev.Item.ItemType == ItemType.COIN)
			{
				List<Player> list = (from item in plugin.pluginManager.Server.GetPlayers()
									 where (item.PlayerId != ev.Player.PlayerId && item.TeamRole.Role != Role.TUTORIAL && item.TeamRole.Role != Role.SPECTATOR) && !item.OverwatchMode
									 select item).ToList();

				if (list.Count > 0)
				{
					Random random = new Random();
					ev.Player.Teleport(list[random.Next(0, list.Count)].GetPosition());
				}

				ev.ChangeTo = ItemType.NULL;
				ev.Player.GiveItem(ItemType.COIN);
			}
		}
	}
}
