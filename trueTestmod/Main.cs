// Copyright (c) 2019 Jennifer Messerly
// This code is licensed under MIT license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.LevelUp;
using Kingmaker.Utility;
using UnityEngine;
using UnityModManagerNet;

namespace thelostgrimoire
{
    public class Main
    {
        [Harmony12.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary", new Type[0])]
        static class LibraryScriptableObject_LoadDictionary_Patch
        {
            static void Postfix(LibraryScriptableObject __instance)
            {
                var self = __instance;
                if (Main.library != null) return;
                Main.library = self;

                EnableGameLogging();

            
                SafeLoad(Helpers.Load, "Initialization code");


                // Note: needs to be loaded after other spells, so it can offer them as a choice.

                // Note: needs to run before almost everything else, so they can find the Oracle class.
                // However needs to run after spells are added, because it uses some of them.

                // Note: spells need to be added before this, because it adds metamagics.
                //
                // It needs to run after new classes too, because SpellSpecialization needs to find
                // all class spell lists.
                
                SafeLoad(GolemDiscovery.Load, "Golem Constructor");
                SafeLoad(RelatedFeat.Load, "Other feat");
                SafeLoad(ArcaneDiscoveries.Load, "Other Arcane Discoveries");
                SafeLoad(Fixandchange.Load, "Collection of feat and change");
                // Note: needs to run after arcane spells (it uses some of them).

                // Note: needs to run after things that add classes, and after bloodlines in case
                // they allow qualifying for racial prerequisites.


                // Note: needs to run after we create Favored Prestige Class above.

                // Note: needs to run after things that add bloodlines.

                // Note: needs to run after things that add martial classes or bloodlines.

                // Note: needs to run after crossblooded and spontaneous caster classes,
                // so it can find their spellbooks.


            }
           
        }
        [Harmony12.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
        static class LibraryScriptableObject_LoadDictionary2_Patch
        {
            static void Postfix()
            {
                try
                {
                    if (ShaderLookup == null)
                    {
                        ShaderLookup = new Dictionary<string, Shader>();
                        foreach (var shader in Resources.FindObjectsOfTypeAll<Shader>())
                        {
                            if (ShaderLookup.ContainsKey(shader.name))
                            {
                                Error($"Duplicate shader {shader.name}");
                            }
                            else
                            {
                                ShaderLookup[shader.name] = shader;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }

        [Harmony12.HarmonyPatch(typeof(AssetBundle), "LoadFromFile", new Type[] { typeof(string) })]
        static class AssetBundle_LoadFromFile_Patch
        {

            public static void FixPrefab(GameObject prefab)
            {
                var renderers = prefab.GetComponents<Renderer>();
                foreach (var renderer in renderers)
                {
                    foreach (var material in renderer.materials)
                    {
                        Log.Write($"Fixing shader {renderer.material.shader} on render {renderer.name} material {material.name}");
                        material.shader = ShaderLookup[material.shader.name];
                    }
                }
                // var decals = prefab.GetComponents<FxDecal>();
                //foreach (var decal in decals)
                //{
                //  Log($"Fixing shader {decal.SharedMaterial.shader} on decal {decal.name} material {decal.SharedMaterial.name}");
                // decal.SharedMaterial.shader = ShaderLookup[decal.SharedMaterial.shader.name];
                //}
                foreach (var t in prefab.transform)
                {
                    FixPrefab((t as Transform).gameObject);
                }
            }
            static bool Prefix(string path, ref AssetBundle __result)
            {
                try
                {
                    if (Path.GetFileName(path) == "resource_" + Helpers.getGuid("NewFXforSpectrecycle"))
                    {
                        
                        __result = AssetBundle.LoadFromFile($"{modpath}/Resources/AssetBundles/" + Helpers.getGuid("NewFXforSpectrecycle"), 0, 0);
                        var prefab = __result.LoadAllAssets<GameObject>()[0];
                        FixPrefab(prefab);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
                return true;
            }
        }


        internal static LibraryScriptableObject library;
        static Dictionary<string, Shader> ShaderLookup;
        public static UnityModManager.ModEntry modEntry;
        public static bool enabled;
        
        
        public static UnityModManager.ModEntry.ModLogger logger;
        public static string modpath;
        //internal static Settings settings;


        static Harmony12.HarmonyInstance harmonyInstance;

        static readonly Dictionary<Type, bool> typesPatched = new Dictionary<Type, bool>();
        static readonly List<String> failedPatches = new List<String>();
        static readonly List<String> failedLoading = new List<String>();

        [System.Diagnostics.Conditional("DEBUG")]
        static void EnableGameLogging()
        {
            if (UberLogger.Logger.Enabled) return;

            // Code taken from GameStarter.Awake(). PF:K logging can be enabled with command line flags,
            // but when developing the mod it's easier to force it on.
            var dataPath = ApplicationPaths.persistentDataPath;
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            UberLogger.Logger.Enabled = true;
            var text = Path.Combine(dataPath, "GameLog.txt");
            if (File.Exists(text))
            {
                File.Copy(text, Path.Combine(dataPath, "GameLogPrev.txt"), overwrite: true);
                File.Delete(text);
            }
            UberLogger.Logger.AddLogger(new UberLoggerFile("GameLogFull.txt", dataPath));
            UberLogger.Logger.AddLogger(new UberLoggerFilter(new UberLoggerFile("GameLog.txt", dataPath), UberLogger.LogSeverity.Warning, "MatchLight"));

            UberLogger.Logger.Enabled = true;
        }


        static string GetMd5Hash(MD5 md5, string input)
        {
            // Note: this uses MD5 to hash a string to 128-bits, it's not for any security purpose.
            var str = new StringBuilder();
            foreach (var b in md5.ComputeHash(Encoding.UTF8.GetBytes(input)))
            {
                str.Append(b.ToString("x2"));
            }
            Log.Write($"md5 hash of '${input}' is: {str}");
            return str.ToString();
        }



        // We don't want one patch failure to take down the entire mod, so they're applied individually.
        //
        // Also, in general the return value should be ignored. If a patch fails, we still want to create
        // blueprints, otherwise the save won't load. Better to have something be non-functional.
        internal static bool ApplyPatch(Type type, String featureName)
        {
            try
            {
                if (typesPatched.ContainsKey(type)) return typesPatched[type];

                var patchInfo = Harmony12.HarmonyMethodExtensions.GetHarmonyMethods(type);
                if (patchInfo == null || patchInfo.Count() == 0)
                {
                    Log.Error($"Failed to apply patch {type}: could not find Harmony attributes");
                    failedPatches.Add(featureName);
                    typesPatched.Add(type, false);
                    return false;
                }
                var processor = new Harmony12.PatchProcessor(harmonyInstance, type, Harmony12.HarmonyMethod.Merge(patchInfo));
                var patch = processor.Patch().FirstOrDefault();
                if (patch == null)
                {
                    Log.Error($"Failed to apply patch {type}: no dynamic method generated");
                    failedPatches.Add(featureName);
                    typesPatched.Add(type, false);
                    return false;
                }
                typesPatched.Add(type, true);
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to apply patch {type}: {e}");
                failedPatches.Add(featureName);
                typesPatched.Add(type, false);
                return false;
            }
        }

        static void CheckPatchingSuccess()
        {
            // Check to make sure we didn't forget to patch something.
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var infos = Harmony12.HarmonyMethodExtensions.GetHarmonyMethods(type);
                if (infos != null && infos.Count() > 0 && !typesPatched.ContainsKey(type))
                {
                    Log.Write($"Did not apply patch for {type}");
                }
            }
        }

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            //modEntry.OnSaveGUI = OnSaveGUI;
            modpath = modEntry.Path;
            //settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            harmonyInstance = Harmony12.HarmonyInstance.Create(modEntry.Info.Id);
            ApplyPatch(typeof(LibraryScriptableObject_LoadDictionary2_Patch), "Loading the shader");
            ApplyPatch(typeof(AssetBundle_LoadFromFile_Patch), "Loading the new fx");
            if (!ApplyPatch(typeof(LibraryScriptableObject_LoadDictionary_Patch), "All mod features"))
            {
                // If we can't patch this, nothing will work, so want the mod to turn red in UMM.
                throw Error("Failed to patch LibraryScriptableObject.LoadDictionary(), cannot load mod");
            }
            
            return true;
        }

        static void WriteBlueprints()
        {
            Log.Append("\n--------------------------------------------------------------------------------");
            foreach (var blueprint in library.GetAllBlueprints())
            {
                Log.Append($"var {blueprint.name} = library.Get<{blueprint.GetType().Name}>(\"{blueprint.AssetGuid}\");");
            }
            Log.Append("--------------------------------------------------------------------------------\n");
            Log.Flush();
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (!enabled) return;

            var fixedWidth = new GUILayoutOption[1] { GUILayout.ExpandWidth(false) };

            if (failedPatches.Count > 0)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("<b>Error: Some patches failed to apply. These features may not work:</b>", fixedWidth);
                foreach (var featureName in failedPatches)
                {
                    GUILayout.Label($"  • <b>{featureName}</b>", fixedWidth);
                }
                GUILayout.EndVertical();
            }
            if (failedLoading.Count > 0)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("<b>Error: Some assets failed to load. Saves using these features won't work:</b>", fixedWidth);
                foreach (var featureName in failedLoading)
                {
                    GUILayout.Label($"  • <b>{featureName}</b>", fixedWidth);
                }
                GUILayout.EndVertical();
            }
            /*
            settings.EldritchKnightFix = GUILayout.Toggle(settings.EldritchKnightFix,
                "Eldritch Knight requires martial class (doesn't affect existing EKs)", fixedWidth);

            settings.ShowCustomPortraits = GUILayout.Toggle(settings.ShowCustomPortraits,
                "Show custom portraits in the portrait list at character creation (if changed, requires restart)", fixedWidth);

            settings.OracleHas3SkillPoints = GUILayout.Toggle(settings.OracleHas3SkillPoints,
                "Give Oracle class 3+int skill points on level up (instead of 4, due to condensed skills)");
           

            settings.RelaxAncientLorekeeper = GUILayout.Toggle(settings.RelaxAncientLorekeeper,
                "Any race can choose the Oracle Ancient Lorekeeper archetype", fixedWidth);

            settings.RelaxTonguesCurse = GUILayout.Toggle(settings.RelaxTonguesCurse,
                "Disable Tongues curse penalty (that party members need 1 rank Knowledge: World to be controlled by PC in combat)", fixedWidth);*/
        }

       /* static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }*/

        internal static void SafeLoad(Action load, String name)
        {
            try
            {
                load();
            }
            catch (Exception e)
            {
                failedLoading.Add(name);
                Log.Error(e);
            }
        }

        internal static T SafeLoad<T>(Func<T> load, String name)
        {
            try
            {
                return load();
            }
            catch (Exception e)
            {
                failedLoading.Add(name);
                Log.Error(e);
                return default(T);
            }
        }

        internal static Exception Error(String message)
        {
            logger?.Log(message);
            return new InvalidOperationException(message);
        }
    }


   /* public class Settings : UnityModManager.ModSettings
    {
        public bool EldritchKnightFix = true;

        public bool ShowCustomPortraits = false;

        public bool RelaxAncientLorekeeper = false;

        public bool RelaxTonguesCurse = false;

        public bool OracleHas3SkillPoints = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
    }*/
}
