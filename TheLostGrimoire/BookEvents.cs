﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.DialogSystem;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.DialogSystem.Blueprints;
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

namespace thelostgrimoire
{
    class BookEvents
    {

        static LibraryScriptableObject library => Main.library;
        static Sprite GetIcon(string id) => Helpers.GetIcon(id);
        static BlueprintAbility GetAbility(string id) => library.Get<BlueprintAbility>(id);
        static BlueprintFeature GetFeat(string id) => library.Get<BlueprintFeature>(id);
        static BlueprintBuff GetBuff(string id) => library.Get<BlueprintBuff>(id);
        
        static string Guid(string name) => Helpers.getGuid(name);

       

      
        static AbilityEffectRunAction RunAction(params GameAction[] actions) => Helpers.CreateRunActions(actions);


        internal static void Load()
        {
            // Load  spell
            
            //needed patch
            
        }
        public static BlueprintCheck CreateCheck(string name, int num, StatType type, int dc, BlueprintCueBase success, BlueprintCueBase fail, bool hidden = false,  DCModifier[] dCModifier = null, ConditionsChecker condition = null, params BlueprintComponent[] components )
        {
            BlueprintCheck o = Helpers.Create<BlueprintCheck>();
            o.Success = success;
            o.Fail = fail;
            o.Hidden = hidden;
            o.DC = dc;
            o.DCModifiers = dCModifier == null ? Array.Empty<DCModifier>() : dCModifier;
            o.Type = type;
            o.Conditions = condition == null ? new ConditionsChecker() : condition;
            o.name = "Check_" + name + num;
            o.SetComponents(components);
            library.AddAsset(o, Guid(o.name));
            return o;
        }
        public static BlueprintDialog CreateBookEvent(string name, CueSelection FirstCue, Condition[] conditions = null, GameAction[] startaction = null, GameAction[] finishaction = null, 
            GameAction[] replaceaction = null, params BlueprintComponent[] components )
        {
            BlueprintDialog o = Helpers.Create<BlueprintDialog>();
            o.FirstCue = FirstCue;
            o.Type = DialogType.Book;
            o.TurnFirstSpeaker = true;
            o.TurnPlayer = true;
            o.name = "BookEventCustom" + name;

            o.Conditions = new ConditionsChecker();
            o.Conditions.Conditions = conditions == null ? Array.Empty<Condition>() : conditions;

            o.StartActions = new ActionList();
            o.StartActions.Actions = startaction == null ? Array.Empty<GameAction>() : startaction;

            o.FinishActions = new ActionList();
            o.FinishActions.Actions = finishaction == null ? Array.Empty<GameAction>() : finishaction;

            o.ReplaceActions = new ActionList();
            o.ReplaceActions.Actions = replaceaction == null ? Array.Empty<GameAction>() : replaceaction;

            o.SetComponents(components);
            library.AddAsset(o, Guid(o.name));

            return o;
        }

        public static BlueprintBookPage CreateBookPage(string name, int num, List<BlueprintCueBase> cues, List<BlueprintAnswerBase> answers, string ImageId, Condition[] conditions = null, string foreimageid = null, GameAction[] 
            onshowaction = null, string title = "", params BlueprintComponent[] components)
        {


            SpriteResourceLink image = new SpriteResourceLink();
            image.AssetId = ImageId;

            SpriteResourceLink foreimage = null;
            if (foreimageid != null)
            {
                foreimage = new SpriteResourceLink();
                foreimage.AssetId = foreimageid;
            }
            
            BlueprintBookPage p = Helpers.Create<BlueprintBookPage>();

            p.name = "BookPage_"+name + num;

            p.ImageLink = image;
            p.ForeImageLink = foreimage;
            p.Answers = answers;
            p.Cues = cues;

            p.Title = Helpers.CreateString(p.name + ".Title", title);
            p.OnShow = new ActionList();
            p.OnShow.Actions = onshowaction == null? Array.Empty<GameAction>() : onshowaction;

            p.Conditions = new ConditionsChecker();
            p.Conditions.Conditions = conditions == null ? Array.Empty<Condition>() : conditions;

            library.AddAsset(p, Guid(p.name));

            return p;
        }

        public static BlueprintAnswer CreateAnswer(string name, int num, string text, Condition[] showconditions= null, Condition[] selectconditions = null,  CheckData[] FakeCheck = null, ActionList onselect = null, CharacterSelection charselection= null, AlignmentShift shift = null, bool showonce = true, bool showoncecurrentdialog = true, bool movecamera = true, bool checkdistance = true,
            bool addtohistory = true, params BlueprintComponent[] components)
        {
            BlueprintAnswer o = Helpers.Create<BlueprintAnswer>();
            o.name = "Answer_"+name + num;
            o.AlignmentShift = shift == null ? CreateAlignmentShift("", AlignmentShiftDirection.TrueNeutral) : shift;

            o.ShowConditions = new ConditionsChecker();
            o.ShowConditions.Conditions = showconditions == null? Array.Empty<Condition>(): showconditions;


            o.FakeChecks = FakeCheck == null ? Array.Empty<CheckData>() : FakeCheck;

            o.SelectConditions = new ConditionsChecker();
            o.SelectConditions.Conditions = selectconditions == null ? Array.Empty<Condition>() : selectconditions;

            o.AddToHistory = addtohistory;

            o.ShowCheck = new ShowCheck();

            o.CharacterSelection = charselection == null? new CharacterSelection(): charselection;

            o.OnSelect = onselect == null ? new ActionList() : onselect;


            o.ShowOnce = showonce;
            o.ShowOnceCurrentDialog = showoncecurrentdialog;

            o.SetComponents(components);

            o.Text = Helpers.CreateString(o.name + ".Text", text);
            library.AddAsset(o, Guid(o.name));

            return o;
        }
        public static void AnswerNextCue(params (BlueprintAnswer answer, CueSelection cueSelection)[] Answernextcue)
        {
            foreach ((BlueprintAnswer answer, CueSelection selection) item in Answernextcue)
            {
                item.answer.NextCue = item.selection;
            }
        }
        public static void ParentingAnswer(params (BlueprintAnswerBase Answerchild, BlueprintScriptableObject parent)[] AnswerParenting)
        {
            foreach ((BlueprintAnswerBase Answerchild, BlueprintScriptableObject parent) item in AnswerParenting)
            {
                item.Answerchild.ParentAsset = item.parent;
            }
        }
        public static void ParentingCue(params (BlueprintCueBase Cuechild, BlueprintScriptableObject parent)[] CueBookParenting)
        {
            foreach ((BlueprintCueBase Cuechild, BlueprintScriptableObject parent) item in CueBookParenting)
            {
                item.Cuechild.ParentAsset = item.parent;
            }

        }
            
       
        
        public static CueSelection CreateCueSelection(Strategy strategy = Strategy.First, params BlueprintCueBase[] cues)
        {
            CueSelection s = new CueSelection();
            s.Cues.AddRange(cues);
            s.Strategy = strategy;
            return s; 
        }
        public static BlueprintCue CreateCue(string name, int num, string text, ConditionsChecker conditions = null, AlignmentShift shift = null, bool showonce = true, bool showoncecurrentdialog = true, bool movecamera = true, bool checkdistance = true, 
            bool turnspeaker = true,  params BlueprintComponent[] components )
        {
            BlueprintCue o = Helpers.Create<BlueprintCue>();

            o.name = "Cue_"+name+num ;
            o.AlignmentShift = shift == null? CreateAlignmentShift("", AlignmentShiftDirection.TrueNeutral) : shift;
            o.OnShow = new ActionList();
            o.OnStop = new ActionList();
            o.Continue = new CueSelection();

            o.ShowOnce = showonce;
            o.ShowOnceCurrentDialog = showoncecurrentdialog;

            o.Speaker = new DialogSpeaker();
            o.Speaker.MoveCamera = movecamera;
            o.Speaker.CheckDistance = checkdistance;

            o.TurnSpeaker = turnspeaker;

            o.Conditions = conditions == null ? new ConditionsChecker() : conditions;

            o.SetComponents(components);

            o.Text = Helpers.CreateString(o.name + ".Text", text);
            library.AddAsset(o, Guid(o.name));

            return o;
        }
        public static AlignmentShift CreateAlignmentShift(string description, AlignmentShiftDirection direction)
        {
            var AlignmentShift = new AlignmentShift();
            AlignmentShift.Description = Helpers.CreateString(AlignmentShift + ".Description", description);
            AlignmentShift.Direction = direction;

            return AlignmentShift;

        }

    }
}
