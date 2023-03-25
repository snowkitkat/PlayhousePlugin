using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using InventorySystem.Items.Usables.Scp330;
using MEC;

namespace PlayhousePlugin.CustomClass.Abilities
{
    public class TonicShot : CooldownAbilityBase
    {
        public override string Name { get; } = "Tonic Shot";
        public override Player Ply { get; }
        public override double Cooldown { get; set; } = 40;
        private int multiplier = 1;
        private int TimeElapsed = 0;
        private bool Enabled = false;
        public CoroutineHandle _coroutineHandle;
        public TonicShot(Player ply)
        {
            Ply = ply;
        }
        public override string GenerateHud()
        {
            if(!Enabled)
                return base.GenerateHud();
	        
            return $"Selected: {Name} ({20-TimeElapsed} seconds of Tonic remains)";
        }
        public override bool UseCooldownAbility()
        {
            Ply.EnableEffect<MovementBoost>();
            Ply.ChangeEffectIntensity<MovementBoost>(2);
            Ply.MaxHealth = 125;
            Ply.Health = 100;
            Scp330Bag.AddSimpleRegeneration(Ply.ReferenceHub, 2, 20);
            Ply.EnableEffect(EffectType.MovementBoost, 20);
            Ply.EnableEffect(EffectType.Scp1853, 20);
            Ply.GetEffect(EffectType.MovementBoost).Intensity = 50;
            Enabled = true;
            return true;
        }
    }
}