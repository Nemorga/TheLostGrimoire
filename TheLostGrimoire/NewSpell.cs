

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
using static Kingmaker.View.MapObjects.TrapObjectView;
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
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
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
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.Inspect;
using Kingmaker.Controllers.Dialog;

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
            Main.ApplyPatch(typeof(GiveNoXPForSPellDialogue), "No Xp exploit");
        }

     
        public static void CreateContacPLane()
        {
            Sprite icon = GetIcon("fafd77c6bfa85c04ba31fdc1c962c914");
            string name = "Contact";
            string display = "Contact Other Plane";
            string desc = "You gained special insight on a chosen topic granting you a bonus on the revelant skill.";
            string spelldesc = "You send your mind to another plane of existence in order to receive advice and information from powers there. You can ask one question, the powers reply in a language you understand and your question is answered truthfully.\nContact with minds so powerful and alien to yours incurs the possibility that your will may not withstand it your brain may be overwhelmed, the higher the being asked the greater the possibility, but also the greater benefit from the answer.Therefore after receiving your answer you must make an Intelligence check(DC 10, 15 or 20 depending of the being power). A failure mean that your mind need time to readjust and you lose 10 minutes of time, also if you contacted a being other than the lesser one, you will be fatigued or exhausted for the most powerful.\nYou can ask about this following topics: Each of the knowledge and lore skill(granting a bonus of + 4, +7 or + 10, depending of the being power), the enemies, trap or hidden object in your area(the more powerful the being the more information on those topic will be revealed).";
            BlueprintBuff fatigue = GetBuff("e6f2fc5d73d88064583cb828801212f4");
            BlueprintBuff exhaustion = GetBuff("46d1b9cc3d0fd36469a471b047d773a2");

            BlueprintBuff easybuffarcane = spellbuff(name+"EasyArcane", "Knowledge from another dimension: Arcane", desc, icon, null, null, StackingType.Replace, BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath,
                Helpers.CreateAddStatBonus(StatType.SkillKnowledgeArcana, 4, ModifierDescriptor.Insight)
                );
            BlueprintBuff easybuffworld = spellbuff(name + "EasyWorld", "Knowledge from another dimension: World", desc, icon, null, null, StackingType.Replace, BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath,
                Helpers.CreateAddStatBonus(StatType.SkillKnowledgeWorld, 4, ModifierDescriptor.Insight)
                );
            BlueprintBuff easybuffreligion = spellbuff(name + "EasyReligion", "Knowledge from another dimension: Religion", desc, icon, null, null, StackingType.Replace, BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath,
                Helpers.CreateAddStatBonus(StatType.SkillLoreReligion, 4, ModifierDescriptor.Insight)
                );
            BlueprintBuff easybuffnature = spellbuff(name + "EasyNature", "Knowledge from another dimension: Nature", desc, icon, null, null, StackingType.Replace, BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath,
                Helpers.CreateAddStatBonus(StatType.SkillLoreNature, 4, ModifierDescriptor.Insight)
                );
            BlueprintBuff Averagebuffarcane = spellbuff(name + "AverageArcane", "Knowledge from another dimension: Arcane", desc, icon, null, null, StackingType.Replace, BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath,
                Helpers.CreateAddStatBonus(StatType.SkillKnowledgeArcana, 7, ModifierDescriptor.Insight)
                );
            BlueprintBuff Averagebuffworld = spellbuff(name + "AverageWorld", "Knowledge from another dimension: World", desc, icon, null, null, StackingType.Replace, BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath,
                Helpers.CreateAddStatBonus(StatType.SkillKnowledgeWorld, 7, ModifierDescriptor.Insight)
                );
            BlueprintBuff Averagebuffreligion = spellbuff(name + "AverageReligion", "Knowledge from another dimension: Religion", desc, icon, null, null, StackingType.Replace, BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath,
                Helpers.CreateAddStatBonus(StatType.SkillLoreReligion, 7, ModifierDescriptor.Insight)
                );
            BlueprintBuff Averagebuffnature = spellbuff(name + "AverageNature", "Knowledge from another dimension: Nature", desc, icon, null, null, StackingType.Replace, BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath,
                Helpers.CreateAddStatBonus(StatType.SkillLoreNature, 7, ModifierDescriptor.Insight)
                );

            BlueprintBuff hardbuffarcane = spellbuff(name + "HardArcane", "Knowledge from another dimension: Arcane", desc, icon, null, null, StackingType.Replace, BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath,
                Helpers.CreateAddStatBonus(StatType.SkillKnowledgeArcana, 10, ModifierDescriptor.Insight)
                );
            BlueprintBuff hardbuffworld = spellbuff(name + "HardWorld", "Knowledge from another dimension: World", desc, icon, null, null, StackingType.Replace, BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath,
                Helpers.CreateAddStatBonus(StatType.SkillKnowledgeWorld, 10, ModifierDescriptor.Insight)
                );
            BlueprintBuff hardbuffreligion = spellbuff(name + "HardReligion", "Knowledge from another dimension: Religion", desc, icon, null, null, StackingType.Replace, BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath,
                Helpers.CreateAddStatBonus(StatType.SkillLoreReligion, 10, ModifierDescriptor.Insight)
                );
            BlueprintBuff hardbuffnature = spellbuff(name + "HardNature", "Knowledge from another dimension: Nature", desc, icon, null, null, StackingType.Replace, BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath,
                Helpers.CreateAddStatBonus(StatType.SkillLoreNature, 10, ModifierDescriptor.Insight)
                );


            BlueprintBuff TokenIscaster = tokenbuff(name);
            TokenIscaster.AddComponent(Divination);

            #region : BookEvent
            //The Answer of the first pages
            var AnswerContinue = BookEvents.CreateAnswer("ContactPlane", 0, "Continue...");
            var AnswerBye = BookEvents.CreateAnswer("ContactPlane", 1, "But… No, you made a mistake : the power, the danger. You must leave at once, before anything happens to you.");
            var AnswerBye2 = BookEvents.CreateAnswer("ContactPlane", 2, "But… No, you made a mistake : the power, the danger. You must leave at once, before anything happens to you.");
            
            var AnswerEasy = BookEvents.CreateAnswer("ContactPlane", 3, "I will go to the strange forest, where I feel some spirit of lesser power may have answer.");
            var AnswerAverage = BookEvents.CreateAnswer("ContactPlane", 4, "I will go to the city, where some higher spirits may know more.");
            var AnswerHard = BookEvents.CreateAnswer("ContactPlane", 5, "I will go to the heart of this plane, where I feel the greatest and wisest spirit reside.");

            //Char selection rule => keep the same, needed so that only the caster do check inside the book event
            CharacterSelection charselect = new CharacterSelection();
            charselect.SelectionType = CharacterSelection.Type.Keep;


            //Answers used when  choosing the easy option
            List<BlueprintAnswerBase> easyanswer = new List<BlueprintAnswerBase>();
            var AnswerEasyArcane = BookEvents.CreateAnswer("ContactPlane", 6, "Give me knowledge of the planes, magic and spells! [Bonus to Knowledge: Arcane until next rest]", onselect: Helpers.CreateActionList(Helpers.Create<AttachBuff>(b => { b.Buff = easybuffarcane; b.Target = Helpers.Create<DialogInitiator>(); })), charselection: charselect);
            easyanswer.Add(AnswerEasyArcane);
            var AnswerEasyWorld = BookEvents.CreateAnswer("ContactPlane", 7, "Give me knowledge of History, of the folks and their tradition! [Bonus to Knowledge: World until next rest]", onselect: Helpers.CreateActionList(Helpers.Create<AttachBuff>(b => { b.Buff = easybuffworld; b.Target = Helpers.Create<DialogInitiator>(); })), charselection: charselect);
            easyanswer.Add(AnswerEasyWorld);
            var AnswerEasyNature = BookEvents.CreateAnswer("ContactPlane", 8, "I want to know about the natural world, about animals and plants! [Bonus to Lore: Nature until next rest]", onselect: Helpers.CreateActionList(Helpers.Create<AttachBuff>(b => { b.Buff = easybuffnature; b.Target = Helpers.Create<DialogInitiator>(); })), charselection: charselect);
            easyanswer.Add(AnswerEasyNature);
            var AnswerEasyReligion = BookEvents.CreateAnswer("ContactPlane", 9, "I want to know the gods, their teaching and the things they despise! [Bonus to Lore: Religion until next rest]", onselect: Helpers.CreateActionList(Helpers.Create<AttachBuff>(b => { b.Buff = easybuffreligion; b.Target = Helpers.Create<DialogInitiator>(); })), charselection: charselect);
            easyanswer.Add(AnswerEasyReligion);
            var AnswerEasyEnemy = BookEvents.CreateAnswer("ContactPlane", 10, "Tell me of my foes! [Look at your battle log for enemy stats]", onselect: Helpers.CreateActionList(Helpers.Create<GetEnemyInfo>(g => { g.Actor = Helpers.Create<DialogInitiator>(); g.BaseResult = 10; g.Token = TokenIscaster;  })), charselection: charselect);
            easyanswer.Add(AnswerEasyEnemy);
            var AnswerEasyTrap = BookEvents.CreateAnswer("ContactPlane", 11, "Are there traps ahead of me? [Try to reveal one trap ]", onselect: Helpers.CreateActionList(Helpers.Create<GetTrapInfo>(g => { g.Actor = Helpers.Create<DialogInitiator>(); g.BaseResult = 10; g.Token = TokenIscaster; g.limit = 1; })), charselection: charselect);
            easyanswer.Add(AnswerEasyTrap);
            var AnswerEasyHidden = BookEvents.CreateAnswer("ContactPlane", 12, "Are there hidden treasure where I am? [Try to reveal 1 hidden objects or doors]", onselect: Helpers.CreateActionList(Helpers.Create<GetSecretInfo>(g => { g.Actor = Helpers.Create<DialogInitiator>(); g.BaseResult = 10; g.Token = TokenIscaster; g.limit = 1; })), charselection: charselect);
            easyanswer.Add(AnswerEasyHidden);

            Condition[] Donotshow = new Condition[] { };
            Condition[] Doshow = new Condition[] { };
            foreach(BlueprintAnswer answer in easyanswer )
            {
                Donotshow = Donotshow.AddToArray(Helpers.Create<AnswerSelected>(c => { c.Answer = answer; c.CurrentDialog = true; c.Not = true; }));
                Doshow = Doshow.AddToArray(Helpers.Create<AnswerSelected>(c => { c.Answer = answer; c.CurrentDialog = true; }));
            }
            foreach (BlueprintAnswer answer in easyanswer)
            {
                answer.ShowConditions.Conditions = Donotshow;
                answer.ShowConditions.Operation = Operation.And;
            }

            
            var AnswerBye3 = BookEvents.CreateAnswer("ContactPlane", 13, "Leave", Doshow);
            AnswerBye3.ShowConditions.Operation = Operation.Or;
            easyanswer.Add(AnswerBye3);



            //Answer for the average option
            List<BlueprintAnswerBase> averageanswer = new List<BlueprintAnswerBase>();
            var AnswerAverageArcane = BookEvents.CreateAnswer("ContactPlane", 16, "Give me knowledge of the planes, magic and spells! [Bonus to Knowledge: Arcane until next rest]", onselect: Helpers.CreateActionList(Helpers.Create<AttachBuff>(b => { b.Buff = Averagebuffarcane; b.Target = Helpers.Create<DialogInitiator>(); })), charselection: charselect);
            averageanswer.Add(AnswerAverageArcane);
            var AnswerAverageWorld = BookEvents.CreateAnswer("ContactPlane", 17, "Give me knowledge of History, of the folks and their tradition! [Bonus to Knowledge: World until next rest]", onselect: Helpers.CreateActionList(Helpers.Create<AttachBuff>(b => { b.Buff = Averagebuffworld; b.Target = Helpers.Create<DialogInitiator>(); })), charselection: charselect);
            averageanswer.Add(AnswerAverageWorld);
            var AnswerAverageNature = BookEvents.CreateAnswer("ContactPlane", 18, "I want to know about the natural world, about animals and plants! [Bonus to Lore: Nature until next rest]", onselect: Helpers.CreateActionList(Helpers.Create<AttachBuff>(b => { b.Buff = Averagebuffnature; b.Target = Helpers.Create<DialogInitiator>(); })), charselection: charselect);
            averageanswer.Add(AnswerAverageNature);
            var AnswerAverageReligion = BookEvents.CreateAnswer("ContactPlane", 19, "I want to know the gods, their teaching and the things they despise! [Bonus to Lore: Religion until next rest]", onselect: Helpers.CreateActionList(Helpers.Create<AttachBuff>(b => { b.Buff = Averagebuffreligion; b.Target = Helpers.Create<DialogInitiator>(); })), charselection: charselect);
            averageanswer.Add(AnswerAverageReligion);
            var AnswerAverageEnemy = BookEvents.CreateAnswer("ContactPlane", 20, "Tell me of my foes! [Look at your battle log for enemy stats]", onselect: Helpers.CreateActionList(Helpers.Create<GetEnemyInfo>(g => { g.Actor = Helpers.Create<DialogInitiator>(); g.BaseResult = 15; g.Token = TokenIscaster; })), charselection: charselect);
            averageanswer.Add(AnswerAverageEnemy);
            var AnswerAverageTrap = BookEvents.CreateAnswer("ContactPlane", 21, "Are there traps ahead of me? [Try to reveal 5 traps]", onselect: Helpers.CreateActionList(Helpers.Create<GetTrapInfo>(g => { g.Actor = Helpers.Create<DialogInitiator>(); g.BaseResult = 15; g.Token = TokenIscaster; g.limit = 3; })), charselection: charselect);
            averageanswer.Add(AnswerAverageTrap);
            var AnswerAverageHidden = BookEvents.CreateAnswer("ContactPlane", 22, "Are there hidden treasure where I am? [Try to reveal 2 hidden objects or doors]", onselect: Helpers.CreateActionList(Helpers.Create<GetSecretInfo>(g => { g.Actor = Helpers.Create<DialogInitiator>(); g.BaseResult = 15; g.Token = TokenIscaster; g.limit = 2; })), charselection: charselect);
            averageanswer.Add(AnswerAverageHidden);

            Condition[] DonotshowAverage = new Condition[] { };
            Condition[] DoshowAverage = new Condition[] { };
            foreach (BlueprintAnswer answer in averageanswer)
            {
                DonotshowAverage = DonotshowAverage.AddToArray(Helpers.Create<AnswerSelected>(c => { c.Answer = answer; c.CurrentDialog = true; c.Not = true; }));
                DoshowAverage = DoshowAverage.AddToArray(Helpers.Create<AnswerSelected>(c => { c.Answer = answer; c.CurrentDialog = true; }));
            }
            foreach (BlueprintAnswer answer in averageanswer)
            {
                answer.ShowConditions.Conditions = DonotshowAverage;
                answer.ShowConditions.Operation = Operation.And;
            }

            
            var AnswerBye4 = BookEvents.CreateAnswer("ContactPlane", 23, "Leave", DoshowAverage);
            AnswerBye4.ShowConditions.Operation = Operation.Or;
            averageanswer.Add(AnswerBye4);



            //Answer for the hard option
            List<BlueprintAnswerBase> hardanswer = new List<BlueprintAnswerBase>();
            var AnswerhardArcane = BookEvents.CreateAnswer("ContactPlane", 26, "Give me knowledge of the planes, magic and spells! [Bonus to Knowledge: Arcane until next rest]", onselect: Helpers.CreateActionList(Helpers.Create<AttachBuff>(b => { b.Buff = hardbuffarcane; b.Target = Helpers.Create<DialogInitiator>(); })), charselection: charselect);
            hardanswer.Add(AnswerhardArcane);
            var AnswerhardWorld = BookEvents.CreateAnswer("ContactPlane", 27, "Give me knowledge of History, of the folks and their tradition! [Bonus to Knowledge: World until next rest]", onselect: Helpers.CreateActionList(Helpers.Create<AttachBuff>(b => { b.Buff = hardbuffworld; b.Target = Helpers.Create<DialogInitiator>(); })), charselection: charselect);
            hardanswer.Add(AnswerhardWorld);
            var AnswerhardNature = BookEvents.CreateAnswer("ContactPlane", 28, "I want to know about the natural world, about animals and plants! [Bonus to Lore: Nature until next rest]", onselect: Helpers.CreateActionList(Helpers.Create<AttachBuff>(b => { b.Buff = hardbuffnature; b.Target = Helpers.Create<DialogInitiator>(); })), charselection: charselect);
            hardanswer.Add(AnswerhardNature);
            var AnswerhardReligion = BookEvents.CreateAnswer("ContactPlane", 29, "I want to know the gods, their teaching and the things they despise! [Bonus to Lore: Religion until next rest]", onselect: Helpers.CreateActionList(Helpers.Create<AttachBuff>(b => { b.Buff = hardbuffreligion; b.Target = Helpers.Create<DialogInitiator>(); })), charselection: charselect);
            hardanswer.Add(AnswerhardReligion);
            var AnswerhardEnemy = BookEvents.CreateAnswer("ContactPlane", 30, "Tell me of my foes! [Look at your battle log for enemy stats]", onselect: Helpers.CreateActionList(Helpers.Create<GetEnemyInfo>(g => { g.Actor = Helpers.Create<DialogInitiator>(); g.BaseResult = 20; g.Token = TokenIscaster; })), charselection: charselect);
            hardanswer.Add(AnswerhardEnemy);
            var AnswerhardTrap = BookEvents.CreateAnswer("ContactPlane", 31, "Are there traps ahead of me? [Try to reveal 5 traps]", onselect: Helpers.CreateActionList(Helpers.Create<GetTrapInfo>(g => { g.Actor = Helpers.Create<DialogInitiator>(); g.BaseResult = 20; g.Token = TokenIscaster; g.limit = 5; })), charselection: charselect);
            hardanswer.Add(AnswerhardTrap);
            var AnswerhardHidden = BookEvents.CreateAnswer("ContactPlane", 32, "Are there hidden treasure where I am? [Try to reveal 2 hidden objects or doors]", onselect: Helpers.CreateActionList(Helpers.Create<GetSecretInfo>(g => { g.Actor = Helpers.Create<DialogInitiator>(); g.BaseResult = 20; g.Token = TokenIscaster; g.limit = 3; })), charselection: charselect);
            hardanswer.Add(AnswerhardHidden);

            Condition[] Donotshowhard = new Condition[] { };
            Condition[] Doshowhard = new Condition[] { };
            foreach (BlueprintAnswer answer in hardanswer)
            {
                Donotshowhard = Donotshowhard.AddToArray(Helpers.Create<AnswerSelected>(c => { c.Answer = answer; c.CurrentDialog = true; c.Not = true; }));
                Doshowhard = Doshowhard.AddToArray(Helpers.Create<AnswerSelected>(c => { c.Answer = answer; c.CurrentDialog = true; }));
            }
            foreach (BlueprintAnswer answer in hardanswer)
            {
                answer.ShowConditions.Conditions = Donotshowhard;
                answer.ShowConditions.Operation = Operation.And;
            }


            var AnswerBye5 = BookEvents.CreateAnswer("ContactPlane", 33, "Leave", Doshowhard);
            AnswerBye5.ShowConditions.Operation = Operation.Or;
            hardanswer.Add(AnswerBye5);




            //The Cue
            //Skip time value, needed for later
            var Cast = new IntConstant();
            Cast.Value = 1;
            var CueFirst = BookEvents.CreateCue("ContactPlane", 0, "As you speak the words of power, you feel your body going numb, your sense of your surroundings fades, your vision blurs. Finally, as your companions disappear from your sight, and as your mind leave your body,  there is only darkness.\nBut, far, far away, in what is otherwise an eerie void : a light, a star, tinkling in the distance. Another plane, your destination. Your mind set course for it, sailing the void as if all the abysses were chasing you and then, you feel it. The power, the knowledge of being greater than yours, greater than anything you encountered.\nAs you come closer and closer, a strange world start to reveal itself inside the light... ");
            var CueArrival = BookEvents.CreateCue("ContactPlane", 1, "Strange lands, strange cities, strange beings walking their trails and streets... like nothing you ever saw.\nAnd again, this feeling that drew you here: the knowledge and power. You’ve yet to ask your question but the weight of possible answer is already overwhelming, straining your mind as if you had spent days at a library without sleeping.\nYou must make a choice, you know it. Where will you seek answer? Who (or what) will you ask for wisdom? And you must be careful, the inhabitants of this plane do not care for your mind or its capability. The more knowledge you seek, the more danger to your mind. ");

            var CueEasy = BookEvents.CreateCue("ContactPlane", 2, "You are surrounded by trees of multiple and strange essence, some familiar, others unknown. Is it a jungle? A forest of pine tree? Something else? \nWhatever the answer, you find out that while you were musing about the tree, many being drew close to you. Animal, spirit and otherwise of nature unknown: they wait beside you as if you were the one in charge. \nSomehow you are. They are waiting for a question to answer. What will you ask?");
            CueEasy.OnShow.Actions = new GameAction[] {Helpers.Create<SetActingUnit>(c => c.unit = Helpers.Create<DialogInitiator>()) };

            var CueAverage = BookEvents.CreateCue("ContactPlane", 3, "Ivory tower and silvery bridge. Crystal castle and golden gate. Glimmering pavement and Shimmering wall of light...\nThis is truly one of the fairest city you’ve ever seen.As full of wonder as it is full of alien inhabitants.\nThey pass and go beside you as if you were not really there. And in a way, you are not. You are a spirit wandering the expanse of the plane. It would take someone(or something) with particular ability or knowledge to see you.\nFortunately, you finally notice one of them. A creature made of living steel, sited beside a fountain spouting liquid diamond. They gesture for you to approach and when you do, speak with a voice of crumbled parchment:\n“Greetings mortal, I gather you cam here seeking wisdom, as many of your kind do… Well, what do you want to know? Ask away, I have time, you don’t, mortal.”\nWhat will be your question ? ");
            CueAverage.OnShow.Actions = new GameAction[] { Helpers.Create<SetActingUnit>(c => c.unit = Helpers.Create<DialogInitiator>()) };

            var CueHard = BookEvents.CreateCue("ContactPlane", 4, "You fly above the city, above its highest spears, only guided by a supernatural sense that this is the way to a creature of tremendous wisdom.\nFinally you reach it: a building, flying among the clouds of this world.Without hesitation, you penetrate its gate, fly through its dark iron arch, follow its damp and cloudy corridor and finally you arrive in a small room.It is some kind of a laboratory, and in it the ethereal and shadowy figure of what must be, in this world, an arcane practitioner.\nAs you enter the laboratory, he immediately turns to you:\n“A mortal, here? How dare you! Thousand of your kind come to this world everyday but you, you!You chose to disturb me!”\nIs it really rage in is voice ? Or carefully disguised amusement?\n“Such recklessness, such boldness!It must be rewarded. Yes, indeed! So, mortal, what are you here for, which secret of mine do you want to learn ?”\nAs you start to carefully word a question, you hear the being whispering almost to himself:\n“...and go one with it, you wormy disturbing prick, before I must make you go myself”\nHurriedly, you formulate question: ");
            CueHard.OnShow.Actions = new GameAction[] { Helpers.Create<SetActingUnit>(c => c.unit = Helpers.Create<DialogInitiator>()) };
            
            // The string for the success and failure of the Int Check, they're the same whatever the difficulty
            string End = "There is no answer. No sound, nothing but a flash of light and then...\nOnly silence.\nSilence and pain.\nPain, as your mind is instantly filled with new knowledge, and seems to be about to explode. But the wisdom is here, you can feel it… know it. New ideas, new notions, visions of distant land, patterns of forgotten runes, songs never heard by a living man, payers to ancient god…\nIf only you could harness them, control the influx, keep it from being a spiraling mess, a wave of pure thought and pain.\nAssembling all the powers of your mind, you gather a powerful wall of will, a channel made of ideas. You mastered spells and arcane might : nothing will shatter your mind!\nThe wave of knowledge crash against the dam of your will…";
            string Success = "\nAnd stop there, broken, dispersed and slowly funneled into your memory. The pain subsides gently and you start to really understand what answers you were given. But as you want to thanks whoever answered you, you realize that you are back into your body. ";
            string Fail = "\nBut its force is so mighty that you can not handle it. As the pain keep on rising you feel all your mental barriers being broken one by one, all your well organized thought being disrupted and finally, even your conscience cannot withstand the full power of the answer that was gifted to you. Under this total assault, you lose consciousness, having only the time to hear a mocking laughter.\nYou regain consciousness a few minutes later and, to your relief, all of your mind seems in one piece and the knowledge you sought is here. But you have lost precious time";
            

            //Skip time value, needed for later
            var Minutes = new IntConstant();
            Minutes.Value = 10;

            //Cue to show for the result ofthe easy check
            var CueSuccessEasy = BookEvents.CreateCue("ContactPlane", 5, End + Success);
            var CueFailEasy = BookEvents.CreateCue("ContactPlane", 6, End + Fail+".");
            CueFailEasy.OnShow.Actions = new GameAction[] { Helpers.Create<TimeSkip>(s => s.MinutesToSkip = Minutes) };

            //Cue to show for the result ofthe average check
            var CueSuccessAverage = BookEvents.CreateCue("ContactPlane", 7, End + Success);
            var CueFailAverage = BookEvents.CreateCue("ContactPlane", 8, End + Fail+" and are fatigued.");
            CueFailAverage.OnShow.Actions = new GameAction[] { Helpers.Create<TimeSkip>(s => s.MinutesToSkip = Minutes), Helpers.Create<AttachBuff>(b => { b.Buff = fatigue; b.Target = Helpers.Create<DialogInitiator>(); }) };

            //Cue to show for the result ofthe average check
            var CueSuccessHard = BookEvents.CreateCue("ContactPlane", 9, End + Success);
            var CueFailHard = BookEvents.CreateCue("ContactPlane", 10, End + Fail + " and are exhausted.");
            CueFailHard.OnShow.Actions = new GameAction[] { Helpers.Create<TimeSkip>(s => s.MinutesToSkip = Minutes), Helpers.Create<AttachBuff>(b => { b.Buff = exhaustion; b.Target = Helpers.Create<DialogInitiator>(); }) };


            var BookPage = BookEvents.CreateBookPage("ContactPlane", 0, new List<BlueprintCueBase> { CueFirst }, new List<BlueprintAnswerBase> { AnswerContinue, AnswerBye }, "90e75a92e9fbcc340b2869a5347c946b", title:"Contact Other Plane");
            var BookPage1 = BookEvents.CreateBookPage("ContactPlane", 1, new List<BlueprintCueBase> { CueArrival }, new List<BlueprintAnswerBase> {AnswerEasy,AnswerAverage,AnswerHard, AnswerBye2 }, "acff890219eb26140b40a312b3e7c047");

            var BookPageEasy = BookEvents.CreateBookPage("ContactPlane", 2, new List<BlueprintCueBase> { CueEasy, CueSuccessEasy, CueFailEasy }, easyanswer, "d32a596156bd4db41b1ec6e19a09356f");
            var BookPageAverage = BookEvents.CreateBookPage("ContactPlane", 3, new List<BlueprintCueBase> { CueAverage, CueSuccessAverage, CueFailAverage }, averageanswer, "7e01dd21e02de5d408c86b16e689a817");
            var BookPageHard = BookEvents.CreateBookPage("ContactPlane", 4, new List<BlueprintCueBase> { CueHard, CueFailHard, CueSuccessHard }, hardanswer, "7372713ea8ec8a243813a3278a1ba57c");


            //Check to do for easy option and Cue parameter
            var CheckEasy = BookEvents.CreateCheck("ContactPlane", 0, StatType.Intelligence, 10, null, null,true, components: new BlueprintComponent[] {Helpers.Create<NoXp>()});
            CueSuccessEasy.Conditions.Conditions = new Condition[] { Helpers.Create<CheckPassed>(c => c.Check = CheckEasy) };
            CueFailEasy.Conditions.Conditions = new Condition[] { Helpers.Create<CheckFailed>(c => c.Check = CheckEasy) };
            CheckEasy.Success = BookPageEasy;
            CheckEasy.Fail = BookPageEasy;


            //Check to do for average option and Cue parameter
            var CheckAverage = BookEvents.CreateCheck("ContactPlane", 1, StatType.Intelligence, 15, null, null,true, components: new BlueprintComponent[] { Helpers.Create<NoXp>() });
            CueSuccessAverage.Conditions.Conditions = new Condition[] { Helpers.Create<CheckPassed>(c => c.Check = CheckAverage) };
            CueFailAverage.Conditions.Conditions = new Condition[] { Helpers.Create<CheckFailed>(c => c.Check = CheckAverage) };
            CheckAverage.Success = BookPageAverage;
            CheckAverage.Fail = BookPageAverage;

            //Check to do for average option and Cue parameter
            var CheckHard = BookEvents.CreateCheck("ContactPlane", 2, StatType.Intelligence, 20, null, null,true, components: new BlueprintComponent[] { Helpers.Create<NoXp>() });
            CueSuccessHard.Conditions.Conditions = new Condition[] { Helpers.Create<CheckPassed>(c => c.Check = CheckHard) };
            CueFailHard.Conditions.Conditions = new Condition[] { Helpers.Create<CheckFailed>(c => c.Check = CheckHard) };
            CheckHard.Success = BookPageHard;
            CheckHard.Fail = BookPageHard;


            var Dialog = BookEvents.CreateBookEvent("ContactPlane", BookEvents.CreateCueSelection(Strategy.First,BookPage));


            BookEvents.AnswerNextCue((AnswerContinue, BookEvents.CreateCueSelection(Strategy.First, BookPage1)),
                (AnswerBye, new CueSelection()),
                (AnswerBye2, new CueSelection()),
                (AnswerBye3, new CueSelection()),
                (AnswerEasy, BookEvents.CreateCueSelection(Strategy.First, BookPageEasy)),
                (AnswerEasyArcane, BookEvents.CreateCueSelection(Strategy.First, CheckEasy)),
                (AnswerEasyWorld, BookEvents.CreateCueSelection(Strategy.First, CheckEasy)),
                (AnswerEasyNature, BookEvents.CreateCueSelection(Strategy.First, CheckEasy)),
                (AnswerEasyReligion, BookEvents.CreateCueSelection(Strategy.First, CheckEasy)),
                (AnswerEasyEnemy, BookEvents.CreateCueSelection(Strategy.First, CheckEasy)),
                (AnswerEasyTrap, BookEvents.CreateCueSelection(Strategy.First, CheckEasy)),
                (AnswerEasyHidden, BookEvents.CreateCueSelection(Strategy.First, CheckEasy)),

                (AnswerBye4, new CueSelection()),
                (AnswerAverage, BookEvents.CreateCueSelection(Strategy.First, BookPageAverage)),
                (AnswerAverageArcane, BookEvents.CreateCueSelection(Strategy.First, CheckAverage)),
                (AnswerAverageWorld, BookEvents.CreateCueSelection(Strategy.First, CheckAverage)),
                (AnswerAverageNature, BookEvents.CreateCueSelection(Strategy.First, CheckAverage)),
                (AnswerAverageReligion, BookEvents.CreateCueSelection(Strategy.First, CheckAverage)),
                (AnswerAverageEnemy, BookEvents.CreateCueSelection(Strategy.First, CheckAverage)),
                (AnswerAverageTrap, BookEvents.CreateCueSelection(Strategy.First, CheckAverage)),
                (AnswerAverageHidden, BookEvents.CreateCueSelection(Strategy.First, CheckAverage)),

                (AnswerBye5, new CueSelection()),
                (AnswerHard, BookEvents.CreateCueSelection(Strategy.First, BookPageHard)),
                (AnswerhardArcane, BookEvents.CreateCueSelection(Strategy.First, CheckHard)),
                (AnswerhardWorld, BookEvents.CreateCueSelection(Strategy.First, CheckHard)),
                (AnswerhardNature, BookEvents.CreateCueSelection(Strategy.First, CheckHard)),
                (AnswerhardReligion, BookEvents.CreateCueSelection(Strategy.First, CheckHard)),
                (AnswerhardEnemy, BookEvents.CreateCueSelection(Strategy.First, CheckHard)),
                (AnswerhardTrap, BookEvents.CreateCueSelection(Strategy.First, CheckHard)),
                (AnswerhardHidden, BookEvents.CreateCueSelection(Strategy.First, CheckHard))

                );


            BookEvents.ParentingAnswer(
                (AnswerBye, BookPage), 
                (AnswerContinue, BookPage), 
                (AnswerBye2, BookPage1), 

                (AnswerBye3, BookPageEasy), 
                (AnswerEasy, BookPage1), 
                (AnswerEasyArcane, BookPageEasy),
                (AnswerEasyWorld, BookPageEasy), 
                (AnswerEasyReligion, BookPageEasy), 
                (AnswerEasyNature, BookPageEasy), 
                (AnswerEasyTrap, BookPageEasy), 
                (AnswerEasyHidden, BookPageEasy),
                (AnswerEasyEnemy, BookPageEasy),

                (AnswerBye4, BookPageAverage),
                (AnswerAverage, BookPage1),
                (AnswerAverageArcane, BookPageAverage),
                (AnswerAverageWorld, BookPageAverage),
                (AnswerAverageReligion, BookPageAverage),
                (AnswerAverageNature, BookPageAverage),
                (AnswerAverageTrap, BookPageAverage),
                (AnswerAverageHidden, BookPageAverage),
                (AnswerAverageEnemy, BookPageAverage),

                (AnswerBye5, BookPageHard),
                (AnswerHard, BookPage1),
                (AnswerhardArcane, BookPageHard),
                (AnswerhardWorld, BookPageHard),
                (AnswerhardReligion, BookPageHard),
                (AnswerhardNature, BookPageHard),
                (AnswerhardTrap, BookPageHard),
                (AnswerhardHidden, BookPageHard),
                (AnswerhardEnemy, BookPageHard)

                );

            BookEvents.ParentingCue(
                (BookPage, Dialog),    
                (CueFirst, BookPage),
                (BookPage1, AnswerContinue),
                (CueArrival, BookPage1),
                (BookPageEasy, AnswerEasy),
                (CueEasy, BookPageEasy), 
                (CheckEasy, BookPageEasy),
                (CueFailEasy, CheckEasy), 
                (CueSuccessEasy, CheckEasy),

                (BookPageAverage, AnswerAverage),
                (CueAverage, BookPageAverage),
                (CheckAverage, BookPageAverage),
                (CueFailAverage, CheckAverage),
                (CueSuccessAverage, CheckAverage),

                (BookPageHard, AnswerHard),
                (CueHard, BookPageHard),
                (CheckHard, BookPageHard),
                (CueFailHard, CheckHard),
                (CueSuccessHard, CheckHard)

                );









            #endregion

            var spell = Spell(name, display, spelldesc, icon, AbilityRange.Personal, components: new BlueprintComponent[] {
                RunAction(Helpers.CreateApplyBuff(TokenIscaster, Helpers.CreateContextDuration(1), true, false, true),
                    Helpers.Create<SpellStartDialogue>(d =>{
                    d.Dialogue = Dialog;
                    })),
                Divination,
                Helpers.Create<AbilityCasterHasNoFacts>(f => f.Facts = new BlueprintUnitFact[]{fatigue, exhaustion })
            }) ;
            spell.AvailableMetamagic = Metamagic.Heighten;
            Helpers.AddSpell(spell);
            spell.AddToSpellList(Helpers.wizardSpellList, 5);


        }
        public class NoXp : BlueprintComponent
        { }
        public class GetSecretInfo : GameAction
        {
            public override string GetCaption()
            {
                return "Make a checkon each enemy, Check result = 10+CasterLevel";
            }
            public int GetCl()
            {
                UnitEntityData unit = GetCaster();
                foreach (Buff buff in unit.Descriptor.Buffs)
                {
                    if (buff.Blueprint == Token)
                    {
                        return buff.Context.Params.CasterLevel;
                    }

                }
                return unit.Descriptor.Progression.CharacterLevel;
            }
            public UnitEntityData GetCaster()
            {
                return Actor.GetValue();
            }

            public override void RunAction()
            {
                var caster = GetCaster();
                if (caster == null)
                    return;
                var CL = GetCl();
                int result = BaseResult + CL + caster.Descriptor.Stats.Intelligence.Bonus;
                BlueprintUnit[] handled = new BlueprintUnit[] { };
                int count = 0;
                foreach (MapObjectEntityData Secret in Game.Instance.State.MapObjects
                    .Where(e => !(e is TrapObjectData))
                    .Select(e => e as MapObjectEntityData ))
                {
                    if (count > limit)
                        return;
                    if (Secret.IsRevealed)
                        continue;
                    if (result >= Secret.PerceptionCheckDC && Secret.PerceptionCheckDC > 4)
                    {

                        Secret.IsRevealed = true;
                        Secret.IsPerceptionCheckPassed = true;
                        EventBus.RaiseEvent<IPerceptionHandler>(delegate (IPerceptionHandler h)
                        {
                            h.OnEntityNoticed(Secret, caster);
                        });
                        count++;
                    }
                    Log.Write(Secret.UniqueId+ " " + "DC :" + Secret.PerceptionCheckDC + " Revelead: " + Secret.IsRevealed.ToString() + " Check: " + Secret.IsPerceptionCheckPassed.ToString() + " Count=" + count);



                }


            }

            public BlueprintBuff Token;
            public UnitEvaluator Actor;
            public int BaseResult;
            public int limit;

        }
        public class GetTrapInfo : GameAction
        {
            public override string GetCaption()
            {
                return "Make a checkon each enemy, Check result = 10+CasterLevel";
            }
            public int GetCl()
            {
                UnitEntityData unit = GetCaster();
                foreach (Buff buff in unit.Descriptor.Buffs)
                {
                    if (buff.Blueprint == Token)
                    {
                        return buff.Context.Params.CasterLevel;
                    }

                }
                return unit.Descriptor.Progression.CharacterLevel;
            }
            public UnitEntityData GetCaster()
            {
                return Actor.GetValue();
            }

            public override void RunAction()
            {
                var caster = GetCaster();
                if (caster == null)
                    return;
                var CL = GetCl();
                int result = BaseResult + CL + caster.Descriptor.Stats.Intelligence.Bonus;
                BlueprintUnit[] handled = new BlueprintUnit[] { };
                int count = 0;
                foreach (TrapObjectData trap in Game.Instance.State.MapObjects
                    .Where(e => e is TrapObjectData)
                    .Select(e => e as TrapObjectData))
                {
                    if (count > limit)
                        return;
                    
                    if(result>= trap.PerceptionCheckDC)
                    {
                        
                        trap.IsRevealed = true;
                        trap.IsPerceptionCheckPassed = true;
                        EventBus.RaiseEvent<IPerceptionHandler>(delegate (IPerceptionHandler h)
                        {
                            h.OnEntityNoticed(trap, caster);
                        });
                        count++;
                    }
                    Log.Write(trap.Actor.CharacterName + " " + "DC :" + trap.PerceptionCheckDC + " Revelead: " + trap.IsRevealed.ToString() + " Check: " + trap.IsPerceptionCheckPassed.ToString()+" Count="+count);



                }


            }

            public BlueprintBuff Token;
            public UnitEvaluator Actor;
            public int BaseResult;
            public int limit;

        }


        public class GetEnemyInfo : GameAction
        {
            public override string GetCaption()
            {
                return "Make a checkon each enemy, Check result = 10+CasterLevel";
            }
            public int GetCl()
            {
                UnitEntityData unit = GetCaster();
                foreach (Buff buff in unit.Descriptor.Buffs)
                {
                    if(buff.Blueprint == Token )
                    {
                        return buff.Context.Params.CasterLevel;
                    }
                    
                }
                return unit.Descriptor.Progression.CharacterLevel;
            }
            public UnitEntityData GetCaster()
            {
                return Actor.GetValue();
            }

            public override void RunAction()
            {
                var caster = GetCaster();
                if (caster == null)
                    return;
                var CL = GetCl();
                int result = BaseResult + CL + caster.Descriptor.Stats.Intelligence.Bonus;
                BlueprintUnit[] handled = new BlueprintUnit[] { };
                foreach (UnitEntityData enemy in Game.Instance.State.Units)
                {
                    Log.Write(enemy.CharacterName + ": " + enemy.Blueprint.CR);

                    if (!InspectUnitsHelper.IsInspectAllow(enemy))
                            continue;


                    BlueprintUnit InspectBlueprint = enemy.BlueprintForInspection;
                    InspectUnitsManager.UnitInfo info = Game.Instance.Player.InspectUnitsManager.GetInfo(InspectBlueprint);

                    if (info == null)
                    {
                        info = new InspectUnitsManager.UnitInfo(InspectBlueprint);
                        Traverse traverse = Traverse.Create(Game.Instance.Player.InspectUnitsManager);
                        List<InspectUnitsManager.UnitInfo> unitinfos = (List<InspectUnitsManager.UnitInfo>)traverse.Field("m_UnitInfos").GetValue();
                        unitinfos.Add(info);
                    }
                    if (handled.Contains(info.Blueprint))
                        continue;

                    if (info.KnownPartsCount == 4)
                            continue;


                    int DC = info.DC;
                    info.SetCheck(result, enemy);
                    EventBus.RaiseEvent<IKnowledgeHandler>((Action<IKnowledgeHandler>)(h => h.HandleKnowledgeUpdated(info)));
                    handled = handled.AddToArray(info.Blueprint);
                    
                    
                }
                    
                
            }

            public BlueprintBuff Token;
            public UnitEvaluator Actor;
            public int BaseResult;

        }

        public class SetActingUnit : GameAction
        {
            public override string GetCaption()
            {
                return "Make Unit the Acting Unit";
            }

            public override void RunAction()
            {
                Traverse traverse = Traverse.Create(Game.Instance.DialogController);
                traverse.Property("ActingUnit").SetValue(unit.GetValue());
                Log.Write(Game.Instance.DialogController.ActingUnit.CharacterName);
                
            }

            public UnitEvaluator unit;
        }
        public class SpellStartDialogue : ContextAction
        {
            public override void RunAction()
            {
                
                Game.Instance.DialogController.StartDialogWithoutTarget(Dialogue, speakerName: null, initiator: Context.MaybeCaster);


            }

            public override string GetCaption()
            {
                return string.Format("Start Dialog ({0})", Dialogue.NameSafe());
            }
            public BlueprintDialog Dialogue;
            

        }
        public static void CreateInsectSpies()
        {
            Sprite icon = GetIcon("20b548bf09bb3ea4bafea78dcb4f3db6");
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
            string desc = "You enter a heightened state of awareness that allows you to notice more about your surroundings and recall information effortlessly. You gain a +2 insight bonus on Perception checks, Initiative checks and on all Knowledge checks.";

            BlueprintBuff buff = spellbuff(name, display, desc, icon, CommonDivination, components: new BlueprintComponent[] {
                Helpers.CreateAddStatBonus(StatType.SkillPerception, 2, ModifierDescriptor.Insight),
                Helpers.CreateAddStatBonus(StatType.Initiative, 2, ModifierDescriptor.Insight),
                Helpers.CreateAddStatBonus(StatType.SkillKnowledgeArcana, 2, ModifierDescriptor.Insight),
                Helpers.CreateAddStatBonus(StatType.SkillKnowledgeWorld, 2, ModifierDescriptor.Insight),
                Helpers.CreateAddStatBonus(StatType.SkillLoreNature, 2, ModifierDescriptor.Insight),
                Helpers.CreateAddStatBonus(StatType.SkillLoreReligion, 2, ModifierDescriptor.Insight)
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

            Archetype.LesserCounterSpell = abilitylesser;
            Archetype.CounterSpell = ability;
            Archetype.GreaterCounterSpell = abilitygreater;

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

        [HarmonyPatch(typeof(DialogController), "PlayCheck")]
        private static class GiveNoXPForSPellDialogue
        {
            static BlueprintStatProgression Progression = library.Get<BlueprintStatProgression>("674a4e0eb86cc89478119de258f602a5");
            private static bool Prefix(DialogController __instance, BlueprintCheck check)
            {
                if (check.GetComponent<NoXp>() != null)
                {
                    BlueprintRoot.Instance.Progression.DCToCRTable = null;

                }
                else
                    BlueprintRoot.Instance.Progression.DCToCRTable = Progression;


                return true;
            }
            private static void Postifx(DialogController __instance, BlueprintCheck check)
            {
                if(BlueprintRoot.Instance.Progression.DCToCRTable == null)
                    BlueprintRoot.Instance.Progression.DCToCRTable = Progression;
            }


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
