using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;

namespace CS2_ExecAfter
{
	public partial class CS2_ExecAfter
	{
		public string? exec_after_map_start;
		public string? exec_after_map_start_once;
		
		private DateTime _lastMapStartExecution = DateTime.MinValue;
		private float _mapStartCooldownSeconds = 5.0f;
		private string _lastMapName = string.Empty;

		private void OnMapStartHandler(string mapName)
		{
			DateTime now = DateTime.Now;
			TimeSpan timeSinceLastExecution = now - _lastMapStartExecution;
			
			if (_lastMapName == mapName && timeSinceLastExecution.TotalSeconds < _mapStartCooldownSeconds)
			{
				Log($"Ignoring duplicate MapStart event for {mapName} (within {_mapStartCooldownSeconds}s cooldown)");
				return;
			}
			
			_lastMapStartExecution = now;
			_lastMapName = mapName;
			
			Server.NextFrame(() =>
			{
				if (!string.IsNullOrEmpty(exec_after_map_start))
				{
					ReplyToCommand($"Executing (map start): {exec_after_map_start}");
					Server.ExecuteCommand(exec_after_map_start);
				}
				if (!string.IsNullOrEmpty(exec_after_map_start_once))
				{
					ReplyToCommand($"Executing once (map start): {exec_after_map_start_once}");
					Server.ExecuteCommand(exec_after_map_start_once);
					exec_after_map_start_once = null;
				}
			});
		}

		[ConsoleCommand("exec_after_map_start", "Executes a command after every map start")]
		[RequiresPermissions("@css/rcon")]
		public void ConVarExecAfterMapStart(CCSPlayerController? player, CommandInfo command)
		{
			string args = command.ArgString.Trim();
			if (string.IsNullOrEmpty(args))
			{
				ReplyToCommand($"exec_after_map_start = {exec_after_map_start}", player);
				return;
			}
			args = StripQuotes(args);
			exec_after_map_start = args;
			ReplyToCommand($"exec_after_map_start = {exec_after_map_start}", player);
		}

		[ConsoleCommand("exec_after_map_start_once", "Executes a command after the next map start")]
		[RequiresPermissions("@css/rcon")]
		public void ConVarExecAfterMapStartOnce(CCSPlayerController? player, CommandInfo command)
		{
			string args = command.ArgString.Trim();
			if (string.IsNullOrEmpty(args))
			{
				ReplyToCommand($"exec_after_map_start_once = {exec_after_map_start_once}", player);
				return;
			}
			args = StripQuotes(args);
			exec_after_map_start_once = args;
			ReplyToCommand($"exec_after_map_start_once = {exec_after_map_start_once}", player);
		}
		
		[ConsoleCommand("exec_after_map_start_cooldown", "Sets the cooldown in seconds to prevent duplicate executions (default: 5.0)")]
		[RequiresPermissions("@css/rcon")]
		public void ConVarExecAfterMapStartCooldown(CCSPlayerController? player, CommandInfo command)
		{
			string args = command.ArgString.Trim();
			if (string.IsNullOrEmpty(args))
			{
				ReplyToCommand($"exec_after_map_start_cooldown = {_mapStartCooldownSeconds}", player);
				return;
			}
			
			if (float.TryParse(StripQuotes(args), out float cooldown) && cooldown >= 0)
			{
				_mapStartCooldownSeconds = cooldown;
				ReplyToCommand($"exec_after_map_start_cooldown = {_mapStartCooldownSeconds}", player);
			}
			else
			{
				ReplyToCommand("Invalid value. Please specify a non-negative number.", player);
			}
		}
	}
}