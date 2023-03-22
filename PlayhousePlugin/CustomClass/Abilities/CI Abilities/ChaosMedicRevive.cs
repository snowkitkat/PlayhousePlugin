using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;

namespace PlayhousePlugin.CustomClass.Abilities
{
    public class ChaosMedicRevive : CooldownAbilityBase
    {
        public override string Name { get; } = "Revive";
        public override Player Ply { get; }
        public override double Cooldown { get; set; } = 30;

        public ChaosMedicRevive(Player ply)
        {
            Ply = ply;
        }
        
        public override bool UseCooldownAbility()
        {
            if (Ply.CurrentRoom.Type == RoomType.Pocket)
			{
				Ply.ShowCenterDownHint($"<color=yellow>You cannot revive in the Pocket Dimension</color>",3);
				return false;
			}

			List<Collider> colliders = Physics.OverlapSphere(Ply.Position, 3f, LayerMask.GetMask("Ragdoll")).Where(e => e.gameObject.GetComponentInParent<Ragdoll>() != null).ToList();

			colliders.Sort((x, y) => Vector3.Distance(x.gameObject.transform.position, Ply.Position).CompareTo(Vector3.Distance(y.gameObject.transform.position, Ply.Position)));

			if (colliders.Count == 0)
			{
				Ply.ShowCenterDownHint($"<color=yellow>There are no bodies nearby</color>",3);
				return false;
			}

			Ragdoll doll = colliders[0].gameObject.GetComponentInParent<Ragdoll>();

			Player patient = Player.Get(doll.NetworkInfo.OwnerHub);
			if (patient != null)
			{
				if (patient.IsAlive)
				{
					Ply.ShowCenterDownHint($"<color=yellow>This person is already alive</color>",3);
					return false;
				}

				patient.Role.Set(RoleTypeId.ChaosConscript);
				Vector3 pos = Ply.Position;
				Timing.CallDelayed(0.75f, () =>
				{
					patient.EnableEffect<MovementBoost>(10);
					patient.ChangeEffectIntensity<MovementBoost>(15);
					patient.ReferenceHub.playerStats.GetModule<AhpStat>().ServerAddProcess(70).DecayRate = 1f;
					patient.Position = pos + Vector3.up*1.6f;
					patient.Broadcast(7, $"<size=60><b><i>You have been revived by an <color=army_green>Chaos Medic</color>!</i></b></size>");
					
					Ply.ShowCenterDownHint($"<color=yellow>Revived Patient!</color>",3);
				});

				NetworkServer.Destroy(doll.GameObject);
				return true;
			}
			else
			{
				Ply.ShowCenterDownHint($"<color=yellow>This is an unrevivable body</color>",3);
				NetworkServer.Destroy(doll.GameObject);
				return false;
			}
        }
    }
}