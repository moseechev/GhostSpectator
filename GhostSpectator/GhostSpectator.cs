using Smod.GhostSpectatorPlugin;
using Smod2;
using Smod2.Attributes;


namespace GhostSpectator
{
	[PluginDetails(
		author = "ShingekiNoRex",
		name = "GhostSpectator",
		description = "",
		id = "rex.ghost.spectator",
		version = "1.0",
		SmodMajor = 3,
		SmodMinor = 1,
		SmodRevision = 20
		)]
	class GhostSpectator : Plugin
	{
		public override void OnDisable()
		{
		}

		public override void OnEnable()
		{
			this.Info("GhostSpectator has loaded :)");
		}

		public override void Register()
		{
			// Register Events
			this.AddEventHandlers(new EventHandler(this), Smod2.Events.Priority.Low);
		}
	}
}
