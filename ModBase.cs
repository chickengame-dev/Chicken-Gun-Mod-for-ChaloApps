using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ChickenGun.ModAPI
{
    // ========== اطلاعات مود ==========
    [Serializable]
    public class ModInfo
    {
        public string Id;
        public string Name;
        public string Author;
        public string Version;
        public string Description;
    }

    // ========== کلاس پایه هر مود ==========
    public abstract class ModBase
    {
        public ModInfo Info { get; internal set; }
        public bool IsEnabled { get; private set; }

        public abstract void OnLoad();
        public abstract void OnUnload();
        public virtual void OnUpdate() { }

        internal void Enable()
        {
            if (IsEnabled) return;
            IsEnabled = true;
            try { OnLoad(); }
            catch (Exception e) { Debug.LogError($"[{Info.Name}] {e.Message}"); }
        }

        internal void Disable()
        {
            if (!IsEnabled) return;
            IsEnabled = false;
            try { OnUnload(); }
            catch (Exception e) { Debug.LogError($"[{Info.Name}] {e.Message}"); }
        }
    }

    // ========== رویدادهای بازی ==========
    public static class GameEvents
    {
        public static event Action<string, string> OnChickenDeath;
        public static event Action<string, string> OnShotFired;
        public static event Action<string> OnPlayerJoin;

        internal static void RaiseChickenDeath(string killer, string victim)
            => OnChickenDeath?.Invoke(killer, victim);

        internal static void RaiseShotFired(string player, string weapon)
            => OnShotFired?.Invoke(player, weapon);

        internal static void RaisePlayerJoin(string player)
            => OnPlayerJoin?.Invoke(player);
    }

    // ========== لودر مودها ==========
    public static class ModLoader
    {
        private static readonly List<ModBase> _mods = new List<ModBase>();
        public static IReadOnlyList<ModBase> Mods => _mods;
        public static string ModsFolder => Path.Combine(Application.persistentDataPath, "Mods");

        public static void Initialize()
        {
            if (!Directory.Exists(ModsFolder))
                Directory.CreateDirectory(ModsFolder);
            Debug.Log("ModLoader initialized. Folder: " + ModsFolder);
        }

        public static void Register(ModBase mod)
        {
            _mods.Add(mod);
            mod.Enable();
            Debug.Log($"Mod loaded: {mod.Info?.Name}");
        }

        public static void Unregister(ModBase mod)
        {
            mod.Disable();
            _mods.Remove(mod);
        }

        public static void UpdateAll()
        {
            foreach (var mod in _mods)
                if (mod.IsEnabled) mod.OnUpdate();
        }

        public static void UnloadAll()
        {
            foreach (var mod in _mods) mod.Disable();
            _mods.Clear();
        }
    }

    // ========== سندباکس امنیتی ==========
    public static class ModSandbox
    {
        public static bool ValidateMod(ModBase mod)
        {
            if (mod?.Info == null) return false;
            if (string.IsNullOrEmpty(mod.Info.Id)) return false;
            if (string.IsNullOrEmpty(mod.Info.Name)) return false;
            return true;
        }

        public static void SafeCall(Action action, string modName)
        {
            try { action?.Invoke(); }
            catch (Exception e) { Debug.LogError($"[{modName}] SafeCall failed: {e.Message}"); }
        }
    }

    // ========== نمونه مود: مرغ رنگین‌کمانی ==========
    public class RainbowChickenMod : ModBase
    {
        private float _hue = 0f;

        public RainbowChickenMod()
        {
            Info = new ModInfo
            {
                Id = "com.chickengame.rainbow",
                Name = "Rainbow Chicken",
                Author = "ChickenGame",
                Version = "1.0.0",
                Description = "Makes your chicken rainbow colored!"
            };
        }

        public override void OnLoad()
        {
            Debug.Log("Rainbow Chicken loaded!");
            GameEvents.OnChickenDeath += HandleDeath;
        }

        public override void OnUnload()
        {
            GameEvents.OnChickenDeath -= HandleDeath;
        }

        public override void OnUpdate()
        {
            _hue += Time.deltaTime * 0.5f;
            if (_hue > 1f) _hue -= 1f;
        }

        private void HandleDeath(string killer, string victim)
        {
            Debug.Log($"Chicken {victim} killed by {killer}");
        }
    }
}
