using ImGuiNET;

using REFrameworkNET;
using REFrameworkNET.Attributes;

// Don't delete, it is actually in use
using System;
// Don't delete, it is actually in use
using System.Collections.Generic;

using app;

namespace MHWs_AFK_Farmer;

// Cool plugin, very wow
public class MhWsAfkFarmer
{
	private static bool _isSmallMonsterFarmingOn = false;
	private static bool _isLargeMonsterFarmingOn = false;

	private static readonly List<EnemyCharacter> _smallMonsters = [];
	private static readonly List<EnemyCharacter> _largeMonsters = [];

	[PluginEntryPoint]
	public static void Main()
	{
		REFrameworkNET.Callbacks.ImGuiDrawUI.Post += OnImGuiDrawUI;
	}

	private static void OnImGuiDrawUI()
	{
		if(ImGui.TreeNode(label: "AFK Farmer##afk-farmer"))
		{
			if(ImGui.Button(label: "Small Monsters##afk-farmer"))
			{
				_isSmallMonsterFarmingOn = !_isSmallMonsterFarmingOn;

				if(!_isSmallMonsterFarmingOn)
				{
					_smallMonsters.Clear();
				}
			}

			ImGui.SameLine();
			ImGui.Text(_isSmallMonsterFarmingOn ? "On" : "Off");

			if(ImGui.Button(label: "Large Monsters##afk-farmer"))
			{
				_isLargeMonsterFarmingOn = !_isLargeMonsterFarmingOn;

				if(!_isLargeMonsterFarmingOn)
				{
					_largeMonsters.Clear();
				}
			}

			ImGui.SameLine();
			ImGui.Text(_isLargeMonsterFarmingOn ? "On" : "Off");

			ImGui.TreePop();
		}
	}

	[MethodHook(typeof(app.EnemyCharacter), nameof(app.EnemyCharacter.doUpdateEnd), MethodHookType.Pre)]
	public static PreHookResult OnPreDoUpdateEnd(Span<ulong> args)
	{
		try
		{
			var enemyCharacterPtr           = args[1];
			var enemyCharacterManagedObject = ManagedObject.ToManagedObject(enemyCharacterPtr);
			if(enemyCharacterManagedObject is null)
			{
				return PreHookResult.Continue;
			}

			var enemyCharacter = enemyCharacterManagedObject.As<app.EnemyCharacter>();

			var context = enemyCharacter._Context;
			if(context is null)
			{
				return PreHookResult.Continue;
			}

			var enemyContext = context._Em;
			if(enemyContext is null)
			{
				return PreHookResult.Continue;
			}

			var isSmallMonster = enemyContext.IsZako;
			if(isSmallMonster)
			{
				var isFound = _smallMonsters.Contains(enemyCharacter);
				if(isFound)
				{
					return PreHookResult.Continue;
				}

				KillSmallMonster(enemyCharacter);
				return PreHookResult.Continue;
			}

			var isLargeMonster = enemyContext.IsBoss;
			if(isLargeMonster)
			{
				var isFound = _largeMonsters.Contains(enemyCharacter);
				if(isFound)
				{
					return PreHookResult.Continue;
				}

				KillLargeMonster(enemyCharacter);
				return PreHookResult.Continue;
			}

			return PreHookResult.Continue;
		}
		catch(Exception exception)
		{
			return PreHookResult.Continue;
		}
	}

	[MethodHook(typeof(app.EnemyCharacter), nameof(app.EnemyCharacter.doOnDestroy), MethodHookType.Pre)]
	public static PreHookResult OnPreDoOnDestroy(Span<ulong> args)
	{
		try
		{
			var enemyCharacterPtr = args[1];
			var enemyCharacter    = ManagedObject.ToManagedObject(enemyCharacterPtr).As<app.EnemyCharacter>();

			var context = enemyCharacter._Context;
			if(context is null)
			{
				return PreHookResult.Continue;
			}

			var enemyContext = context._Em;
			if(enemyContext is null)
			{
				return PreHookResult.Continue;
			}

			var isLargeMonster = enemyContext.IsBoss;
			if(isLargeMonster)
			{
				var isFound = _largeMonsters.Contains(enemyCharacter);
				if(isFound)
				{
					_largeMonsters.Remove(enemyCharacter);
				}

				return PreHookResult.Continue;
			}

			var isSmallMonster = enemyContext.IsZako;
			if(isSmallMonster)
			{
				var isFound = _smallMonsters.Contains(enemyCharacter);
				if(isFound)
				{
					_smallMonsters.Remove(enemyCharacter);
				}

				return PreHookResult.Continue;
			}

			return PreHookResult.Continue;
		}
		catch(Exception exception)
		{
			return PreHookResult.Continue;
		}
	}

	public static void KillMonster(app.EnemyCharacter enemyCharacter)
	{
		enemyCharacter._Context.Chara.HealthManager.Health = 1f;
		enemyCharacter.requestForceDieDamage(null, GUI020020.State.NORMAL, null);
	}

	public static void KillSmallMonster(EnemyCharacter enemyCharacter)
	{
		if(!_isSmallMonsterFarmingOn) return;

		_smallMonsters.Add(enemyCharacter);
		KillMonster(enemyCharacter);
	}

	public static void KillLargeMonster(app.EnemyCharacter enemyCharacter)
	{
		if(!_isLargeMonsterFarmingOn) return;

		_largeMonsters.Add(enemyCharacter);
		KillMonster(enemyCharacter);
	}
}