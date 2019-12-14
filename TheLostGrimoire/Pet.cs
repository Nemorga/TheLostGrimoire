using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Localization;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.Visual.HitSystem;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.GenericSlot;
using Kingmaker.UI.Group;
using Kingmaker.Utility;
using Kingmaker.Visual.Sound;
using Kingmaker.Visual.Animation.Kingmaker;
using UnityEngine;

namespace thelostgrimoire
{
    class Pet
    {
        static LibraryScriptableObject library => Main.library;
        static Sprite GetIcon(string id) => Helpers.GetIcon(id);
        static BlueprintAbility GetAbility(string id) => library.Get<BlueprintAbility>(id);
        static BlueprintFeature GetFeat(string id) => library.Get<BlueprintFeature>(id);
        static BlueprintBuff GetBuff(string id) => library.Get<BlueprintBuff>(id);
        static string Guid(string name) => Helpers.getGuid(name);

        static Sprite VoidIcon = GetIcon("fafd77c6bfa85c04ba31fdc1c962c914");//Just for show, for method that need it as parameter, in fact, we dont care

        public static BlueprintFeature CreateRank(string name, int RankCount = 20, bool classfeature = true)
        {
            var constructrank = Helpers.CreateFeature(name+"Rank", name, name, Guid(name+"Rank"), VoidIcon, FeatureGroup.None);
            constructrank.HideInUI = true;
            constructrank.Ranks = RankCount;
            constructrank.IsClassFeature = classfeature;

            return constructrank;

        }

        public static BlueprintProgression MasterRankProgression()
        {
            BlueprintProgression Prog = Helpers.CreateProgression();


            return Prog;
        }


    }
}
