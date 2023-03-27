using System.Collections.Generic;
using System.Linq;
using MEC;
using Exiled.API.Features;
using UnityEngine;
using Mirror;
using Exiled.API.Enums;
using CustomPlayerEffects;
using System.Text;
using System.IO;
using InventorySystem.Items.Pickups;
using System;
using AdminToys;
using Exiled.API.Features.Items;
using Exiled.API.Extensions;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Player;
using Footprinting;
using RemoteAdmin;
using Mirror.LiteNetLib4Mirror;
using InventorySystem;
using MapEditorReborn.API.Features;
using MapEditorReborn.API.Features.Objects;
using PlayerRoles;
using PlayerStatsSystem;
using AmmoPickup = InventorySystem.Items.Firearms.Ammo.AmmoPickup;

namespace PlayhousePlugin
{
	// Contains commonly used methods
	class UtilityMethods
	{
		public static void FindAndSetCustomBadge(Player ply)
		{
			if (ply.GlobalBadge != null) return;
			try
			{
				var t = File.ReadAllText($@"/home/ubuntu/.config/EXILED/Configs/CustomBadgeTexts/badges.txt").Split('\n').ToList().Where(x => x.Contains(ply.RawUserId)).FirstOrDefault();

				if (t != null)
				{
					Timing.CallDelayed(5f, () =>
					{
						ply.RankName = ply.RankName=="" ? $"{t.Substring(ply.RawUserId.Length + 1)}" : $"{t.Substring(ply.RawUserId.Length+1)} ({ply.RankName})";
					});
				}

			}
			catch
			{
				return;
			}
		}

		public static IEnumerator<float> FadeAway(PrimitiveObjectToy toy)
		{
			var time = 0f;
			while (time < 3f)
			{
				time += Time.deltaTime;
				toy.NetworkMaterialColor = new Color(1, 0, 0, 0.7f - time/4.3f);
				yield return Timing.WaitForOneFrame;
			}
			NetworkServer.Destroy(toy.gameObject);
		}
		
		public static SchematicObject SpawnSchematic(string schematicName, Vector3 position)
		{
			var schematicData = MapUtils.GetSchematicDataByName(schematicName);
			return ObjectSpawner.SpawnSchematic(schematicName,
				position, Quaternion.identity, Vector3.one, schematicData);
		}
		
		public static SchematicObject SpawnSchematic(string schematicName, Vector3 position, Quaternion rotation)
		{
			var schematicData = MapUtils.GetSchematicDataByName(schematicName);
			return ObjectSpawner.SpawnSchematic(schematicName,
				position, rotation, Vector3.one, schematicData);
		}
		
		public static SchematicObject SpawnSchematic(string schematicName, Vector3 position, Vector3 scale)
		{
			var schematicData = MapUtils.GetSchematicDataByName(schematicName);
			return ObjectSpawner.SpawnSchematic(schematicName,
				position, Quaternion.identity, scale, schematicData);
		}

		public static SchematicObject SpawnSchematic(string schematicName, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			var schematicData = MapUtils.GetSchematicDataByName(schematicName);
			return ObjectSpawner.SpawnSchematic(schematicName,
				position, rotation, scale, schematicData);
		}
		
		// Spawns an actual grenade that gets spit from the players view
		/*public static Pickup SpawnGrenadeOnPlayer(Player player, ItemType grenadeType, float timer, float velocity = 1f)
		{
			var nade = (ExplosiveGrenade) ExplosiveGrenade.Create(grenadeType, player);
			nade.FuseTime = 99999;
			var pickup = nade.CreatePickup(player.Position);
			nade.Base.ServerThrow(10, 1, Vector3.one, player.CameraTransform.forward*2);
			//nade.SpawnActive(player.Position, player);
			return pickup;
		}*/

		public static void ApplyAmmoRegen(Player p, ushort ammoCount, bool displayHint, Player Ammo)
		{
			List<ItemType> typesToGive = new List<ItemType>();
			foreach(var gun in p.Items.Where(x => x.Type.IsWeapon()))
			{
				Firearm firearm = (Firearm)gun;
				if (!typesToGive.Contains(firearm.AmmoType.GetItemType()))
					typesToGive.Add(firearm.AmmoType.GetItemType());
			}

			ArmourAmmo limits;
			var armour = p.Items.FirstOrDefault(x => x.Type.IsArmor());

			if (armour == null)
				limits = Utils.ArmourAmmoLimits[ItemType.None];
			else
				limits = Utils.ArmourAmmoLimits[armour.Type];

			bool ammoGiven = false;
			
			foreach (var type in typesToGive)
			{
				try
				{
					if (p.Ammo[type] < limits.LimitDictionary[type])
					{
						if (p.Ammo[type] + ammoCount > limits.LimitDictionary[type])
							p.Inventory.ServerSetAmmo(type, limits.LimitDictionary[type]);
						else
							p.Inventory.ServerAddAmmo(type, ammoCount);
						
						ammoGiven = true;
					}
				}
				catch
				{
					p.Inventory.ServerAddAmmo(type, 1);
				}
			}

			if (displayHint && ammoGiven)
			{
				Ammo.ShowHint("<color=red>You are providing ammo!</color>", 1);
				p.ShowHint($"<color=red>Ammo Replenished</color>", 1);

			}
		}

		public static void ApplyPoison(Player p, Player Exterminator)
		{
			if (p.Health - 4f <= 0)
			{
				p.Kill("Military Grade Bio-Weapon");
			}
			else
			{
				if (p.Role.Type != RoleTypeId.Scp079 && p.IsAlive)
				{
					p.Hurt(4f, "Military Grade Bio-Weapon");
					//p.Hurt(7.5f, Exterminator, damageType: DamageTypes.Poison);
					p.ShowHint("<color=yellow>You are being poisoned by a military grade Bio-Weapon</color>");
				}
			}
		}

		// Healing method designed to be used for the Medic which heals HP and overheals AHP
		public static void ApplyMedicHeal(Player p, float h, bool displayHint, Player Medic)
		{
			float HpGiven = 0;
			float AHPGiven = 0;
			if (p.Health + h > p.MaxHealth)
			{
				HpGiven = p.MaxHealth - p.Health;
				p.Health = p.MaxHealth;
			}
			else
			{
				HpGiven = h;
				p.Health += h;
			}
			
			if (p.Health == p.MaxHealth && p != Medic)
			{
				if (p.ArtificialHealth < 20)
				{
					//Give player 5 AHP if their current AHP is less than 20 if we add 5 AHP
					if (p.ArtificialHealth + 5 > 20)
					{
						AHPGiven = 20 - p.ArtificialHealth;
						p.ArtificialHealth = 20;
					}
					else
					{
						AHPGiven = 5;
						p.ArtificialHealth += 5;
					}
				}
			}
			if (displayHint && HpGiven != 0)
			{
				Medic.ShowHint("<color=red>You are healing!</color>", 1);
				p.ShowHint($"<color=red>+HP</color>");
			}
			if (displayHint && AHPGiven > 1)
			{
				p.ShowHint($"<color=red>+AHP</color>");
			}
		}

		// Heals HP of target
		public static void ApplyHeal(Player p, float h, bool displayHint, Player Healer)
		{
			float HpGiven = 0;
			if (p.Health + h > p.MaxHealth)
			{
				HpGiven = p.MaxHealth - p.Health;
				p.Health = p.MaxHealth;
			}
			else
			{
				HpGiven = h;
				p.Health += h;
			}
			
			if (displayHint && HpGiven > 1)
			{
				Healer.ShowHint("<color=red>You are healing!</color>", 1);
				p.ShowHint($"<color=red>+HP</color>");
			}
		}

		// Overheals AHP of target
		public static void ApplyOverheal(Player p, float h, bool displayHint, Player Healer)
		{
			if (p.MaxArtificialHealth == 0 && (p.Role.Type == RoleTypeId.Scp049 || p.Role.Type == RoleTypeId.Scp0492))
			{
				p.MaxArtificialHealth = 100;
			}
			
			float HpGiven = 0;
			if (p.ArtificialHealth + h > p.MaxArtificialHealth)
			{
				HpGiven = p.MaxArtificialHealth - p.ArtificialHealth;
				p.ArtificialHealth = p.MaxArtificialHealth;
			}
			else
			{
				HpGiven = h;
				p.ArtificialHealth += h;
			}
			
			if (displayHint && HpGiven > 1)
			{
				Healer.ShowHint("<color=red>You are overhealing!</color>", 1);
				p.ShowHint($"<color=red>+AHP</color>");
			}
		}

		public static void InfectPlayer(Player Ply)
		{
			if (!PlayhousePlugin.PlayhousePluginRef.Handler.InfectedPlayers.Contains(Ply))
			{
				PlayhousePlugin.PlayhousePluginRef.Handler.InfectedPlayers.Add(Ply);
				Ply.ReferenceHub.playerEffectsController.EnableEffect<Poisoned>();
				Ply.ReferenceHub.playerEffectsController.EnableEffect<Hemorrhage>();
			}
		}

		public static IEnumerator<float> LobbyTimer()
		{
			StringBuilder messageUp = new StringBuilder();
			StringBuilder messageDown = new StringBuilder();
			var text = "<b>discord.gg/kognity</b>\n<color=%rainbow%><b>Welcome To Kognity's Playhouse\nGo stand near the team you want to play as!</b></color>";
			int x = 0;
			string[] colors = { "#f54242", "#f56042", "#f57e42", "#f59c42", "#f5b942", "#f5d742", "#f5f542", "#d7f542", "#b9f542", "#9cf542", "#7ef542", "#60f542", "#42f542", "#42f560", "#42f57b", "#42f599", "#42f5b6", "#42f5d4", "#42f5f2", "#42ddf5", "#42bcf5", "#429ef5", "#4281f5", "#4263f5", "#4245f5", "#5a42f5", "#7842f5", "#9642f5", "#b342f5", "#d142f5", "#ef42f5", "#f542dd", "#f542c2", "#f542aa", "#f5428d", "#f5426f", "#f54251" };
			while (!Round.IsStarted)
			{
				messageUp.Clear();
				messageDown.Clear();

				messageUp.Append("<size=40><color=yellow><b>The game will be starting soon, %seconds</b></color></size>");

				short NetworkTimer = GameCore.RoundStart.singleton.NetworkTimer;

				switch (NetworkTimer)
				{
					case -2: messageUp.Replace("%seconds", "Server is paused"); break;

					case -1: messageUp.Replace("%seconds", "Round is being started"); break;

					case 1: messageUp.Replace("%seconds", $"{NetworkTimer} second remain"); break;

					case 0: messageUp.Replace("%seconds", "Round is being started"); break;

					default: messageUp.Replace("%seconds", $"{NetworkTimer} seconds remains"); break;
				}

				messageUp.Append($"\n<size=30><i>%players</i></size>");

				if (Player.List.Count() == 1) messageUp.Replace("%players", $"{Player.List.Count()} player has connected");
				else messageUp.Replace("%players", $"{Player.List.Count()} players have connected");

				messageDown.Append(text.Replace("%rainbow%", colors[x++ % colors.Length]));

				foreach (Player ply in Player.List)
				{
					ply.ShowHint(messageUp.ToString());
					ply.ShowHint(messageDown.ToString());
					//ply.ShowCenterHint(message.ToString(), 1);
					//ply.Broadcast(1, message.ToString());
				}
				x++;
				yield return Timing.WaitForSeconds(1f);
			}
		}

		// FIX THIS!!!!!!!!!!!!!!!!!!!!!!!!! - snow :D
		/*public static void SpawnRagdoll(Vector3 pos, Quaternion rot, RoleTypeId roleType, string deathCause, Player owner = null)
		{
			ReferenceHub target = owner?.ReferenceHub ?? ReferenceHub.HostHub;
			Exiled.API.Features.Ragdoll.Spawn(new RagdollData(target, new CustomReasonDamageHandler(deathCause), pos, rot));
		}*/
		public static IEnumerator<float> CleanupItems()
		{
			// Cleans all the items
			foreach (ItemPickupBase item in UnityEngine.Object.FindObjectsOfType<ItemPickupBase>())
				item.DestroySelf();
			return null;
		}

		/// <summary>
		/// Changes their PlayerInfo based on params
		/// </summary>
		/// <param name="Ply">Player object</param>
		/// <param name="text">Text to show</param>
		/// <param name="size">Size of badge default 25</param>
		/// <param name="colour">Color used</param>
		public static void GiveCustomPlayerInfo(Player Ply, string text, int size, string colour)
		{
			switch (colour)
			{
				case "mint":
					colour = "98FB98";
					break;

				case "army_green":
					colour = "4B5320";
					break;

				case "yellow":
					colour = "FAFF86";
					break;
			}
			switch (size)
			{
				case -1:
					size = 25;
					break;
			}
			Ply.ReferenceHub.nicknameSync.Network_customPlayerInfoString = $"<size=\"{size}\"><color=#{colour}>{text}</color></size>";
		}

		/// <summary>
		/// Jails a player
		/// </summary>
		/// <param name="player"></param>
		/// <param name="skipadd"></param>
		/// <returns></returns>
		public static IEnumerator<float> DoJail(Player player, bool skipadd = false)
		{
			List<Item> items = new List<Item>();
			Dictionary<AmmoType, ushort> ammo = new Dictionary<AmmoType, ushort>();
			foreach (KeyValuePair<ItemType, ushort> kvp in player.Ammo)

				ammo.Add(kvp.Key.GetAmmoType(), kvp.Value);
			foreach (Item item in player.Items)
				items.Add(item);
			if (!skipadd)
			{
				EventHandler.JailedPlayers.Add(new Jailed
				{
					Health = player.Health,
					Position = player.Position,
					Items = items,
					Name = player.Nickname,
					Role = player.Role.Type,
					Userid = player.UserId,
					CurrentRound = true,
					Ammo = ammo,
					SCP207Intensity = player.ReferenceHub.playerEffectsController.GetEffect<Scp207>()._intensity
				});
			}

			if (player.IsOverwatchEnabled)
				player.IsOverwatchEnabled = false;

			yield return Timing.WaitForSeconds(1f);
			player.ClearInventory(false);
			player.Role.Set(RoleTypeId.Tutorial);
			player.Position = new Vector3(53f, 1020f, -44f);
		}


		/// <summary>
		/// Unjails a player
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static IEnumerator<float> DoUnJail(Player player)
		{
			Jailed jail = EventHandler.JailedPlayers.Find(j => j.Userid == player.UserId);
			if (jail.CurrentRound)
			{
				player.Role.Set(jail.Role);
				yield return Timing.WaitForSeconds(0.5f);
				player.ResetInventory(jail.Items);
				player.Health = jail.Health;
				player.Position = jail.Position;
				foreach (KeyValuePair<AmmoType, ushort> kvp in jail.Ammo)
					player.Ammo[kvp.Key.GetItemType()] = kvp.Value;

				player.ReferenceHub.playerEffectsController.ChangeState<Scp207>(jail.SCP207Intensity);
			}
			else
			{
				player.Role.Set(RoleTypeId.Spectator);
			}
			EventHandler.JailedPlayers.Remove(jail);
		}

		/// <summary>
		/// Adds the rainbow controller for a given Player object
		/// </summary>
		/// <param name="Ply"></param>
		public static void AddRainbowController(Player Ply)
		{
			if (Ply.ReferenceHub.TryGetComponent(out PlayhousePlugin RainbowTagCtrl))
				return;

			Ply.GameObject.AddComponent<RainbowTagController>();
		}


		/// <summary>
		/// Finds the preference of a certain player
		/// </summary>
		/// <param name="Ply"></param>
		/// <returns></returns>

		/// <summary>
		/// Finds the preference of a certain player
		/// </summary>
		/// <param name="Ply"></param>
		/// <returns></returns>
		public static string FindPreferenceRaw(Player Ply)
		{
			try
			{
				return File.ReadAllText($@"/home/ubuntu/.config/EXILED/Configs/Pets/PetPreference/{Ply.UserId}");
			}
			catch
			{
				return "";
			}

		}

		public static int GetDonatorNum(string itemNum)
		{
			return int.Parse($"{itemNum[0]}");
		}

		/// <summary>
		/// Updates the Pet Preference for a certain player
		/// </summary>
		/// <param name="Ply"></param>
		/// <param name="itemNum"></param>
		public static void UpdatePreference(Player Ply, string itemNum)
		{
			System.IO.File.WriteAllText($@"/home/ubuntu/.config/EXILED/Configs/Pets/PetPreference/{Ply.UserId}", itemNum);
		}

		/// <summary>
		/// Quite simply it checks if they already have a hat that is following them and kill the coroutine responsible for it.
		/// </summary>
		/// <param name="ev"></param>
		public static void CheckExistingSpawnedHatAndKill(string UserId)
		{
			Hat.KillHat(Player.Get(UserId));
		}
		
		public static void Explode(Player player)
		{
			var grenade = (ExplosiveGrenade) ExplosiveGrenade.Create(ItemType.GrenadeHE, player);
			grenade.PinPullTime = 0.1f;
			grenade.FuseTime = 0.2f;
			grenade.SpawnActive(player.Position, player);
			player.Hurt(new ExplosionDamageHandler(new Footprint(player.ReferenceHub), Vector3.one*10, float.MaxValue, 100));
		}
		
		/// <summary>
		/// RNG generates a random number with the one inputted being the max. 1 = 100%, 2 = 50%, 3 = 33%, etc.
		/// </summary>
		/// <param name="chance"></param>
		/// <returns>True if it generates the same, otherwise false.</returns>
		public static bool RandomChance(int chance)
		{
			return chance == EventHandler.random.Next(chance)+1;
		}

		/// <summary>
		/// Spawns a fake grenade for cosmetic effects
		/// </summary>
		/// <param name="player"></param>
		public static void FakeExplode(Player player)
		{
			var grenade = (ExplosiveGrenade) ExplosiveGrenade.Create(ItemType.GrenadeHE, player);

			grenade.PinPullTime = 0.1f;
			grenade.FuseTime = 0.2f;
			grenade.MaxRadius = 0f;
			
			EventHandler.GrenadesToFake.Add(grenade.Base.Projectile);
			grenade.SpawnActive(player.Position);
		}

		/// <summary>
		/// Spawns a fake grenade or flashbang for cosmetic effects
		/// </summary>
		/// <param name="player"></param>

		public static void RewardPlayers()
		{
			if (EventHandler.RewardPlayers)
			{
				foreach (Player Ply in Player.List)
				{
					string myfile = $"/home/ubuntu/klpBot/RoundClaimsIDs/{Ply.UserId}";

					// Appending the given texts 
					using (StreamWriter sw = File.AppendText(myfile))
					{
						sw.WriteLine("roundPlayed");
					}
				}
			}
		}
	}
}
