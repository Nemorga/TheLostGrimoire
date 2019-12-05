

using System;
using System.Reflection;
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
using Kingmaker.Blueprints.Items;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.View.MapObjects;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Controllers;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.EntitySystem;
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
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Commands;
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
using Kingmaker.View;
using Kingmaker.Controllers.Projectiles;
using Pathfinding.Util;
using Harmony12;
using Newtonsoft.Json;
using UnityEngine;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.DialogSystem;

namespace thelostgrimoire
{
    class NewSpell
    {

        static LibraryScriptableObject library => Main.library;
        static Sprite GetIcon(string id) => Helpers.GetIcon(id);
        static BlueprintAbility GetAbility(string id) => library.Get<BlueprintAbility>(id);
        static BlueprintFeature GetFeat(string id) => library.Get<BlueprintFeature>(id);
        static BlueprintBuff GetBuff(string id) => library.Get<BlueprintBuff>(id);
        static BlueprintCharacterClass wizardclass = Main.library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
        static BlueprintSpellList hunterlist = library.TryGet<BlueprintSpellList>("b161506e0b8f4116806a243f6838ae01");
        static BlueprintSpellList witchlist = library.TryGet<BlueprintSpellList>("422490cf62744e16a3e131efd94cf290");
        static BlueprintSpellList shamanlist = library.TryGet<BlueprintSpellList>("7113337f695742559ecdecc8905b132a");
        static string Guid(string name) => Helpers.getGuid(name);

        //creature type
        static BlueprintFeature Undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
        static BlueprintFeature Construct = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
        static BlueprintFeature Dragon = library.Get<BlueprintFeature>("455ac88e22f55804ab87c2467deff1d6");
        static BlueprintFeature Plant = library.Get<BlueprintFeature>("706e61781d692a042b35941f14bc41c5");
        static BlueprintFeature Vermine = library.Get<BlueprintFeature>("09478937695300944a179530664e42ec");

        static BlueprintAbility Spell(String name, String displayName,
            String description, Sprite icon, AbilityRange range, CommandType actionType = CommandType.Standard, string var = "",
            String duration = "", String savingThrow = "",
            params BlueprintComponent[] components) => Helpers.CreateAbility(name + var + "Spell", displayName, description, Guid(name + var + "Spell"), icon, AbilityType.Spell, actionType, range, duration, savingThrow, components);
        static BlueprintBuff spellbuff(String name, String displayName, String description, Sprite icon,
            PrefabLink fxOnStart = null, PrefabLink FxOnRemove = null, StackingType stack = StackingType.Replace, BuffFlags flags = BuffFlags.IsFromSpell,
            params BlueprintComponent[] components)
        {
            BlueprintBuff buff = Helpers.CreateBuff(name + "Buff", displayName, description, Guid(name + "Buff"), icon, fxOnStart, FxOnRemove, components);
            buff.Stacking = stack;
            buff.SetBuffFlags(flags);

            return buff;
        }
        static BlueprintBuff tokenbuff(string name) => spellbuff(name + "Token", "", "", wizardclass.Icon, null, null, StackingType.Replace, BuffFlags.HiddenInUi);
        static AbilityEffectRunAction RunAction(params GameAction[] actions) => Helpers.CreateRunActions(actions);
        static SpellComponent Divination = Helpers.CreateSpellComponent(SpellSchool.Divination);
        static PrefabLink CommonDivination = Helpers.GetFx("c388856d0e8855f429a83ccba67944ba");


        internal static void Load()
        {
            // Load  spell
            Main.SafeLoad(CreateCounterSpells, "zapy zap");
            Main.SafeLoad(CreateImprovedTrueStrike, "never miss");
            Main.SafeLoad(CreateClairevoyance, "see far");
            Main.SafeLoad(CreateDetectEnemy, "see you");
            Main.SafeLoad(CreateHeigthenedAwerness, "feel my senses");
            Main.SafeLoad(CreateInsectSpies, "fly fly my minions!");
            Main.SafeLoad(CreateContacPLane, ":p");
            //needed patch
            Main.ApplyPatch(typeof(IgnoreAppoachPatch), "please work");
            Main.ApplyPatch(typeof(AugmentDetectionRadiusPacth), "please work");
        }


        public static void CreateContacPLane()
        {

            var AnswerYes = BookEvents.CreateAnswer("Test", 0, "Yes");
            var AnswerNo = BookEvents.CreateAnswer("Test", 1, "No");
            var AnswerBye = BookEvents.CreateAnswer("Test", 2, "Close");

            var CueFirst = BookEvents.CreateCue("Test", 0, "Do you want unlimited power?");
            var CuetoYes = BookEvents.CreateCue("Test", 1, "Too Bad! You're Ugly", conditions:new Condition[] { Helpers.Create<AnswerSelected>(s => { s.Answer = AnswerYes; s.CurrentDialog = true; }) });
            var CuetoNo = BookEvents.CreateCue("Test", 2, "Meh You're no fun", conditions: new Condition[] { Helpers.Create<AnswerSelected>(s => { s.Answer = AnswerNo; s.CurrentDialog = true; }) });


            var BookPage = BookEvents.CreateBookPage("Test", 0, new List<BlueprintCueBase> { CueFirst, CuetoNo, CuetoYes }, new List<BlueprintAnswerBase> { AnswerYes, AnswerNo, AnswerBye }, "1dabb49b2d57f1b4da20d50b1e503069");
            var Dialog = BookEvents.CreateBookEvent("test", BookEvents.CreateCueSelection(new List<BlueprintCueBase> {BookPage }));

            BookEvents.CreateTree(
                new (BlueprintAnswer, List<BlueprintCueBase>, Strategy)[] {
                (AnswerYes, new List<BlueprintCueBase> { CuetoYes}, Strategy.First),
                (AnswerNo, new List<BlueprintCueBase> { CuetoNo}, Strategy.First),
                (AnswerBye, new List<BlueprintCueBase> {}, Strategy.First)
            },
                new (BlueprintAnswerBase, BlueprintScriptableObject)[]
            {
                (AnswerBye, BookPage), (AnswerYes, BookPage),(AnswerNo, BookPage)
            },
                new (BlueprintCueBase, BlueprintScriptableObject)[] 
                {
                    (BookPage, Dialog),
                    (CueFirst, BookPage), 
                    (CuetoNo, BookPage),
                    (CuetoYes, BookPage)
                });




            Sprite icon = GetIcon("4cf3d0fae3239ec478f51e86f49161cb");
            string name = "Contact";
            string display = "Contact";
            string desc = "bcuiau";
            var spell = Spell(name, display, desc, icon, AbilityRange.Personal, components: new BlueprintComponent[] {
                RunAction(Helpers.Create<StartDialog>(d =>{
                    d.Dialogue = Dialog;
                    
                    })),
                Divination
            }) ;
            spell.AvailableMetamagic = Metamagic.Heighten;
            Helpers.AddSpell(spell);
            spell.AddToSpellList(Helpers.wizardSpellList, 1);


        }
        public static void CreateInsectSpies()
        {
            Sprite icon = GetIcon("4cf3d0fae3239ec478f51e86f49161cb");//to set
            string name = "UnboundSight";
            string display = "Unbound Sight";
            string desc = "This spell allows you to project your sight and other senses to another nearby place. This grants you the ability to search for trap and hidden object while being farther than would normaly be needed for the task.\nNote: Most trap and hidden objet can only be detected if a character is within 5-8 meters  (~=17-27 feet) of it. This spell increase this distance by 5 meters (~= 17 feet)";
            string descG = "This spell function like " + display + " except that it can be casted on an ally.\n" + display + " :" + desc;


            var buff = tokenbuff(name);
            var Gbuff = tokenbuff(name+"Greater");

            var spell = Spell(name, display, desc, icon, AbilityRange.Personal, duration: "1 minute/ level", components: new BlueprintComponent[] {
                Divination,
                RunAction(Helpers.CreateApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), true, true,true))
            });
            spell.SetCantarget(self: true);
            spell.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten;
            spell.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Self;

            Helpers.AddSpell(spell);
            spell.AddToSpellList(Helpers.wizardSpellList, 2);
            spell.AddToSpellList(Helpers.rangerSpellList, 1);
            spell.AddToSpellList(Helpers.druidSpellList, 2);
            if (witchlist != null)
                spell.AddToSpellList(witchlist, 2);

            var Gpell = Spell(name + "Greater", display + ", Greater", descG, icon, AbilityRange.Touch, duration: "1 minute/ level", components: new BlueprintComponent[] {
                Divination, 
                RunAction(Helpers.CreateApplyBuff(Gbuff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), true, true,true))
            });
            Gpell.SetCantarget(self: true, allies: true);
            Gpell.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            Gpell.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;

            Helpers.AddSpell(Gpell);
            Gpell.AddToSpellList(Helpers.wizardSpellList, 5);
            spell.AddToSpellList(Helpers.rangerSpellList, 4);
            spell.AddToSpellList(Helpers.druidSpellList, 4);
            if (witchlist != null)
                spell.AddToSpellList(witchlist, 5);

        }
        public class AugmentLineOfSight : AbilityAreaEffectLogic
        {
            protected override void OnTick(MechanicsContext context, AreaEffectEntityData areaEffect)
            {
                var caster = context.MainTarget.Unit;
                if (!context.MainTarget.IsUnit)
                {
                    Log.Write("NOPE!!");
                    return;
                }
                List<StaticEntityData> list = ListPool<StaticEntityData>.Claim();
                AreaPersistentState loadedAreaState = Game.Instance.State.LoadedAreaState;
                loadedAreaState.CollectAllEntities<StaticEntityData>(list);
                if (context.MainTarget.Unit.HasMotionThisTick)
                {
                    foreach (StaticEntityData Entity in list)
                    {
                        if (Entity.IsInGame && !Entity.IsInFogOfWar)
                        {
                            float num = caster.DistanceTo(Entity.View.transform.position);
                            if (!Entity.IsPerceptionCheckPassed && Entity.View.PerceptionCheckComponent == null)
                            {
                                Entity.IsPerceptionCheckPassed = true;
                            }
                            else if (Entity.IsPerceptionRollAllowed(caster) && num < Entity.View.PerceptionCheckComponent.Radius && caster.HasLOS(Entity.View.transform))
                            {
                                RollPerception(caster, Entity);
                            }
                        }
                    }
                }
            }
            private static void RollPerception(UnitEntityData character, StaticEntityData data)
            {
                int dc = data.View.PerceptionCheckComponent.DC;
                RuleSkillCheck ruleSkillCheck = Rulebook.Trigger<RuleSkillCheck>(new RuleSkillCheck(character, StatType.SkillPerception, dc)
                {
                    Reason = data
                });
                data.LastPerceptionRollRank[character] = character.Stats.SkillPerception.BaseValue;
                data.IsPerceptionCheckPassed = ruleSkillCheck.IsPassed;
                if (ruleSkillCheck.IsPassed)
                {
                    EventBus.RaiseEvent<IPerceptionHandler>(delegate (IPerceptionHandler h)
                    {
                        h.OnEntityNoticed(data, character);
                    });
                }
               
            }

        }

        public static void CreateHeigthenedAwerness()
        {
            Sprite icon = GetIcon("4093d5a0eb5cae94e909eb1e0e1a6b36");
            string name = "HeightenedAwareness";
            string display = "Heightened Awareness";
            string desc = "You enter a heightened state of awareness that allows you to notice more about your surroundings and recall information effortlessly. You gain a +2 competence bonus on Perception checks, Initiative checks and on all Knowledge checks.";

            BlueprintBuff buff = spellbuff(name, display, desc, icon, CommonDivination, components: new BlueprintComponent[] {
                Helpers.CreateAddStatBonus(StatType.SkillPerception, 2, ModifierDescriptor.Competence),
                Helpers.CreateAddStatBonus(StatType.Initiative, 2, ModifierDescriptor.Competence),
                Helpers.CreateAddStatBonus(StatType.SkillKnowledgeArcana, 2, ModifierDescriptor.Competence),
                Helpers.CreateAddStatBonus(StatType.SkillKnowledgeWorld, 2, ModifierDescriptor.Competence),
                Helpers.CreateAddStatBonus(StatType.SkillLoreNature, 2, ModifierDescriptor.Competence),
                Helpers.CreateAddStatBonus(StatType.SkillLoreReligion, 2, ModifierDescriptor.Competence)
            });

            BlueprintAbility spell = Spell(name, display, desc, icon, AbilityRange.Personal, CommandType.Standard, duration:"10 minutes/level", 
                components: new BlueprintComponent[] {
                    Helpers.CreateRunActions(Helpers.CreateApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.TenMinutes), true, true, true)),
                    Helpers.CreateSpellComponent(SpellSchool.Divination)
                });
            spell.SetCantarget(self: true);
            spell.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            spell.SetEffectOn();
            spell.AvailableMetamagic = Metamagic.Quicken | Metamagic.Extend | Metamagic.Heighten;

            Helpers.AddSpell(spell);
            spell.AddToSpellList(Helpers.wizardSpellList, 1);
            spell.AddToSpellList(Helpers.alchemistSpellList, 1);
            spell.AddToSpellList(Helpers.bardSpellList, 1);
            spell.AddToSpellList(Helpers.druidSpellList, 1);
            spell.AddToSpellList(Helpers.inquisitorSpellList, 1);
            spell.AddToSpellList(Helpers.rangerSpellList, 1);
            if(shamanlist != null)
                spell.AddToSpellList(shamanlist, 1);

        }

        public static void CreateDetectEnemy()
        {
            Sprite icon = GetIcon("82962a820ebc0e7408b8582fdc3f4c0c");
            string name = "DetectEnemy";
            string display = "Discern Enemies' Location";
            string desc = "A discern location spell is among the most powerful means of locating creatures or objects. Nothing short of a mind blank spell keep you from learning the exact location of all the hostile creature in the area you are in. The spell reveals the name of each creature and its location.\nNote: this spell adds map marker on the local map.";
            BlueprintUnitFact POI = library.Get<BlueprintUnitFact>("d74ba40ce400c854f9487b720550cc82");
            BlueprintUnitFact marker_POI = library.Get<BlueprintUnitFact>("a1fac90afc6b3814f9e89bcdcd9fda24");
            BlueprintUnitFact marker_unit = library.Get<BlueprintUnitFact>("86c643ff85c5d1843bb8ec3d15f6ff24");
            BlueprintUnitFact marker_POI_always = library.Get<BlueprintUnitFact>("8d665cf41716dc24581137ee84ecec27");
            BlueprintUnitFact marker_VIT = library.Get<BlueprintUnitFact>("c4cbe77f822100f4d85e907fa9a50e9a");
            BlueprintBuff MindBlank = GetBuff("35f3724d4e8877845af488d167cb8a89");

            var marker = Helpers.CreateFeature(name + "marker", "", "", Guid(name + "marker"), icon, FeatureGroup.None,
                Helpers.Create<AddLocalMapMarker>(m => { m.Type = Kingmaker.UI.ServiceWindow.LocalMap.LocalMap.MarkType.Poi; m.ShowIfNotRevealed = true; })
                );

            var buff = spellbuff(name, display, desc, icon, components: new BlueprintComponent[] {
                Helpers.Create<AddTemporaryFeat>(t => t.Feat = marker),
                Helpers.CreateSpellComponent(SpellSchool.Divination)
            });
            buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.IsFromSpell);

            var spell = Spell(name, display, desc, icon, AbilityRange.Unlimited, CommandType.Standard,"", "10 minutes", "Will", 
                Helpers.CreateRunActions(Helpers.Create<ApplySuperBuff>(a => { a.buff = buff; a.avoid = new BlueprintUnitFact[] {POI, marker_POI, marker_POI_always, marker_unit, marker_VIT, MindBlank }; })),
                Helpers.CreateSpellComponent(SpellSchool.Divination)
                );
            spell.SpellResistance = true;
            spell.AvailableMetamagic = Metamagic.Heighten;
            spell.SetCantarget(self: true);
            spell.SetEffectOn(foe: AbilityEffectOnUnit.Harmful);
            spell.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            spell.SetFullround();
            spell.MaterialComponent.Item = library.Get<BlueprintItem>("92752bbbf04dfa1439af186f48aee0e9");
            spell.MaterialComponent.Count = 1;
            Helpers.AddSpell(spell);

            spell.AddToSpellList(Helpers.wizardSpellList, 8);
            spell.AddToSpellList(Helpers.clericSpellList, 8);
            if (witchlist != null)
                spell.AddToSpellList(witchlist, 8);
            if (shamanlist != null)
                spell.AddToSpellList(shamanlist, 8); 


        }
        public class ApplySuperBuff : ContextAction
        {
            public override string GetCaption()
            {
                return string.Format("Detecting Enemies");
            }

            public override void RunAction()
            {
                foreach (UnitEntityData unit in Game.Instance.State.Units)
                {
                    if(unit.IsPlayersEnemy && !unit.Descriptor.State.IsDead && unit.Faction != trap)
                    {
                        bool dont = false;
                        foreach(BlueprintUnitFact avoidit in avoid)
                        {
                            if (unit.Descriptor.HasFact(avoidit))
                                dont = true;
                        }
                        if (!dont)
                        {
                            RuleSavingThrow Save = Context.TriggerRule(new RuleSavingThrow(unit, SavingThrowType.Will, Context.Params.DC));
                            if(!Save.IsPassed)
                                unit.Descriptor.AddBuff(buff, Context, new TimeSpan?(10.Minutes()));

                        }

                    }
                }
            }
            public BlueprintBuff buff;
            public BlueprintUnitFact[] avoid;
            static BlueprintFaction trap = library.Get<BlueprintFaction>("d75c5993785785d468211d9a1a3c87a6");
                
        }

        public static void CreateClairevoyance()
        {
            Sprite icon = GetIcon("4cf3d0fae3239ec478f51e86f49161cb");
            string name = "Clairevoyance";
            string display = "Clairaudience-Clairvoyance";
            string desc = "Clairaudience/clairvoyance creates an invisible magical sensor at a specific location that enables you to hear or see (your choice) almost as if you were there." +
                "You don’t need line of sight or line of effect, but the locale must be in the same area you are now. Once you have selected the locale, the sensor doesn’t move, " +
                "but you can view the area as desired in all direction.";
            PrefabLink Aurafx = Helpers.GetFx("01068bf9f84d7344d8339eb148c14cf5");

            var Area = Helpers.Create<BlueprintAbilityAreaEffect>(a =>
            {
                a.name = name + "Aura";
                a.AffectEnemies = false;
                a.AggroEnemies = false;
                a.Fx = Aurafx;
                a.Shape = AreaEffectShape.Cylinder;
                a.Size = 30.Feet();
                
            });
            Area.AddComponent(Helpers.Create<AOERevealZone>(a => a.AreaBlueprint = Area));
            library.AddAsset(Area, Helpers.getGuid(Area.name));

            BlueprintAbility spell = Spell(name, display, desc, icon, AbilityRange.Unlimited, CommandType.Standard, "", "1 min./level", "",
                Helpers.CreateRunActions(Helpers.Create<ContextActionSpawnAreaEffect>(a =>
                {
                    a.AreaEffect = Area;
                    a.DurationValue = Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes);
                })),
                Helpers.CreateSpellComponent(SpellSchool.Divination),
                Helpers.Create<NoApproach>()
                );
            spell.SetCantarget(point: true);
            spell.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Omni;
            spell.SetFullround();
            spell.SetEffectOn();
            spell.AvailableMetamagic = Metamagic.Extend;
            spell.AddToSpellList(Helpers.wizardSpellList, 3);
            spell.AddToSpellList(Helpers.bardSpellList, 3);
            if(witchlist!= null)
                spell.AddToSpellList(witchlist, 3);
            if(shamanlist != null)
                spell.AddToSpellList(shamanlist, 3);

            Helpers.AddSpell(spell);


        }
        public class AOERevealZone : AbilityAreaEffectLogic
        {
           protected override void OnRound(MechanicsContext context, AreaEffectEntityData area)
            {
                FogOfWarController.AddRevealer(area.View.transform);
                foreach (AreaEffectEntityData AreaEntity in Game.Instance.State.AreaEffects)
                {
                    if (AreaEntity != area && AreaEntity.Blueprint== AreaBlueprint)
                    {
                        if (AreaEntity.Context.MaybeCaster == area.Context.MaybeCaster)
                            AreaEntity.FadeOutViewAndDestroy();
                    }
                }

            }


            public BlueprintAbilityAreaEffect AreaBlueprint;

        }

        public class NoApproach : OwnedGameLogicComponent<UnitDescriptor>
        {
            NoApproach()
            {
            }
            

        }

        public static void CreateImprovedTrueStrike()
        {
            string name = "TrueStrikeGreater";
            string display = "True Strike, Greater";
            BlueprintAbility TrueStrike = GetAbility("2c38da66e5a599347ac95b3294acbe00");
            string buffdesc = "Your next {0} will ignore concealement and get an insight bonus of {1} to the attack roll.";
            string vardesc = "The target next {0} will ignore concealement and get an insight bonus of {1} to the attack roll.";
            string abilitydesc = "This spell functions as True Strike except that you can cast it on another creature and can make the spell affect up to 4 attacks, dividing the insight bonus between them.\n True Strike:  " + TrueStrike.Description;

            BlueprintBuff[] buffs = new BlueprintBuff[4];
            for (int i = 0; i < 4; i++)
            {
                string attack = i == 0 ? "attack" : (i + 1) + " attacks";
                string descformat = string.Format(buffdesc, attack, (21) / (i + 1));
                buffs[i] = spellbuff(name + "Var" + i, display + "(" + (i + 1) + ")", descformat, TrueStrike.Icon, CommonDivination, components: new BlueprintComponent[] {
                    Helpers.CreateSpellComponent(SpellSchool.Divination),
                    Helpers.Create<IgnoreConcealment>(),
                    Helpers.Create<AddGenericStatBonus>(b => {
                        b.Stat = StatType.AdditionalAttackBonus;
                        b.Value = (21)/(i+1);
                        b.Descriptor = ModifierDescriptor.Insight;
                    }),
                    Helpers.Create<RemoveBuffAfterXAttack>( a => a.max = (i+1))

                });

            }
            BlueprintAbility[] variant = new BlueprintAbility[4];
            for (int i = 0; i < 4; i++)
            {

                string attack = i == 0 ? "attack" : (i + 1) + " attacks";
                string descformat = string.Format(vardesc, attack, (21) / (i + 1));
                variant[i] = Spell(name, display + "(" + (i + 1) + ")", descformat, TrueStrike.Icon, AbilityRange.Close, var: "Variant" + i, duration: "1 round or until discharged", components: new BlueprintComponent[] {
                    Helpers.CreateSpellComponent(SpellSchool.Divination),
                    Helpers.CreateRunActions(Helpers.CreateApplyBuff(buffs[i], Helpers.CreateContextDuration(1), true))
                });
                variant[i].SetAssetId(CommonDivination.AssetId);
                variant[i].SetCantarget(allies: true, self: true);
                variant[i].SetEffectOn(AbilityEffectOnUnit.Helpful, AbilityEffectOnUnit.None);
                variant[i].AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach;
            }

            BlueprintAbility spell = Spell(name, display, abilitydesc, TrueStrike.Icon, AbilityRange.Close, CommandType.Standard, duration: "1 round or until discharged", components: new BlueprintComponent[] {
                Helpers.CreateSpellComponent(SpellSchool.Divination)
            });
            spell.SetAssetId(CommonDivination.AssetId);
            spell.SetCantarget(allies: true, self: true);
            spell.SetEffectOn(AbilityEffectOnUnit.Helpful, AbilityEffectOnUnit.None);
            spell.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach;
            spell.AddComponent(spell.CreateAbilityVariants(variant));
            spell.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Directional;
            //Level alchemist 6, magus 6, sorcerer/wizard 5;
            spell.AddToSpellList(Helpers.wizardSpellList, 5);
            spell.AddToSpellList(Helpers.alchemistSpellList, 6);
            spell.AddToSpellList(Helpers.magusSpellList, 6);

            Helpers.AddSpell(spell);



        }

        public class RemoveBuffAfterXAttack : RuleInitiatorLogicComponent<RuleAttackRoll>
        {
            public override void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
            }

            public override void OnEventDidTrigger(RuleAttackRoll evt)
            {
                Count++;
                if (Count >= max)
                    base.Owner.RemoveFact(base.Fact);
            }

            private int Count = 0;
            public int max = 1;

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

            var bufflesser = Helpers.CreateBuff(name + "LesserBuff", "Lesser Counterspell", "The next spell this creature will cast has a chance to be blocked and wasted.", Helpers.getGuid(name + "LesserBuff"), icon, fx, fx,
                Helpers.Create<ArcaneDiscoveries.CounterSummonSpell>(p =>
                {
                    p.bonus = 0;
                    p.type = ArcaneDiscoveries.CounterSummonSpell.source.spell;

                })
                );
            var buff = Helpers.CreateBuff(name + "Buff", "Counterspell", "The next spell this creature will cast has a chance to be blocked and wasted.", Helpers.getGuid(name + "Buff"), icon2, fx, fx,
                Helpers.Create<ArcaneDiscoveries.CounterSummonSpell>(p =>
                {
                    p.bonus = 2;
                    p.type = ArcaneDiscoveries.CounterSummonSpell.source.spell;

                }));
            var buffgreater = Helpers.CreateBuff(name + "GreaterBuff", "Greater Counterspell", "The next spell this creature will cast has a chance to be blocked and wasted.", Helpers.getGuid(name + "GreaterBuff"), icon3, fx, fx,
                Helpers.Create<ArcaneDiscoveries.CounterSummonSpell>(p =>
                {
                    p.bonus = 5;
                    p.type = ArcaneDiscoveries.CounterSummonSpell.source.spell;

                }));

            var abilitylesser = Helpers.CreateAbility(name + "LesserAbility", "Lesser Counterspell", "This spell allows the caster to see glimpses of the future, immediately learning what spell their target will cast, if any, before the start of their next round.\nOnce the caster learn the nature of this spell, they automatically spend a standard action to prepare a counterspell and start accumulating magical energy to disrupt the future spell.\n" +
                "Then, when the target finally cast their spell, the caster release their counterspell as an immediate action and must make a Caster level check against a DC equal to 11 + the spell level +the target caster level.\nIf the check is successful, the target's spell is blocked and wasted.",
                Helpers.getGuid(name + "LesserAbility"), icon, AbilityType.Spell, CommandType.Standard, AbilityRange.Medium, "1 round", "",
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

            if (hunterlist != null)
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

            public void OnCast(AbilityExecutionContext Context)
            {
                Log.Write("on cast");
                switch (Action)
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

        [HarmonyPatch(typeof(PartyPerceptionController), nameof(PartyPerceptionController.Tick))]
        private static class AugmentDetectionRadiusPacth
        {
            static BlueprintBuff Extended = GetBuff(Guid("UnboundSightTokenBuff"));
            static BlueprintBuff ExtendedG = GetBuff(Guid("UnboundSightGreaterTokenBuff"));

            private static bool Prefix(PartyPerceptionController __instance)
            {
                


                List<StaticEntityData> list = ListPool<StaticEntityData>.Claim();
                AreaPersistentState loadedAreaState = Game.Instance.State.LoadedAreaState;
                loadedAreaState.CollectAllEntities<StaticEntityData>(list);
                foreach (UnitEntityData unit in Game.Instance.Player.ControllableCharacters)
                {
                    
                    if (unit.HasMotionThisTick)
                    {
                        foreach (StaticEntityData Entity in list)
                        {
                            if (Entity.IsInGame && !Entity.IsInFogOfWar)
                            {
                                float num = unit.DistanceTo(Entity.View.transform.position);
                                if (!Entity.IsPerceptionCheckPassed && Entity.View.PerceptionCheckComponent == null)
                                {
                                    Entity.IsPerceptionCheckPassed = true;
                                }
                                else if (Entity.IsPerceptionCheckPassed)
                                {
                                    MapObjectEntityData mapObjectEntityData = Entity as MapObjectEntityData;
                                    if (mapObjectEntityData != null && mapObjectEntityData.View.Interactions.HasItem((i) => i is LootComponent && i.Enabled) &&
                                        !mapObjectEntityData.WasHighlightedOnReveal &&
                                        num < (float)BlueprintRoot.Instance.StandartPerceptionRadius && !Game.Instance.Player.IsInCombat)
                                    {
                                        mapObjectEntityData.View.ForceHighlightOnReveal();
                                    }
                                }
                                else if (Entity.IsPerceptionRollAllowed(unit)  && unit.HasLOS(Entity.View.transform))
                                {
                                    float RadiusExtenion = 0;

                                    if (unit.Descriptor.HasFact(ExtendedG) || unit.Descriptor.HasFact(Extended))
                                        RadiusExtenion = 5f;
                                    if( num < Entity.View.PerceptionCheckComponent.Radius + RadiusExtenion)
                                    { 
                                    Log.Write("Perception Roll| Distance :" + num + "| Perception Radius :" + Entity.View.PerceptionCheckComponent.Radius + "| Extension : " + RadiusExtenion);
                                    AccessTools.Method(typeof(PartyPerceptionController), "RollPerception", new Type[] { typeof(UnitEntityData), typeof(StaticEntityData) }).Invoke(null, new object[] { unit, Entity });
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

        }



        [HarmonyPatch(typeof(UnitUseAbility))]
        [Harmony12.HarmonyPatch("ctor", Harmony12.MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(CommandType), typeof(AbilityData), typeof(TargetWrapper) })]
        private static class IgnoreAppoachPatch
        {
            static void Postfix(UnitUseAbility __instance, AbilityData spell)
            {
                if (spell.Blueprint.GetComponent<NoApproach>())
                    __instance.NeedLoS = false;

            }

        }
    }
}
