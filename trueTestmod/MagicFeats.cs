// Copyright (c) 2019 Jennifer Messerly
// This code is licensed under MIT license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;

using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Class.LevelUp;

using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Controllers.Projectiles;
using Harmony12;
using Newtonsoft.Json;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace trueTestmod
{
    static class MagicFeats
    {
        static LibraryScriptableObject library => Main.library;



        internal static void Load()
        {
            // Load metamagic feats
            
            //var feats = Array.Empty<BlueprintFeature>();
            Main.SafeLoad(LoadRayShield, "RayShield Feat");
            Main.SafeLoad(LoadWhirlwind, "Whirlwind Attack");
            Main.SafeLoad(LoadLunge, "Lunge");
            Main.SafeLoad(LoadBodyGuard, "BodyGuard");
            Main.SafeLoad(LoadInHarmsWay, "In Harms Way"); 
            
            // Add all feats (including metamagic, wizard discoveries) to general feats.
            //library.AddFeats(feats.ToArray());


           
        }



        static void LoadWhirlwind()
        {
            
            var CombExpertise = library.Get<BlueprintFeature>("4c44724ffa8844f4d9bedb5bb27d144a");
            var Dodge = library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5");
            var Mobility = library.Get<BlueprintFeature>("2a6091b97ad940943b46262600eaeaeb");
            var KineticWhirl = library.Get<BlueprintFeature>("80fdf049d396c33408a805d9e21a42e1");
            var WhirlwindAttackAbility = Helpers.CreateAbility("Whirlwindattack", "Whirlwind Attack",
                "When you use the full-attack action, you can give up your regular attacks and instead make one melee attack at your highest base attack bonus against each opponent within 10 feets. You must make a separate attack roll against each opponent.\nWhen you use the Whirlwind Attack feat, you also forfeit any bonus or extra attacks granted by other feats, spells, or abilities",
                "aea44da3f5204a218840c80ef6a24bec",
                KineticWhirl.Icon,
                AbilityType.Special,
                CommandType.Standard,
                AbilityRange.Personal,
                "",
                "",
                Helpers.CreateRunActions(Helpers.Create<ContextActionMeleeAttack>()),
                Helpers.CreateAbilityTargetsAround(10.Feet(), Kingmaker.UnitLogic.Abilities.Components.TargetType.Enemy, null, 10.Feet()));
            WhirlwindAttackAbility.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.CoupDeGrace;
            Helpers.SetField(WhirlwindAttackAbility, "m_IsFullRoundAction", true);



            var WhirlwindAttackFeature = Helpers.CreateFeature("WhirlwindAttack", "Whirlwind Attack",
                "When you use the full-attack action, you can give up your regular attacks and instead make one melee attack at your highest base attack bonus against each opponent within reach. You must make a separate attack roll against each opponent.\nWhen you use the Whirlwind Attack feat, you also forfeit any bonus or extra attacks granted by other feats, spells, or abilities",
                "64f07282c2c44084a1037746231735a6",
                KineticWhirl.Icon,
                FeatureGroup.CombatFeat,
                Helpers.PrerequisiteStatValue(StatType.Dexterity, 13),
                Helpers.PrerequisiteStatValue(StatType.Intelligence, 13),
                Helpers.PrerequisiteFeature(Dodge),
                Helpers.PrerequisiteFeature(CombExpertise),
                Helpers.PrerequisiteFeature(Mobility));
            WhirlwindAttackFeature.ComponentsArray = WhirlwindAttackFeature.ComponentsArray.AddToArray(WhirlwindAttackAbility.CreateAddFact());
            WhirlwindAttackFeature.Groups = WhirlwindAttackFeature.Groups.AddToArray(FeatureGroup.Feat);
            library.AddCombatFeats(WhirlwindAttackFeature);
            


        }



        static void LoadRayShield()
        {

            var fighter = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var missileshield = library.Get<BlueprintFeature>("5ffcd225924514348ac71730179b5b24");
            var Deflect = library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5");
            var rayshield = Helpers.CreateFeature("rayshield", "Ray Shield",
            "You must be using a light, heavy, or tower shield to use this feat. Once per round when you would normally be hit with a ranged touch attack (including rays and similar magical effects), you may deflect it so that you take no damage from it.",
            "f7893079d7cb4759a06dbe3436653dd8",
            Deflect.Icon,
            FeatureGroup.CombatFeat,
            Helpers.PrerequisiteStatValue(StatType.Dexterity, 15),
            Helpers.PrerequisiteFeature(missileshield),
            Helpers.PrerequisiteClassLevel(fighter, 11, true),
            Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 15, true),
            Helpers.Create<DeflectRayLogic>()) ;
            
            rayshield.Groups = rayshield.Groups.AddToArray(FeatureGroup.Feat);
            library.AddCombatFeats(rayshield);

        }
       
        static void LoadLunge()
        {
            var furysfall = library.Get<BlueprintFeature>("0fc1ed8532168f74a9441bd17ad59e66");
            var LungeBuff = Helpers.CreateBuff("lungebuff", "Lunge", 
                "You can increase the reach of your melee attacks by 5 feet until the end of your turn by taking a –2 penalty " +
                "to your AC until your next turn.",
                 "83e847dcc58946108da49c138c71cfac",
                 furysfall.Icon,
                 null,
                 Helpers.CreateAddStatBonus(StatType.Reach, 5, ModifierDescriptor.None),
                 Helpers.CreateAddStatBonus(StatType.AC, -2, ModifierDescriptor.None));


            var LungeToggleAbility = Helpers.CreateActivatableAbility("lunge toggle", "Lunge",
           "You can increase the reach of your melee attacks by 5 feet until the end of your turn by taking a –2 penalty to " +
           "your AC until your next turn.",
                "8a03ac24e9054b1fabf871ee84bd5c20",
                furysfall.Icon,
                LungeBuff,
                AbilityActivationType.OnUnitAction,
                CommandType.Free, null);
            Helpers.SetField(LungeToggleAbility, "DeactivateImmediately", false);

            var Lungefeature = Helpers.CreateFeature("lunge", "Lunge",
           "You can increase the reach of your melee attacks by 5 feet until the end of your turn by taking a –2 penalty to " +
           "your AC until your next turn.",
           "21d8e1f4f0ae404dbbe219a37d60ff85",
           furysfall.Icon,
           FeatureGroup.CombatFeat,
           Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 6, true));
            Lungefeature.ComponentsArray = Lungefeature.ComponentsArray.AddToArray(LungeToggleAbility.CreateAddFact());
            Lungefeature.Groups = Lungefeature.Groups.AddToArray(FeatureGroup.Feat);
           library.AddCombatFeats(Lungefeature);
        }


        static void LoadInHarmsWay ()
        {
            var destructionaura = library.Get<BlueprintAbilityAreaEffect>("5a6c8bb6faf11fc4bb1022c3683d12d3");
            var bodyguard = library.Get<BlueprintFeature>("8ab9b28d4055449eb6133979eb2b7fe5");
            var cranewing = library.Get<BlueprintFeature>("af0aae1b973114f47a19ea532237b5fc");
            var isally = Helpers.Create<ContextConditionIsAlly>();
            var iscaster = Helpers.Create<ContextConditionIsCaster>();


            var InHarmsWaybuffcooldown = Helpers.CreateBuff("inharmswaycdbuff", "In Harm's Way",
                "blablou",
                "fc091c396d074b80bc642f976f5e114a",
                 bodyguard.Icon,
                 null);
            
            //InHarmsWaybuffcooldown.SetBuffFlags(BuffFlags.HiddenInUi);

            var ProtectedBuff = Helpers.CreateBuff("protected", "Protected",
                "blublu of his attack of opportunity to protect you against an attack.",
                 "b01a4cbec0c04ce8b53bdab9322d7c35",
                 bodyguard.Icon,
                 null,
                 Helpers.Create<InHarmsWayLogic>());
            //ProtectedBuff.SetBuffFlags(BuffFlags.HiddenInUi);

            var protectedAura = library.CopyAndAdd(destructionaura, "InHarmsWayArea", "2b48f17872134f1c87babed95d2191b3");
            protectedAura.Size = 100.Feet();
            protectedAura.AggroEnemies = false;
            protectedAura.AffectEnemies = false;
            protectedAura.SpellResistance = false;
            protectedAura.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            protectedAura.SetComponents(
                 Helpers.CreateAreaEffectRunAction(
                     unitEnter: Helpers.CreateConditional(iscaster, null, Helpers.CreateConditional(isally, Helpers.CreateApplyBuff(ProtectedBuff, Helpers.CreateContextDuration(), false, false, false, false, true))),
                     unitExit: Helpers.Create<ContextActionRemoveBuff>(r => r.Buff = ProtectedBuff))
                 );

            var InHarmsWaybuff = Helpers.CreateBuff("inharmswaybuff", "In Harm's Way",
                "ntil the end of your turn by taking a –2 penalty " +
                "to your AC until your next turn.",
                 "853253ecb9244387bd203d5fe7ba5353",
                 cranewing.Icon,
                 null,
                Helpers.Create<AddAreaEffect>(a => a.AreaEffect = protectedAura)
                 );
            


            var InHarmsWayToggleability = Helpers.CreateActivatableAbility("inharmswayability", "In Harm's Way",
                "When you use the full-attack action, you can give up your regular attacks and instead make one melee attack at your highest base attack bonus against each opponent within 10 feets. You must make a separate attack roll against each opponent.\nWhen you use the Whirlwind Attack feat, you also forfeit any bonus or extra attacks granted by other feats, spells, or abilities",
                "cc9143d403714c77af1e06f92228835d",
                cranewing.Icon,
                InHarmsWaybuff,
                AbilityActivationType.Immediately,
                CommandType.Swift,
                null);
            Helpers.SetField(InHarmsWayToggleability, "DeactivateImmediately", true);


            var InHarmsWayfeature = Helpers.CreateFeature("inharmswayfeature", "In Harm's Way",
                "While using the aid another action to improve an adjacent ally’s AC, you can intercept a successful " +
                "attack against that ally as an immediate action, taking full damage from that attack and any associated effects" +
                "(bleed, poison, etc.).A creature cannot benefit from this feat more than once per attack.",
                "6f172479e5b8473e9aef8b326763cba1",
                cranewing.Icon,
                FeatureGroup.CombatFeat,
                Helpers.PrerequisiteFeature(bodyguard));
            InHarmsWayfeature.ComponentsArray = InHarmsWayfeature.ComponentsArray.AddToArray(InHarmsWayToggleability.CreateAddFact());
            InHarmsWayfeature.Groups = InHarmsWayfeature.Groups.AddToArray(FeatureGroup.Feat);
            library.AddCombatFeats(InHarmsWayfeature);

        }

        static void LoadBodyGuard()
        {
            var isally = Helpers.Create<ContextConditionIsAlly>();
            var iscaster = Helpers.Create<ContextConditionIsCaster>();
            var Combreflexe = library.Get<BlueprintFeature>("0f8939ae6f220984e8fb568abbdfba95");
            var diehard = library.Get<BlueprintFeature>("86669ce8759f9d7478565db69b8c19ad");
            var missileshield = library.Get<BlueprintFeature>("5ffcd225924514348ac71730179b5b24");
            var destructionaura = library.Get<BlueprintAbilityAreaEffect>("5a6c8bb6faf11fc4bb1022c3683d12d3");

            var GuardedBuff = Helpers.CreateBuff("guarded", "Guarded",
                "An ally adjacent to you can use one of his attack of opportunity to protect you against an attack.",
                 "755c83c659de483aac7797d701f69e14",
                 missileshield.Icon,
                 null,
                 Helpers.Create<BodyguardBuffLogic>());
            //GuardedBuff.SetBuffFlags(BuffFlags.HiddenInUi);

            var GuardedAura = library.CopyAndAdd(destructionaura, "BodyGuard Area", "c8ee5206ee8f4aa3bc75ef37205219b3");
            GuardedAura.Size = 100.Feet();
            GuardedAura.AggroEnemies = false;
            GuardedAura.AffectEnemies = false;
            GuardedAura.SpellResistance = false;
            GuardedAura.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            GuardedAura.SetComponents(
                 Helpers.CreateAreaEffectRunAction(
                     unitEnter: Helpers.CreateConditional(iscaster, null, Helpers.CreateConditional(isally, Helpers.CreateApplyBuff(GuardedBuff, Helpers.CreateContextDuration(), false, false, false, false, true))),
        unitExit: Helpers.Create<ContextActionRemoveBuff>(r => r.Buff = GuardedBuff))
                 );
            
            var BodyGuardBuff = Helpers.CreateBuff("bodyguardbuff", "BodyGuard",
               "When an adjacent ally is attacked, you may use an attack of opportunity to improve your ally’s AC by 2.",
                "1c0fec0b27cd4270a1f5ff17c09c469e",
                Combreflexe.Icon,
                null,
                Helpers.Create<AddAreaEffect>(a => a.AreaEffect = GuardedAura)
                );

            var BodyGuardToggleAbility = Helpers.CreateActivatableAbility("bodyguard toggle", "BodyGuard",
                "When an adjacent ally is attacked, you may use an attack of opportunity to improve your ally’s AC by 2.",
                "818653e4484f45e2868b90f196eb2671",
                Combreflexe.Icon,
                BodyGuardBuff,
                AbilityActivationType.Immediately,
                CommandType.Free, null);
            Helpers.SetField(BodyGuardToggleAbility, "DeactivateImmediately", true);

            var BodyGuardfeature = Helpers.CreateFeature("bodyguard", "BodyGuard",
           "When an adjacent ally is attacked, you may use an attack of opportunity to improve your ally’s AC by 2.",
           "8ab9b28d4055449eb6133979eb2b7fe5",
           diehard.Icon,
           FeatureGroup.CombatFeat,
           Helpers.PrerequisiteFeature(Combreflexe));
            BodyGuardfeature.ComponentsArray = BodyGuardfeature.ComponentsArray.AddToArray(BodyGuardToggleAbility.CreateAddFact());
            BodyGuardfeature.Groups = BodyGuardfeature.Groups.AddToArray(FeatureGroup.Feat);
            library.AddCombatFeats(BodyGuardfeature);
        }
    }

    
    [ComponentName("Bodyguard's AC bonus against Attacks")]
    [AllowedOn(typeof(BlueprintBuff))]
    [AllowMultipleComponents]
    public class BodyguardBuffLogic : BuffLogic, ITargetRulebookHandler<RuleAttackWithWeapon>, IRulebookHandler<RuleAttackWithWeapon>, ITargetRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            UnitEntityData maybeCaster = base.Buff.Context.MaybeCaster;
            if (maybeCaster == null || evt.Target == maybeCaster)
            {
                return;
            }
            if (this.Check(maybeCaster))
            {
                this.m_Mod = evt.Target.Stats.AC.AddModifier(2, this, ModifierDescriptor.None);
                evt.Target.Stats.AC.AddModifier(2, this, ModifierDescriptor.None);
                maybeCaster.CombatState.AttackOfOpportunityCount--;
            }
        }
        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {

            if (this.m_Mod != null)
            {
                this.m_Mod.Remove();
                evt.Target.Stats.AC.AddModifier(2, this, ModifierDescriptor.None).Remove();
            }
            this.m_Mod = null;
        }


        private bool Check(UnitEntityData caster)
        {
            return caster != null  && caster.DistanceTo(base.Owner.Unit) <= caster.Stats.Reach.BaseValue.Feet().Meters + caster.View.Corpulence / 2f + base.Owner.Unit.View.Corpulence / 2f + 5.Feet().Meters && caster.Descriptor.State.CanAct && caster.CombatState.CanAttackOfOpportunity;
        }

        public ModifierDescriptor Descriptor;
        public ContextValue Value;
        private ModifiableValue.Modifier m_Mod;
    }



    [ComponentName("AC bonus against Attacks")]
    [AllowedOn(typeof(BlueprintBuff))]
    [AllowMultipleComponents]
    public class InHarmsWayLogic : BuffLogic, ITargetRulebookHandler<RuleAttackWithWeapon>, IRulebookHandler<RuleAttackWithWeapon>, ITargetRulebookSubscriber
    {
        static LibraryScriptableObject library => Main.library;
        public BlueprintBuff CooldownBuff = library.Get<BlueprintBuff>("fc091c396d074b80bc642f976f5e114a");

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            UnitEntityData maybeCaster = base.Buff.Context.MaybeCaster;
            Log.Write(maybeCaster.CharacterName);
            if (maybeCaster == null)
            {
                return;
            }
            Log.Write(Check().ToString());

            if (this.Check())
            {
                evt.NewTarget = maybeCaster;
                evt.ReplaceTarget = true;
                Log.Write("checking:"+ evt.NewTarget.CharacterName);
                Helpers.CreateApplyBuff(CooldownBuff, Helpers.CreateContextDuration(1, DurationRate.Rounds), false, true, false, false);
            }
            Log.Write("end");
        }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
        }

        private bool Check()
        {
            Log.Write("checking");
            UnitEntityData maybeCaster = base.Buff.Context.MaybeCaster;
            return maybeCaster != null  && maybeCaster.DistanceTo(base.Owner.Unit) <= maybeCaster.Stats.Reach.BaseValue.Feet().Meters + maybeCaster.View.Corpulence / 2f + base.Owner.Unit.View.Corpulence / 2f + 5.Feet().Meters && maybeCaster.Descriptor.State.CanAct && !maybeCaster.Descriptor.State.HasCondition(UnitCondition.CanNotAttack) && !maybeCaster.Descriptor.State.HasCondition(UnitCondition.Confusion) && !maybeCaster.Descriptor.HasFact(this.CooldownBuff);
        }

       
    }

    public class DeflectRayLogic : RuleTargetLogicComponent<RuleAttackRoll>
    {
        private TimeSpan LastDeflectRayTime;
        bool canbedeflected = false;
        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt.WeaponStats.Weapon.Blueprint.AttackType.IsRanged() && evt.WeaponStats.Weapon.Blueprint.AttackType.IsTouch())
            {
                canbedeflected = true;
                evt.SuspendCombatLog = false;
            }
        }
        public override void OnEventDidTrigger(RuleAttackRoll evt)
        {
            TimeSpan gameTime = Kingmaker.Game.Instance.TimeController.GameTime;
            if (canbedeflected && evt.IsHit && !(gameTime - LastDeflectRayTime < 1.Rounds().Seconds) && (evt.Target.Body.PrimaryHand.HasShield || evt.Target.Body.SecondaryHand.HasShield))
            {
                evt.SetFake(AttackResult.Miss);
                Kingmaker.Game.Instance.UI.BattleLogManager.HandleUnitDeflectArrow(evt.Target);
             
                LastDeflectRayTime = gameTime;
            }
        }
        
    }

    public class Addrayshiekdtoenemy
    {
        static LibraryScriptableObject library => Main.library;
        internal static void Load()
        {
            Main.SafeLoad(getennemy, "truc");
        }
        public static void getennemy()
        {
            var Deflect = library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5");
            var fighter = Helpers.GetClass("48ac8db94d5de7645906c7d0ad3bcfbd");
            var warrior = library.Get<BlueprintCharacterClass>("b2d9af52cf680744eb0cdc3f3034395f");
            var corrupted = library.Get<BlueprintCharacterClass>("87ad019eeca7abf45ad0d68b8edc32e5");
            var paladin = Helpers.GetClass("bfa11238e7ae3544bbeb4d0b92e897ec");
            var allunit = Helpers.ennemies;
            var playerfaction = library.Get<BlueprintFaction>("72f240260881111468db610b6c37c099");
            var RSfeat = library.Get<BlueprintFeature>("f7893079d7cb4759a06dbe3436653dd8");

            foreach (var unit in allunit)
            {
                var id = unit.AssetGuid;
                if (unit.Faction == playerfaction || unit.Faction == null) { continue; }
                if (unit.GetComponent<AddClassLevels>() == null) { continue; }
                if (unit.GetComponent<Kingmaker.Blueprints.Classes.Experience.Experience>() == null || unit.GetComponent<Kingmaker.Blueprints.Classes.Experience.Experience>().CR < 6) { continue; }
                var unitclass = unit.GetComponent<AddClassLevels>().CharacterClass;
                if (unitclass == fighter || unitclass == warrior || unitclass == corrupted || unitclass == paladin)
                {
                    if (unit.Body.SecondaryHand != null && unit.Body.SecondaryHand.ItemType == Kingmaker.UI.Common.ItemsFilter.ItemType.Shield)
                    {
                        unit.AddFacts = unit.AddFacts.AddToArray(RSfeat);
                    }
                }
            }

        }
    }
}

