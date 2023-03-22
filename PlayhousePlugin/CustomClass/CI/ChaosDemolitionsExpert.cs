using System.Collections.Generic;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace PlayhousePlugin.CustomClass
{
    public class ChaosDemolitionsExpert : CustomClassBase
    {
        public override Player Ply { get; set; }
        public override string Name { get; } = "Chaos Demolitions Expert";
        public override int AbilitiesNum { get; } = 0;
        public override CoroutineHandle[] PassiveAbilities { get; }
        public override AbilityBase[] ActiveAbilities { get; }

        public override void Escape()
        {
            Ply.Role.Set(RoleTypeId.NtfPrivate);
            Ply.CustomClassManager().DisposeCustomClass();
            Ply.CustomClassManager().CustomClass = new NTFDemolitionsExpert(Ply);
        }

        public override void Replace(Player ply)
        {
            Dispose();
            ply.Role.Set(Ply.Role.Type);
            Vector3 pos = ply.Position;
            Timing.CallDelayed(0.1f, () =>
            {
                ply.Position = pos;
                ply.CustomClassManager().CustomClass = new ChaosDemolitionsExpert(ply);
            });
        }
        
        public ChaosDemolitionsExpert(Player ply)
        {
            Ply = ply;
            ActiveAbilities = new AbilityBase[] { };
            PassiveAbilities = new[]
            {
                Timing.RunCoroutine(RegenExplosive())
            };

            ply.ClearInventory();
            ply.AddItem(ItemType.GunAK);
            ply.AddItem(ItemType.KeycardChaosInsurgency);
            ply.AddItem(ItemType.Medkit);
            ply.AddItem(ItemType.Medkit);
            ply.AddItem(ItemType.ArmorCombat);
            ply.AddItem(ItemType.GrenadeHE);
            ply.AddItem(ItemType.GrenadeHE);
            ply.AddItem(ItemType.GrenadeHE);
            
            ply.Ammo[ItemType.Ammo762x39] = 120;
            
            Timing.CallDelayed(1.8f, () =>
            {
                ply.Health = 150;
                ply.MaxHealth = 150;
            });

            if (EventHandler.SillySunday)
            {
                UtilityMethods.GiveCustomPlayerInfo(ply, $"{SillySundayNames.ChaosDemolitionsExpertSundayNames.PickRandom()}\n(Custom Class)", -1, "army_green");
            }
            else
            {
                UtilityMethods.GiveCustomPlayerInfo(ply, "Demolitions Expert\n(Custom Class)", -1, "army_green");
            }
            
            ply.Broadcast(10, "<size=40><b><i>You have spawned as a <color=green>Demolitions Expert</color>\nThis is a custom class with special abilities. To learn how to use them, press your console key (~ by default) or join our Discord at discord.gg/kognity</i></b></size>");
            ply.SendConsoleMessage("Name: Chaos Demolitions Expert\n\nDescription: You wear thicker kevlar so you can take a beating and you have explosives at your disposal. Go cause some Chaos.\n\nPassive Buffs: 150 Max HP, Explosive Item Regen every minute\nPassive Debuffs: None\n\nTo get the binding commands for the abilities type \".commands\"", "yellow");
        }
        
        private IEnumerator<float> RegenExplosive()
        {
            var specialItems = new List<ItemType> { ItemType.GrenadeFlash, ItemType.SCP018 , ItemType.SCP2176};

            while (true)
            {
                yield return Timing.WaitForSeconds(60f);

                if (Ply.CustomClassManager().CustomClass.Name != "Chaos Demolitions Expert")
                {
                    break;
                }

                if (Ply.IsCuffed) continue;
                if (Ply.Inventory.UserInventory.Items.Count != 8)
                {
                    var chance = EventHandler.random.Next(0, 100);

                    if (chance <= 20)
                    {
                        Ply.AddItem(specialItems[EventHandler.random.Next(specialItems.Count)]);
                        Ply.ShowCenterDownHint($"<color=yellow>Special Item Generated!</color>", 3);
                    }
                    else
                    {
                        Ply.AddItem(ItemType.GrenadeHE);
                        Ply.ShowCenterDownHint($"<color=yellow>Explosive Item Generated!</color>", 3);
                    }
                }
                else
                {
                    Ply.ShowCenterDownHint($"<color=yellow>You would have regenerated an item here, but your inventory is full.</color>", 4);
                }
            }
        }
    }
}