

using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
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
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker;
using Kingmaker.GameModes;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.ResourceLinks;

using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Abilities.Components.Base;
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

namespace thelostgrimoire
{
    class NewSpell
    {

        static LibraryScriptableObject library => Main.library;
        static BlueprintCharacterClass wizardclass = Main.library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
        static BlueprintSpellList hunterlist = library.TryGet<BlueprintSpellList>("b161506e0b8f4116806a243f6838ae01");
        static BlueprintSpellList witchlist = library.TryGet<BlueprintSpellList>("422490cf62744e16a3e131efd94cf290");
        static BlueprintSpellList shamanlist = library.TryGet<BlueprintSpellList>("7113337f695742559ecdecc8905b132a");


        internal static void Load()
        {
            // Load  spell
            Main.SafeLoad(CreateCounterSpells, "zapy zap");

            //needed patch
           
        }

        public static void CreateCounterSpells()
        {
         

            var icon = Helpers.GetIcon("20a2435574bdd7f4e947f405df2b25ce");
            var icon2 = Helpers.GetIcon("ee30d97c101a2ce4da44ef5745426267");
            var icon3 = Helpers.GetIcon("de18c849c41dbfa44801d812376c707d");
            var name = "CounterSpell";
            var fx = new PrefabLink();
            fx.AssetId = "3eda0e7f710821045a35ebe432af667c";//Dispel Magic
            var fx2 = new PrefabLink();
            fx2.AssetId = "c388856d0e8855f429a83ccba67944ba";//commun divination buff

            var bufflesser = Helpers.CreateBuff(name+"LesserBuff", "Lesser Counterspell", "The next spell this creature will cast has a chance to be blocked and wasted.", Helpers.getGuid(name+"LesserBuff"), icon, fx, fx,
                Helpers.Create<ArcaneDiscoveries.CounterSummonSpell>(p => {
                    p.bonus = 0;
                    p.type = ArcaneDiscoveries.CounterSummonSpell.source.spell;
                    
                })
                );
            var buff = Helpers.CreateBuff(name + "Buff", "Counterspell", "The next spell this creature will cast has a chance to be blocked and wasted.", Helpers.getGuid(name + "Buff"), icon2, fx, fx,
                Helpers.Create<ArcaneDiscoveries.CounterSummonSpell>(p => {
                    p.bonus = 2;
                    p.type = ArcaneDiscoveries.CounterSummonSpell.source.spell;

                }));
            var buffgreater = Helpers.CreateBuff(name + "GreaterBuff", "Greater Counterspell", "The next spell this creature will cast has a chance to be blocked and wasted.", Helpers.getGuid(name + "GreaterBuff"), icon3, fx, fx,
                Helpers.Create<ArcaneDiscoveries.CounterSummonSpell>(p => {
                    p.bonus = 5;
                    p.type = ArcaneDiscoveries.CounterSummonSpell.source.spell;

                }));

            var abilitylesser = Helpers.CreateAbility(name+"LesserAbility", "Lesser Counterspell", "This spell allows the caster to see glimpses of the future, immediately learning what spell their target will cast, if any, before the start of their next round.\nOnce the caster learn the nature of this spell, they automatically spend a standard action to prepare a counterspell and start accumulating magical energy to disrupt the future spell.\n"+
                "Then, when the target finally cast their spell, the caster release their counterspell as an immediate action and must make a Caster level check against a DC equal to 11 + the spell level +the target caster level.\nIf the check is successful, the target's spell is blocked and wasted.", 
                Helpers.getGuid(name+"LesserAbility"), icon, AbilityType.Spell, CommandType.Standard, AbilityRange.Medium, "1 round", "",
                Helpers.CreateRunActions(Helpers.CreateApplyBuff(bufflesser, Helpers.CreateContextDuration(1, DurationRate.Rounds), true, false, false)),
                Helpers.Create<AbilitySpawnFx>(f =>
                 {
                     f.PrefabLink = fx2;
                     f.Anchor = AbilitySpawnFxAnchor.Caster;
                     f.DestroyOnCast = true;
                 }),
               Helpers.CreateSpellComponent(SpellSchool.Divination)
                );
            abilitylesser.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
            abilitylesser.CanTargetPoint = false;
            abilitylesser.CanTargetSelf = false;
            abilitylesser.CanTargetEnemies = true;
            abilitylesser.CanTargetFriends = true;
            abilitylesser.AvailableMetamagic = Metamagic.Heighten | Metamagic.Reach;
            abilitylesser.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            abilitylesser.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            
            Helpers.SetField(abilitylesser, "m_IsFullRoundAction", true);
            abilitylesser.ResourceAssetIds = new string[] { fx.AssetId, fx2.AssetId };

            var ability = Helpers.CreateAbility(name + "Ability", "Counterspell", "This spell is similar to Lesser Counterspell except that the caster level check receive a bonus of +2 to their caster level check.\n\n" +
                "LESSER COUNTERSPELL :\nThis spell allows the caster to see glimpses of the future, immediately learning what spell their target will cast, if any, before the start of their next round.\nOnce the caster learn the nature of this spell, they automatically spend a standard action to prepare a counterspell and start accumulating magical energy to disrupt the future spell.\n" +
                "Then, when the target finally cast their spell, the caster release their counterspell as an immediate action and must make a Caster level check against a DC equal to 11 + the spell level +the target caster level.\nIf the check is successful, the target's spell is blocked and wasted.", Helpers.getGuid(name + "Ability"), icon2, AbilityType.Spell, CommandType.Standard, AbilityRange.Medium, "1 round", "",

                Helpers.CreateRunActions(Helpers.CreateApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Rounds), true, false, false)),
                Helpers.Create<AbilitySpawnFx>(f =>
                {
                    f.PrefabLink = fx2;
                    f.Anchor = AbilitySpawnFxAnchor.Caster;
                    f.DestroyOnCast = true;
                }),
               Helpers.CreateSpellComponent(SpellSchool.Divination)
                );
            ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
            ability.CanTargetPoint = false;
            ability.CanTargetSelf = false;
            ability.CanTargetEnemies = true;
            ability.CanTargetFriends = true;
            ability.AvailableMetamagic = Metamagic.Heighten | Metamagic.Reach;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            Helpers.SetField(ability, "m_IsFullRoundAction", true);
            ability.ResourceAssetIds = new string[] { fx.AssetId, fx2.AssetId };



            var abilitygreater = Helpers.CreateAbility(name + "GreaterAbility", "Greater Counterspell", "This spell is similar to Lesser Counterspell except that the caster level check receive a bonus of +5 to their caster level check.\n\n" +
                "LESSER COUNTERSPELL :\nThis spell allows the caster to see glimpses of the future, immediately learning what spell their target will cast, if any, before the start of their next round.\nOnce the caster learn the nature of this spell, they automatically spend a standard action to prepare a counterspell and start accumulating magical energy to disrupt the future spell.\n" +
                "Then, when the target finally cast their spell, the caster release their counterspell as an immediate action and must make a Caster level check against a DC equal to 11 + the spell level +the target caster level.\nIf the check is successful, the target's spell is blocked and wasted.", Helpers.getGuid(name + "GreaterAbility"), icon3, AbilityType.Spell, CommandType.Standard, AbilityRange.Medium, "1 round", "",

                Helpers.CreateRunActions(Helpers.CreateApplyBuff(buffgreater, Helpers.CreateContextDuration(1, DurationRate.Rounds), true, false, false)),
                Helpers.Create<AbilitySpawnFx>(f =>
                {
                    f.PrefabLink = fx2;
                    f.Anchor = AbilitySpawnFxAnchor.Caster;
                    f.DestroyOnCast = true;
                }),
               Helpers.CreateSpellComponent(SpellSchool.Divination)
                );
            abilitygreater.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
            abilitygreater.CanTargetPoint = false;
            abilitygreater.CanTargetSelf = false;
            abilitygreater.CanTargetEnemies = true;
            abilitygreater.CanTargetFriends = true;
            abilitygreater.AvailableMetamagic = Metamagic.Heighten | Metamagic.Reach;
            abilitygreater.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            abilitygreater.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            Helpers.SetField(abilitygreater, "m_IsFullRoundAction", true);
            abilitygreater.ResourceAssetIds = new string[] { fx.AssetId, fx2.AssetId };

            abilitylesser.AddToSpellList(Helpers.wizardSpellList, 1);
            abilitylesser.AddToSpellList(Helpers.clericSpellList, 1);
            abilitylesser.AddToSpellList(Helpers.druidSpellList, 1);
            abilitylesser.AddToSpellList(Helpers.bardSpellList, 1);
            abilitylesser.AddToSpellList(Helpers.inquisitorSpellList, 1);
            abilitylesser.AddToSpellList(Helpers.magusSpellList, 1);
            
            ability.AddToSpellList(Helpers.wizardSpellList, 4);
            ability.AddToSpellList(Helpers.clericSpellList, 4);
            ability.AddToSpellList(Helpers.druidSpellList, 4);
            ability.AddToSpellList(Helpers.bardSpellList, 4);
            ability.AddToSpellList(Helpers.inquisitorSpellList, 4);
            ability.AddToSpellList(Helpers.magusSpellList, 4);

            abilitygreater.AddToSpellList(Helpers.wizardSpellList, 7);
            abilitygreater.AddToSpellList(Helpers.clericSpellList, 7);
            abilitygreater.AddToSpellList(Helpers.druidSpellList, 7);
            
            if(hunterlist != null)
            {
                abilitylesser.AddToSpellList(hunterlist, 1);
                ability.AddToSpellList(hunterlist, 4);

            }
            if (witchlist != null)
            {
                abilitylesser.AddToSpellList(witchlist, 1);
                ability.AddToSpellList(witchlist, 4);
                abilitygreater.AddToSpellList(witchlist, 7);

            }
            if (shamanlist != null)
            {
                abilitylesser.AddToSpellList(shamanlist, 1);
                ability.AddToSpellList(shamanlist, 4);
                abilitygreater.AddToSpellList(shamanlist, 7);

            }
            Helpers.AddSpell(abilitylesser);
            Helpers.AddSpell(ability);
            Helpers.AddSpell(abilitygreater);

            
        }

        public class SpendAction : BlueprintComponent, IAbilityOnCastLogic
        {

            public void  OnCast(AbilityExecutionContext Context)
            {
                Log.Write("on cast");
                switch(Action)
                {
                    case CommandType.Standard:
                        Context.Caster.Descriptor.Unit.CombatState.Cooldown.StandardAction = 6f;
                        break;

                    case CommandType.Move:
                        Context.Caster.Descriptor.Unit.CombatState.Cooldown.MoveAction = 6f;
                        break;

                    case CommandType.Swift:
                        Context.Caster.Descriptor.Unit.CombatState.Cooldown.SwiftAction = 6f;
                        break;

                    case CommandType.Free:
                        break;

                }
        }

          

            public CommandType Action;
            
            

            

        }
        

    }
}
