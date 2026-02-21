using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FistVR;
using HarmonyLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Niko666
{
    [BepInAutoPlugin]
    [BepInProcess("h3vr.exe")]
    public partial class UniversalRecoilControl : BaseUnityPlugin
    {
        public static UniversalRecoilControl Instance { get; private set; }
        public static Dictionary<string, WeaponSettings> WeaponOverrides = new Dictionary<string, WeaponSettings>();
        public ConfigEntry<bool> configLogDebug;
        public ConfigEntry<float> configVerticalRotPerShot_MultiplyAmount;
        public ConfigEntry<float> configVerticalRotPerShot_AddendAmount;
        public ConfigEntry<float> configHorizontalRotPerShot_MultiplyAmount;
        public ConfigEntry<float> configHorizontalRotPerShot_AddendAmount;
        public ConfigEntry<float> configMaxVerticalRot_MultiplyAmount;
        public ConfigEntry<float> configMaxHorizontalRot_MultiplyAmount;
        public ConfigEntry<float> configVerticalRotRecovery_MultiplyAmount;
        public ConfigEntry<float> configHorizontalRotRecovery_MultiplyAmount;
        public ConfigEntry<float> configZLinearPerShot_MultiplyAmount;
        public ConfigEntry<float> configZLinearPerShot_AddendAmount;
        public ConfigEntry<float> configZLinearMax_MultiplyAmount;
        public ConfigEntry<float> configZLinearRecovery_MultiplyAmount;
        public ConfigEntry<float> configXYLinearPerShot_MultiplyAmount;
        public ConfigEntry<float> configXYLinearPerShot_AddendAmount;
        public ConfigEntry<float> configXYLinearMax_MultiplyAmount;
        public ConfigEntry<float> configXYLinearRecovery_MultiplyAmount;
        public ConfigEntry<bool> configIsConstantRecoil;
        public ConfigEntry<float> configVerticalRotPerShot_Bipodded_MultiplyAmount;
        public ConfigEntry<float> configVerticalRotPerShot_Bipodded_AddendAmount;
        public ConfigEntry<float> configHorizontalRotPerShot_Bipodded_MultiplyAmount;
        public ConfigEntry<float> configHorizontalRotPerShot_Bipodded_AddendAmount;
        public ConfigEntry<float> configMaxVerticalRot_Bipodded_MultiplyAmount;
        public ConfigEntry<float> configMaxHorizontalRot_Bipodded_MultiplyAmount;
        public ConfigEntry<float> configRecoveryStabilizationFactors_Foregrip_MultiplyAmount;
        public ConfigEntry<float> configRecoveryStabilizationFactors_Twohand_MultiplyAmount;
        public ConfigEntry<float> configRecoveryStabilizationFactors_None_MultiplyAmount;
        public ConfigEntry<float> configMassDriftIntensity_MultiplyAmount;
        public ConfigEntry<float> configMassDriftFactors_MultiplyAmount;
        public ConfigEntry<float> configMaxMassDriftMagnitude_MultiplyAmount;
        public ConfigEntry<float> configMaxMassMaxRotation_MultiplyAmount;
        public ConfigEntry<float> configMassDriftRecoveryFactor_MultiplyAmount;
        // 默认值常量
        private const float DefaultVerticalRotPerShot_MultiplyAmount = 1.1f;
        private const float DefaultVerticalRotPerShot_AddendAmount = 1f;
        private const float DefaultHorizontalRotPerShot_MultiplyAmount = 1.1f;
        private const float DefaultHorizontalRotPerShot_AddendAmount = 1f;
        private const float DefaultMaxVerticalRot_MultiplyAmount = 1.1f;
        private const float DefaultMaxHorizontalRot_MultiplyAmount = 1.1f;
        private const float DefaultVerticalRotRecovery_MultiplyAmount = 1f;
        private const float DefaultHorizontalRotRecovery_MultiplyAmount = 1f;
        private const float DefaultZLinearPerShot_MultiplyAmount = 1.5f;
        private const float DefaultZLinearPerShot_AddendAmount = 5f;
        private const float DefaultZLinearMax_MultiplyAmount = 1.5f;
        private const float DefaultZLinearRecovery_MultiplyAmount = 1f;
        private const float DefaultXYLinearPerShot_MultiplyAmount = 1.1f;
        private const float DefaultXYLinearPerShot_AddendAmount = 1f;
        private const float DefaultXYLinearMax_MultiplyAmount = 1.1f;
        private const float DefaultXYLinearRecovery_MultiplyAmount = 1f;
        private const bool DefaultIsConstantRecoil = false;
        private const float DefaultVerticalRotPerShot_Bipodded_MultiplyAmount = 1.1f;
        private const float DefaultVerticalRotPerShot_Bipodded_AddendAmount = 1f;
        private const float DefaultHorizontalRotPerShot_Bipodded_MultiplyAmount = 1.1f;
        private const float DefaultHorizontalRotPerShot_Bipodded_AddendAmount = 1f;
        private const float DefaultMaxVerticalRot_Bipodded_MultiplyAmount = 1.1f;
        private const float DefaultMaxHorizontalRot_Bipodded_MultiplyAmount = 1.1f;
        private const float DefaultRecoveryStabilizationFactors_Foregrip_MultiplyAmount = 1f;
        private const float DefaultRecoveryStabilizationFactors_Twohand_MultiplyAmount = 1f;
        private const float DefaultRecoveryStabilizationFactors_None_MultiplyAmount = 1f;
        private const float DefaultMassDriftIntensity_MultiplyAmount = 1f;
        private const float DefaultMassDriftFactors_MultiplyAmount = 1f;
        private const float DefaultMaxMassDriftMagnitude_MultiplyAmount = 1f;
        private const float DefaultMaxMassMaxRotation_MultiplyAmount = 1f;
        private const float DefaultMassDriftRecoveryFactor_MultiplyAmount = 1f;

        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;
            // 初始化配置项
            configLogDebug = Config.Bind("Debug", "LogDebug", false, "Log debug information");
            configVerticalRotPerShot_MultiplyAmount = Config.Bind("Recoil Impulse Params",
                                                "VerticalRotPerShot_MultiplyAmount",
                                                DefaultVerticalRotPerShot_MultiplyAmount,
                                                "Multiply the vertical recoil rotations (Pitch Up/Down). 1 being H3VR defaults.");
            configVerticalRotPerShot_AddendAmount = Config.Bind("Recoil Impulse Params",
                                                "VerticalRotPerShot_AddendAmount",
                                                DefaultVerticalRotPerShot_AddendAmount,
                                                "The amount of rotations which will always get added on vertical recoils so weaker firearms will still have some movements");
            configHorizontalRotPerShot_MultiplyAmount = Config.Bind("Recoil Impulse Params",
                                                "HorizontalRotPerShot_MultiplyAmount",
                                                DefaultHorizontalRotPerShot_MultiplyAmount,
                                                "Multiply the horizontal recoil rotations (Yaw Left/Right). 1 being H3VR defaults.");
            configHorizontalRotPerShot_AddendAmount = Config.Bind("Recoil Impulse Params",
                                                "HorizontalRotPerShot_AddendAmount",
                                                DefaultHorizontalRotPerShot_AddendAmount,
                                                "The amount of rotations which will always get added on horizontal recoils so weaker firearms will still have some movements");
            configMaxVerticalRot_MultiplyAmount = Config.Bind("Recoil Impulse Params",
                                                "MaxVerticalRot_MultiplyAmount",
                                                DefaultMaxVerticalRot_MultiplyAmount,
                                                "Multiply the maximun vertical recoil rotations (Pitch Up/Down). 1 being H3VR defaults.");
            configMaxHorizontalRot_MultiplyAmount = Config.Bind("Recoil Impulse Params",
                                                "MaxHorizontalRot_MultiplyAmount",
                                                DefaultMaxHorizontalRot_MultiplyAmount,
                                                "Multiply the maximun horizontal recoil rotations (Yaw Left/Right). 1 being H3VR defaults.");
            configVerticalRotRecovery_MultiplyAmount = Config.Bind("Recoil Impulse Params",
                                                "VerticalRotRecovery_MultiplyAmount",
                                                DefaultVerticalRotRecovery_MultiplyAmount,
                                                "Multiply the vertical recoil recovery speed. 1 being H3VR defaults.");
            configHorizontalRotRecovery_MultiplyAmount = Config.Bind("Recoil Impulse Params",
                                                "HorizontalRotRecovery_MultiplyAmount",
                                                DefaultHorizontalRotRecovery_MultiplyAmount,
                                                "Multiply the horizontal recoil recovery speed. 1 being H3VR defaults.");
            configZLinearPerShot_MultiplyAmount = Config.Bind("Recoil Impulse Params",
                                                "ZLinearPerShot_MultiplyAmount",
                                                DefaultZLinearPerShot_MultiplyAmount,
                                                "Multiply the Z linear recoil movements (Backwards/Forwards). 1 being H3VR defaults.");
            configZLinearPerShot_AddendAmount = Config.Bind("Recoil Impulse Params",
                                                "ZLinearPerShot_AddendAmount",
                                                DefaultZLinearPerShot_AddendAmount,
                                                "The amount of movements which will always get added on Z linear recoils so weaker firearms will still have some movements");
            configZLinearMax_MultiplyAmount = Config.Bind("Recoil Impulse Params",
                                                "ZLinearMax_MultiplyAmount",
                                                DefaultZLinearMax_MultiplyAmount,
                                                "Multiply the maximun Z linear recoil movements (Backwards/Forwards). 1 being H3VR defaults.");
            configZLinearRecovery_MultiplyAmount = Config.Bind("Recoil Impulse Params",
                                                "ZLinearRecovery_MultiplyAmount",
                                                DefaultZLinearRecovery_MultiplyAmount,
                                                "Multiply the Z linear recoil recovery speed. 1 being H3VR defaults.");
            configXYLinearPerShot_MultiplyAmount = Config.Bind("Recoil Impulse Params",
                                                "XYLinearPerShot_MultiplyAmount",
                                                DefaultXYLinearPerShot_MultiplyAmount,
                                                "Multiply the XY linear recoil movements (Left/Right). 1 being H3VR defaults.");
            configXYLinearPerShot_AddendAmount = Config.Bind("Recoil Impulse Params",
                                                "XYLinearPerShot_AddendAmount",
                                                DefaultXYLinearPerShot_AddendAmount,
                                                "The amount of movements which will always get added on XY linear recoils so weaker firearms will still have some movements");
            configXYLinearMax_MultiplyAmount = Config.Bind("Recoil Impulse Params",
                                                "XYLinearMax_MultiplyAmount",
                                                DefaultXYLinearMax_MultiplyAmount,
                                                "Multiply the maximun XY linear recoil movements (Left/Right). 1 being H3VR defaults.");
            configXYLinearRecovery_MultiplyAmount = Config.Bind("Recoil Impulse Params",
                                                "XYLinearRecovery_MultiplyAmount",
                                                DefaultXYLinearRecovery_MultiplyAmount,
                                                "Multiply the XY linear recoil recovery speed. 1 being H3VR defaults.");
            configIsConstantRecoil = Config.Bind("Recoil Impulse Params",
                                                "IsConstantRecoil",
                                                DefaultIsConstantRecoil,
                                                "Toggle if the firearm uses constant recoil. Untested but most firearms has this off.");
            configVerticalRotPerShot_Bipodded_MultiplyAmount = Config.Bind("Bipodded Params",
                                                "VerticalRotPerShot_Bipodded_MultiplyAmount",
                                                DefaultVerticalRotPerShot_Bipodded_MultiplyAmount,
                                                "Multiply the vertical recoil per shot when bipodded. 1 being H3VR defaults.");
            configVerticalRotPerShot_Bipodded_AddendAmount = Config.Bind("Bipodded Params",
                                                "VerticalRotPerShot_Bipodded_AddendAmount",
                                                DefaultVerticalRotPerShot_Bipodded_AddendAmount,
                                                "The amount of movements which will always get added on vertical recoil so weaker firearms will still have some movements");
            configHorizontalRotPerShot_Bipodded_MultiplyAmount = Config.Bind("Bipodded Params",
                                                "HorizontalRotPerShot_Bipodded_MultiplyAmount",
                                                DefaultHorizontalRotPerShot_Bipodded_MultiplyAmount,
                                                "Multiply the horizontal recoil per shot when bipodded. 1 being H3VR defaults.");
            configHorizontalRotPerShot_Bipodded_AddendAmount = Config.Bind("Bipodded Params",
                                                "HorizontalRotPerShot_Bipodded_AddendAmount",
                                                DefaultHorizontalRotPerShot_Bipodded_AddendAmount,
                                                "The amount of movements which will always get added on horizontal recoil so weaker firearms will still have some movements");
            configMaxVerticalRot_Bipodded_MultiplyAmount = Config.Bind("Bipodded Params",
                                                "MaxVerticalRot_Bipodded_MultiplyAmount",
                                                DefaultMaxVerticalRot_Bipodded_MultiplyAmount,
                                                "Multiply the maximun vertical recoil when bipodded. 1 being H3VR defaults.");
            configMaxHorizontalRot_Bipodded_MultiplyAmount = Config.Bind("Bipodded Params",
                                                "MaxHorizontalRot_Bipodded_MultiplyAmount",
                                                DefaultMaxHorizontalRot_Bipodded_MultiplyAmount,
                                                "Multiply the maximun horizontal recoil when bipodded. 1 being H3VR defaults.");
            configRecoveryStabilizationFactors_Foregrip_MultiplyAmount = Config.Bind("Recoil Recovery Params",
                                                "RecoveryStabilizationFactors_Foregrip_MultiplyAmount",
                                                DefaultRecoveryStabilizationFactors_Foregrip_MultiplyAmount,
                                                "Multiply the recoil stabilization factors when holding the foregrip. 1 being H3VR defaults.");
            configRecoveryStabilizationFactors_Twohand_MultiplyAmount = Config.Bind("Recoil Recovery Params",
                                                "RecoveryStabilizationFactors_Twohand_MultiplyAmount",
                                                DefaultRecoveryStabilizationFactors_Twohand_MultiplyAmount,
                                                "Multiply the recoil stabilization factors when holding the firearm two-handed. 1 being H3VR defaults.");
            configRecoveryStabilizationFactors_None_MultiplyAmount = Config.Bind("Recoil Recovery Params",
                                                "RecoveryStabilizationFactors_None_MultiplyAmount",
                                                DefaultRecoveryStabilizationFactors_None_MultiplyAmount,
                                                "Multiply the recoil stabilization factors when there's no special action (one-handed). 1 being H3VR defaults.");
            configMassDriftIntensity_MultiplyAmount = Config.Bind("Weapon Dimension/Mass Related Params",
                                                "MassDriftIntensity_MultiplyAmount",
                                                DefaultMassDriftIntensity_MultiplyAmount,
                                                "Multiply the mass drift intensity. 1 being H3VR defaults.");
            configMassDriftFactors_MultiplyAmount = Config.Bind("Weapon Dimension/Mass Related Params",
                                                "MassDriftFactors_MultiplyAmount",
                                                DefaultMassDriftFactors_MultiplyAmount,
                                                "Multiply the mass drift factors. 1 being H3VR defaults.");
            configMaxMassDriftMagnitude_MultiplyAmount = Config.Bind("Weapon Dimension/Mass Related Params",
                                                "MaxMassDriftMagnitude_MultiplyAmount",
                                                DefaultMaxMassDriftMagnitude_MultiplyAmount,
                                                "Multiply the maximun mass drift magnitude. 1 being H3VR defaults.");
            configMaxMassMaxRotation_MultiplyAmount = Config.Bind("Weapon Dimension/Mass Related Params",
                                                "MaxMassMaxRotation_MultiplyAmount",
                                                DefaultMaxMassMaxRotation_MultiplyAmount,
                                                "Multiply the maximun mass max rotation. 1 being H3VR defaults.");
            configMassDriftRecoveryFactor_MultiplyAmount = Config.Bind("Weapon Dimension/Mass Related Params",
                                                "MassDriftRecoveryFactor_MultiplyAmount",
                                                DefaultMassDriftRecoveryFactor_MultiplyAmount,
                                                "Multiply the mass drift recovery factor. 1 being H3VR defaults.");
            Instance.LoadJsonConfig();
            Harmony.CreateAndPatchAll(typeof(UniversalRecoilControlPatch), null);
            Logger.LogMessage($"Fuck this world! Sent from {Id} {Version}");
        }
        public void LogDebugMessage(string message)
        {
            if (Instance.configLogDebug.Value)
                Logger.LogInfo(message);
        }
        public class WeaponSettings
        {
            public float? VerticalRotPerShot_MultiplyAmount { get; set; }
            public float? VerticalRotPerShot_AddendAmount { get; set; }
            public float? HorizontalRotPerShot_MultiplyAmount { get; set; }
            public float? HorizontalRotPerShot_AddendAmount { get; set; }
            public float? MaxVerticalRot_MultiplyAmount { get; set; }
            public float? MaxHorizontalRot_MultiplyAmount { get; set; }
            public float? VerticalRotRecovery_MultiplyAmount { get; set; }
            public float? HorizontalRotRecovery_MultiplyAmount { get; set; }
            public float? ZLinearPerShot_MultiplyAmount { get; set; }
            public float? ZLinearPerShot_AddendAmount { get; set; }
            public float? ZLinearMax_MultiplyAmount { get; set; }
            public float? ZLinearRecovery_MultiplyAmount { get; set; }
            public float? XYLinearPerShot_MultiplyAmount { get; set; }
            public float? XYLinearPerShot_AddendAmount { get; set; }
            public float? XYLinearMax_MultiplyAmount { get; set; }
            public float? XYLinearRecovery_MultiplyAmount { get; set; }
            public bool? IsConstantRecoil { get; set; }
            public float? VerticalRotPerShot_Bipodded_MultiplyAmount { get; set; }
            public float? VerticalRotPerShot_Bipodded_AddendAmount { get; set; }
            public float? HorizontalRotPerShot_Bipodded_MultiplyAmount { get; set; }
            public float? HorizontalRotPerShot_Bipodded_AddendAmount { get; set; }
            public float? MaxVerticalRot_Bipodded_MultiplyAmount { get; set; }
            public float? MaxHorizontalRot_Bipodded_MultiplyAmount { get; set; }
            public float? RecoveryStabilizationFactors_Foregrip_MultiplyAmount { get; set; }
            public float? RecoveryStabilizationFactors_Twohand_MultiplyAmount { get; set; }
            public float? RecoveryStabilizationFactors_None_MultiplyAmount { get; set; }
            public float? MassDriftIntensity_MultiplyAmount { get; set; }
            public float? MassDriftFactors_MultiplyAmount { get; set; }
            public float? MaxMassDriftMagnitude_MultiplyAmount { get; set; }
            public float? MaxMassMaxRotation_MultiplyAmount { get; set; }
            public float? MassDriftRecoveryFactor_MultiplyAmount { get; set; }
        }
        public class RecoilConfigData
        {
            public Dictionary<string, WeaponSettings> WeaponOverrides { get; set; }
        }
        public void CreateDefaultJsonConfig(string path)
        {
            try
            {
                // 创建一个示例数据结构
                var defaultData = new RecoilConfigData
                {
                    WeaponOverrides = new Dictionary<string, WeaponSettings>
                    {
                        {
                            "InsertFirearmObjectIDHere",
                            new WeaponSettings
                            {
                                VerticalRotPerShot_MultiplyAmount = 1.0f,
                                VerticalRotPerShot_AddendAmount = 0.0f,
                                HorizontalRotPerShot_MultiplyAmount = 1.0f,
                                HorizontalRotPerShot_AddendAmount = 0.0f,
                                MaxVerticalRot_MultiplyAmount = 1.0f,
                                MaxHorizontalRot_MultiplyAmount = 1.0f,
                                VerticalRotRecovery_MultiplyAmount = 1.0f,
                                HorizontalRotRecovery_MultiplyAmount = 1.0f,
                                ZLinearPerShot_MultiplyAmount = 1.0f,
                                ZLinearPerShot_AddendAmount = 0.0f,
                                ZLinearMax_MultiplyAmount = 1.0f,
                                ZLinearRecovery_MultiplyAmount = 1.0f,
                                XYLinearPerShot_MultiplyAmount = 1.0f,
                                XYLinearPerShot_AddendAmount= 0.0f,
                                XYLinearMax_MultiplyAmount = 1.0f,
                                XYLinearRecovery_MultiplyAmount = 1.0f,
                                IsConstantRecoil = false,
                                VerticalRotPerShot_Bipodded_MultiplyAmount = 1.0f,
                                VerticalRotPerShot_Bipodded_AddendAmount = 0.0f,
                                HorizontalRotPerShot_Bipodded_MultiplyAmount = 1.0f,
                                HorizontalRotPerShot_Bipodded_AddendAmount = 0.0f,
                                MaxVerticalRot_Bipodded_MultiplyAmount = 1.0f,
                                MaxHorizontalRot_Bipodded_MultiplyAmount = 1.0f,
                                RecoveryStabilizationFactors_Foregrip_MultiplyAmount = 1.0f,
                                RecoveryStabilizationFactors_Twohand_MultiplyAmount = 1.0f,
                                RecoveryStabilizationFactors_None_MultiplyAmount = 1.0f,
                                MassDriftIntensity_MultiplyAmount = 1.0f,
                                MassDriftFactors_MultiplyAmount = 1.0f,
                                MaxMassDriftMagnitude_MultiplyAmount = 1.0f,
                                MaxMassMaxRotation_MultiplyAmount = 1.0f,
                                MassDriftRecoveryFactor_MultiplyAmount = 1.0f
                            }
                        }
                        ,
                        {
                            "ForExampleM1911Classic",
                            new WeaponSettings
                            {
                                 VerticalRotPerShot_MultiplyAmount = 1.0f,
                                VerticalRotPerShot_AddendAmount = 0.0f,
                                HorizontalRotPerShot_MultiplyAmount = 1.0f,
                                HorizontalRotPerShot_AddendAmount = 0.0f,
                                MaxVerticalRot_MultiplyAmount = 1.0f,
                                MaxHorizontalRot_MultiplyAmount = 1.0f,
                                VerticalRotRecovery_MultiplyAmount = 1.0f,
                                HorizontalRotRecovery_MultiplyAmount = 1.0f,
                                ZLinearPerShot_MultiplyAmount = 1.0f,
                                ZLinearPerShot_AddendAmount = 0.0f,
                                ZLinearMax_MultiplyAmount = 1.0f,
                                ZLinearRecovery_MultiplyAmount = 1.0f,
                                XYLinearPerShot_MultiplyAmount = 1.0f,
                                XYLinearPerShot_AddendAmount= 0.0f,
                                XYLinearMax_MultiplyAmount = 1.0f,
                                XYLinearRecovery_MultiplyAmount = 1.0f,
                                IsConstantRecoil = false,
                                VerticalRotPerShot_Bipodded_MultiplyAmount = 1.0f,
                                VerticalRotPerShot_Bipodded_AddendAmount = 0.0f,
                                HorizontalRotPerShot_Bipodded_MultiplyAmount = 1.0f,
                                HorizontalRotPerShot_Bipodded_AddendAmount = 0.0f,
                                MaxVerticalRot_Bipodded_MultiplyAmount = 1.0f,
                                MaxHorizontalRot_Bipodded_MultiplyAmount = 1.0f,
                                RecoveryStabilizationFactors_Foregrip_MultiplyAmount = 1.0f,
                                RecoveryStabilizationFactors_Twohand_MultiplyAmount = 1.0f,
                                RecoveryStabilizationFactors_None_MultiplyAmount = 1.0f,
                                MassDriftIntensity_MultiplyAmount = 1.0f,
                                MassDriftFactors_MultiplyAmount = 1.0f,
                                MaxMassDriftMagnitude_MultiplyAmount = 1.0f,
                                MaxMassMaxRotation_MultiplyAmount = 1.0f,
                                MassDriftRecoveryFactor_MultiplyAmount = 1.0f
                            }
                        }
                    }
                };

                // 序列化为 JSON 字符串
                string json = JsonConvert.SerializeObject(defaultData, Formatting.Indented);

                // 写入文件
                File.WriteAllText(path, json);
                Logger.LogInfo($"Created default config at: {path}");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Error creating default config: {ex.Message}\n{ex.StackTrace}");
            }
        }
        public void LoadJsonConfig()
        {
            try
            {
                // 获取配置目录路径
                string configDirectory = Path.GetDirectoryName(Paths.BepInExConfigPath);
                if (string.IsNullOrEmpty(configDirectory))
                {
                    Logger.LogError("Failed to get config directory path.");
                    return;
                }

                string configPath = Path.Combine(configDirectory, "Niko666.UniversalRecoilControl.json");

                // 如果文件不存在，创建一个默认的示例文件
                if (!File.Exists(configPath))
                {
                    Logger.LogWarning("Config file not found. Creating default config...");
                    CreateDefaultJsonConfig(configPath);
                }

                // 读取文件内容
                string json = File.ReadAllText(configPath);

                // 反序列化为对象
                var data = JsonConvert.DeserializeObject<RecoilConfigData>(json);

                if (data != null && data.WeaponOverrides != null)
                {
                    WeaponOverrides = data.WeaponOverrides;
                    Logger.LogInfo($"Successfully loaded {WeaponOverrides.Count} weapon overrides.");
                    foreach (var kvp in WeaponOverrides)
                    {
                        LogDebugMessage($"  - {kvp.Key}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Error loading config: {ex.Message}\n{ex.StackTrace}");
            }
        }
        public static FVRFireArmRecoilProfile CopyAndAdjustRecoilProfile(FVRFireArmRecoilProfile orig, string weaponID)
        {
            FVRFireArmRecoilProfile tempRecoilProfile = Object.Instantiate(orig);
            // 从配置中获取全局值
            float VerticalRotPerShot_MultiplyAmount = Instance.configVerticalRotPerShot_MultiplyAmount.Value;
            float VerticalRotPerShot_AddendAmount = Instance.configVerticalRotPerShot_AddendAmount.Value;
            float HorizontalRotPerShot_MultiplyAmount = Instance.configHorizontalRotPerShot_MultiplyAmount.Value;
            float HorizontalRotPerShot_AddendAmount = Instance.configHorizontalRotPerShot_AddendAmount.Value;
            float MaxVerticalRot_MultiplyAmount = Instance.configMaxVerticalRot_MultiplyAmount.Value;
            float MaxHorizontalRot_MultiplyAmount = Instance.configMaxHorizontalRot_MultiplyAmount.Value;
            float VerticalRotRecovery_MultiplyAmount = Instance.configVerticalRotRecovery_MultiplyAmount.Value;
            float HorizontalRotRecovery_MultiplyAmount = Instance.configHorizontalRotRecovery_MultiplyAmount.Value;
            float ZLinearPerShot_MultiplyAmount = Instance.configZLinearPerShot_MultiplyAmount.Value;
            float ZLinearPerShot_AddendAmount = Instance.configZLinearPerShot_AddendAmount.Value;
            float ZLinearMax_MultiplyAmount = Instance.configZLinearMax_MultiplyAmount.Value;
            float ZLinearRecovery_MultiplyAmount = Instance.configZLinearRecovery_MultiplyAmount.Value;
            float XYLinearPerShot_MultiplyAmount = Instance.configXYLinearPerShot_MultiplyAmount.Value;
            float XYLinearPerShot_AddendAmount = Instance.configXYLinearPerShot_AddendAmount.Value;
            float XYLinearMax_MultiplyAmount = Instance.configXYLinearMax_MultiplyAmount.Value;
            float XYLinearRecovery_MultiplyAmount = Instance.configXYLinearRecovery_MultiplyAmount.Value;
            bool IsConstantRecoil = Instance.configIsConstantRecoil.Value;
            float VerticalRotPerShot_Bipodded_MultiplyAmount = Instance.configVerticalRotPerShot_Bipodded_MultiplyAmount.Value;
            float VerticalRotPerShot_Bipodded_AddendAmount = Instance.configVerticalRotPerShot_Bipodded_AddendAmount.Value;
            float HorizontalRotPerShot_Bipodded_MultiplyAmount = Instance.configHorizontalRotPerShot_Bipodded_MultiplyAmount.Value;
            float HorizontalRotPerShot_Bipodded_AddendAmount = Instance.configHorizontalRotPerShot_Bipodded_AddendAmount.Value;
            float MaxVerticalRot_Bipodded_MultiplyAmount = Instance.configMaxVerticalRot_Bipodded_MultiplyAmount.Value;
            float MaxHorizontalRot_Bipodded_MultiplyAmount = Instance.configMaxHorizontalRot_Bipodded_MultiplyAmount.Value;
            float RecoveryStabilizationFactors_Foregrip_MultiplyAmount = Instance.configRecoveryStabilizationFactors_Foregrip_MultiplyAmount.Value;
            float RecoveryStabilizationFactors_Twohand_MultiplyAmount = Instance.configRecoveryStabilizationFactors_Twohand_MultiplyAmount.Value;
            float RecoveryStabilizationFactors_None_MultiplyAmount = Instance.configRecoveryStabilizationFactors_None_MultiplyAmount.Value;
            float MassDriftIntensity_MultiplyAmount = Instance.configMassDriftIntensity_MultiplyAmount.Value;
            float MassDriftFactors_MultiplyAmount = Instance.configMassDriftFactors_MultiplyAmount.Value;
            float MaxMassDriftMagnitude_MultiplyAmount = Instance.configMaxMassDriftMagnitude_MultiplyAmount.Value;
            float MaxMassMaxRotation_MultiplyAmount = Instance.configMaxMassMaxRotation_MultiplyAmount.Value;
            float MassDriftRecoveryFactor_MultiplyAmount = Instance.configMassDriftRecoveryFactor_MultiplyAmount.Value;
            // 如果武器有特定配置，则覆盖全局值
            if (WeaponOverrides.TryGetValue(weaponID, out var settings))
            {
                Logger.LogInfo($"Applying weapon-specific override for {weaponID}");
                VerticalRotPerShot_MultiplyAmount = ApplyConfig(settings.VerticalRotPerShot_MultiplyAmount, VerticalRotPerShot_MultiplyAmount, "VerticalRotPerShot_MultiplyAmount");
                VerticalRotPerShot_AddendAmount = ApplyConfig(settings.VerticalRotPerShot_AddendAmount, VerticalRotPerShot_AddendAmount, "VerticalRotPerShot_AddendAmount");
                HorizontalRotPerShot_MultiplyAmount = ApplyConfig(settings.HorizontalRotPerShot_MultiplyAmount, HorizontalRotPerShot_MultiplyAmount, "HorizontalRotPerShot_MultiplyAmount");
                HorizontalRotPerShot_AddendAmount = ApplyConfig(settings.HorizontalRotPerShot_AddendAmount, HorizontalRotPerShot_AddendAmount, "HorizontalRotPerShot_AddendAmount");
                MaxVerticalRot_MultiplyAmount = ApplyConfig(settings.MaxVerticalRot_MultiplyAmount, MaxVerticalRot_MultiplyAmount, "MaxVerticalRot_MultiplyAmount");
                MaxHorizontalRot_MultiplyAmount = ApplyConfig(settings.MaxHorizontalRot_MultiplyAmount, MaxHorizontalRot_MultiplyAmount, "MaxHorizontalRot_MultiplyAmount");
                VerticalRotRecovery_MultiplyAmount = ApplyConfig(settings.VerticalRotRecovery_MultiplyAmount, VerticalRotRecovery_MultiplyAmount, "VerticalRotRecovery_MultiplyAmount");
                HorizontalRotRecovery_MultiplyAmount = ApplyConfig(settings.HorizontalRotRecovery_MultiplyAmount, HorizontalRotRecovery_MultiplyAmount, "HorizontalRotRecovery_MultiplyAmount");
                ZLinearPerShot_MultiplyAmount = ApplyConfig(settings.ZLinearPerShot_MultiplyAmount, ZLinearPerShot_MultiplyAmount, "ZLinearPerShot_MultiplyAmount");
                ZLinearPerShot_AddendAmount = ApplyConfig(settings.ZLinearPerShot_AddendAmount, ZLinearPerShot_AddendAmount, "ZLinearPerShot_AddendAmount");
                ZLinearMax_MultiplyAmount = ApplyConfig(settings.ZLinearMax_MultiplyAmount, ZLinearMax_MultiplyAmount, "ZLinearMax_MultiplyAmount");
                ZLinearRecovery_MultiplyAmount = ApplyConfig(settings.ZLinearRecovery_MultiplyAmount, ZLinearRecovery_MultiplyAmount, "ZLinearRecovery_MultiplyAmount");
                XYLinearPerShot_MultiplyAmount = ApplyConfig(settings.XYLinearPerShot_MultiplyAmount, XYLinearPerShot_MultiplyAmount, "XYLinearPerShot_MultiplyAmount");
                XYLinearPerShot_AddendAmount = ApplyConfig(settings.XYLinearPerShot_AddendAmount, XYLinearPerShot_AddendAmount, "XYLinearPerShot_AddendAmount");
                XYLinearMax_MultiplyAmount = ApplyConfig(settings.XYLinearMax_MultiplyAmount, XYLinearMax_MultiplyAmount, "XYLinearMax_MultiplyAmount");
                XYLinearRecovery_MultiplyAmount = ApplyConfig(settings.XYLinearRecovery_MultiplyAmount, XYLinearRecovery_MultiplyAmount, "XYLinearRecovery_MultiplyAmount");
                IsConstantRecoil = ApplyConfig(settings.IsConstantRecoil, IsConstantRecoil, "IsConstantRecoil");
                VerticalRotPerShot_Bipodded_MultiplyAmount = ApplyConfig(settings.VerticalRotPerShot_Bipodded_MultiplyAmount, VerticalRotPerShot_Bipodded_MultiplyAmount, "VerticalRotPerShot_Bipodded_MultiplyAmount");
                VerticalRotPerShot_Bipodded_AddendAmount = ApplyConfig(settings.VerticalRotPerShot_Bipodded_AddendAmount, VerticalRotPerShot_Bipodded_AddendAmount, "VerticalRotPerShot_Bipodded_AddendAmount");
                HorizontalRotPerShot_Bipodded_MultiplyAmount = ApplyConfig(settings.HorizontalRotPerShot_Bipodded_MultiplyAmount, HorizontalRotPerShot_Bipodded_MultiplyAmount, "HorizontalRotPerShot_Bipodded_MultiplyAmount");
                HorizontalRotPerShot_Bipodded_AddendAmount = ApplyConfig(settings.HorizontalRotPerShot_Bipodded_AddendAmount, HorizontalRotPerShot_Bipodded_AddendAmount, "HorizontalRotPerShot_Bipodded_AddendAmount");
                MaxVerticalRot_Bipodded_MultiplyAmount = ApplyConfig(settings.MaxVerticalRot_Bipodded_MultiplyAmount, MaxVerticalRot_Bipodded_MultiplyAmount, "MaxVerticalRot_Bipodded_MultiplyAmount");
                MaxHorizontalRot_Bipodded_MultiplyAmount = ApplyConfig(settings.MaxHorizontalRot_Bipodded_MultiplyAmount, MaxHorizontalRot_Bipodded_MultiplyAmount, "MaxHorizontalRot_Bipodded_MultiplyAmount");
                RecoveryStabilizationFactors_Foregrip_MultiplyAmount = ApplyConfig(settings.RecoveryStabilizationFactors_Foregrip_MultiplyAmount, RecoveryStabilizationFactors_Foregrip_MultiplyAmount, "RecoveryStabilizationFactors_Foregrip_MultiplyAmount");
                RecoveryStabilizationFactors_Twohand_MultiplyAmount = ApplyConfig(settings.RecoveryStabilizationFactors_Twohand_MultiplyAmount, RecoveryStabilizationFactors_Twohand_MultiplyAmount, "RecoveryStabilizationFactors_Twohand_MultiplyAmount");
                RecoveryStabilizationFactors_None_MultiplyAmount = ApplyConfig(settings.RecoveryStabilizationFactors_None_MultiplyAmount, RecoveryStabilizationFactors_None_MultiplyAmount, "RecoveryStabilizationFactors_Bipod_MultiplyAmount");
                MassDriftIntensity_MultiplyAmount = ApplyConfig(settings.MassDriftIntensity_MultiplyAmount, MassDriftIntensity_MultiplyAmount, "MassDriftIntensity_MultiplyAmount");
                MassDriftFactors_MultiplyAmount = ApplyConfig(settings.MassDriftFactors_MultiplyAmount, MassDriftFactors_MultiplyAmount, "MassDriftFactors_MultiplyAmount");
                MaxMassDriftMagnitude_MultiplyAmount = ApplyConfig(settings.MaxMassDriftMagnitude_MultiplyAmount, MaxMassDriftMagnitude_MultiplyAmount, "MaxMassDriftMagnitude_MultiplyAmount");
                MaxMassMaxRotation_MultiplyAmount = ApplyConfig(settings.MaxMassMaxRotation_MultiplyAmount, MaxMassMaxRotation_MultiplyAmount, "MaxMassMaxRotation_MultiplyAmount");
                MassDriftRecoveryFactor_MultiplyAmount = ApplyConfig(settings.MassDriftRecoveryFactor_MultiplyAmount, MassDriftRecoveryFactor_MultiplyAmount, "MassDriftRecoveryFactor_MultiplyAmount");
            }
            // 应用修改到临时配置
            tempRecoilProfile.VerticalRotPerShot = ValidateFloat(VerticalRotPerShot_MultiplyAmount * orig.VerticalRotPerShot + VerticalRotPerShot_AddendAmount, orig.VerticalRotPerShot, "VerticalRotPerShot");
            tempRecoilProfile.HorizontalRotPerShot = ValidateFloat(HorizontalRotPerShot_MultiplyAmount * orig.HorizontalRotPerShot + HorizontalRotPerShot_AddendAmount, orig.HorizontalRotPerShot, "HorizontalRotPerShot");
            tempRecoilProfile.MaxVerticalRot = ValidateFloatNoZero(MaxVerticalRot_MultiplyAmount * orig.MaxVerticalRot, orig.MaxVerticalRot, "MaxVerticalRot");
            tempRecoilProfile.MaxHorizontalRot = ValidateFloatNoZero(MaxHorizontalRot_MultiplyAmount * orig.MaxHorizontalRot, orig.MaxHorizontalRot, "MaxHorizontalRot");
            tempRecoilProfile.VerticalRotRecovery = ValidateFloat(VerticalRotRecovery_MultiplyAmount * orig.VerticalRotRecovery, orig.VerticalRotRecovery, "VerticalRotRecovery");
            tempRecoilProfile.HorizontalRotRecovery = ValidateFloat(HorizontalRotRecovery_MultiplyAmount * orig.HorizontalRotRecovery, orig.HorizontalRotRecovery, "HorizontalRotRecovery");
            tempRecoilProfile.ZLinearPerShot = ValidateFloat(ZLinearPerShot_MultiplyAmount * orig.ZLinearPerShot + ZLinearPerShot_AddendAmount, orig.ZLinearPerShot, "ZLinearPerShot");
            tempRecoilProfile.ZLinearMax = ValidateFloatNoZero(ZLinearMax_MultiplyAmount * orig.ZLinearMax, orig.ZLinearMax, "ZLinearMax");
            tempRecoilProfile.ZLinearRecovery = ValidateFloat(ZLinearRecovery_MultiplyAmount * orig.ZLinearRecovery, orig.ZLinearRecovery, "ZLinearRecovery");
            tempRecoilProfile.XYLinearPerShot = ValidateFloat(XYLinearPerShot_MultiplyAmount * orig.XYLinearPerShot + XYLinearPerShot_AddendAmount, orig.XYLinearPerShot, "XYLinearPerShot");
            tempRecoilProfile.XYLinearMax = ValidateFloatNoZero(XYLinearMax_MultiplyAmount * orig.XYLinearMax, orig.XYLinearMax, "XYLinearMax");
            tempRecoilProfile.XYLinearRecovery = ValidateFloat(XYLinearRecovery_MultiplyAmount * orig.XYLinearRecovery, orig.XYLinearRecovery, "XYLinearRecovery");
            tempRecoilProfile.IsConstantRecoil = IsConstantRecoil;
            tempRecoilProfile.VerticalRotPerShot_Bipodded = ValidateFloat(VerticalRotPerShot_Bipodded_MultiplyAmount * orig.VerticalRotPerShot_Bipodded + VerticalRotPerShot_Bipodded_AddendAmount, orig.VerticalRotPerShot_Bipodded, "VerticalRotPerShot_Bipodded");
            tempRecoilProfile.HorizontalRotPerShot_Bipodded = ValidateFloat(HorizontalRotPerShot_Bipodded_MultiplyAmount * orig.HorizontalRotPerShot_Bipodded + HorizontalRotPerShot_Bipodded_AddendAmount, orig.HorizontalRotPerShot_Bipodded, "HorizontalRotPerShot_Bipodded");
            tempRecoilProfile.MaxVerticalRot_Bipodded = ValidateFloatNoZero(MaxVerticalRot_Bipodded_MultiplyAmount * orig.MaxVerticalRot_Bipodded, orig.MaxVerticalRot_Bipodded, "MaxVerticalRot_Bipodded");
            tempRecoilProfile.MaxHorizontalRot_Bipodded = ValidateFloatNoZero(MaxHorizontalRot_Bipodded_MultiplyAmount * orig.MaxHorizontalRot_Bipodded, orig.MaxHorizontalRot_Bipodded, "MaxHorizontalRot_Bipodded");
            tempRecoilProfile.RecoveryStabilizationFactors_Foregrip = orig.RecoveryStabilizationFactors_Foregrip * RecoveryStabilizationFactors_Foregrip_MultiplyAmount;
            tempRecoilProfile.RecoveryStabilizationFactors_Twohand = orig.RecoveryStabilizationFactors_Twohand * RecoveryStabilizationFactors_Twohand_MultiplyAmount;
            tempRecoilProfile.RecoveryStabilizationFactors_None = orig.RecoveryStabilizationFactors_None * RecoveryStabilizationFactors_None_MultiplyAmount;
            tempRecoilProfile.MassDriftIntensity = ValidateFloat(MassDriftIntensity_MultiplyAmount * orig.MassDriftIntensity, orig.MassDriftIntensity, "MassDriftIntensity");
            tempRecoilProfile.MassDriftFactors = orig.MassDriftFactors * MassDriftFactors_MultiplyAmount;
            tempRecoilProfile.MaxMassDriftMagnitude = ValidateFloatNoZero(MaxMassDriftMagnitude_MultiplyAmount * orig.MaxMassDriftMagnitude, orig.MaxMassDriftMagnitude, "MaxMassDriftMagnitude");
            tempRecoilProfile.MaxMassMaxRotation = ValidateFloatNoZero(MaxMassMaxRotation_MultiplyAmount * orig.MaxMassMaxRotation, orig.MaxMassMaxRotation, "MaxMassMaxRotation");
            tempRecoilProfile.MassDriftRecoveryFactor = ValidateFloat(MassDriftRecoveryFactor_MultiplyAmount * orig.MassDriftRecoveryFactor, orig.MassDriftRecoveryFactor, "MassDriftRecoveryFactor");
            return tempRecoilProfile;
        }
        private static float ValidateFloat(float value, float defaultValue, string paramName = "")
        {
            // 验证计算结果
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                Logger.LogWarning($"Invalid calculated value for {paramName}: {value}. Using default value {defaultValue}");
                return defaultValue;
            }
            return value;
        }
        private static float ValidateFloatNoZero(float value, float defaultValue, string paramName = "")
        {
            // 验证计算结果
            if (float.IsNaN(value) || float.IsInfinity(value) || value == 0)
            {
                Logger.LogWarning($"Invalid calculated value for {paramName}: {value}. Using default value {defaultValue}");
                return defaultValue;
            }
            return value;
        }
        private static T ApplyConfig<T>(T? configValue, T defaultValue, string paramName) where T : struct
        {
            if (configValue.HasValue)
            {
                Instance.LogDebugMessage($"  {paramName}: {configValue.Value}");
                return configValue.Value;
            }
            return defaultValue;
        }

        internal new static ManualLogSource Logger { get; private set; }
    }

    public class UniversalRecoilControlPatch
    {
        // 添加一个静态字典来存储每个武器的原始后坐力配置
        private static readonly Dictionary<string, FVRFireArmRecoilProfile> _originalRecoilProfiles = new Dictionary<string, FVRFireArmRecoilProfile>();
        private static readonly Dictionary<string, FVRFireArmRecoilProfile> _originalRecoilProfilesStocked = new Dictionary<string, FVRFireArmRecoilProfile>();

        [HarmonyPatch(typeof(FVRFireArm), "Awake")]
        [HarmonyPostfix]
        public static void AwakePatch(FVRFireArm __instance)
        {
            if (__instance.RecoilProfile != null)
            {
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile VertRotPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.VerticalRotPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile HorizontalRotPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.HorizontalRotPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile MaxVerticalRot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxVerticalRot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile MaxHorizontalRot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxHorizontalRot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile VerticalRotRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.VerticalRotRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile HorizontalRotRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.HorizontalRotRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile ZLinearPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.ZLinearPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile ZLinearMax:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.ZLinearMax}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile ZLinearRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.ZLinearRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile XYLinearPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.XYLinearPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile XYLinearMax:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.XYLinearMax}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile XYLinearRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.XYLinearRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile IsConstantRecoil:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.IsConstantRecoil}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile VerticalRotPerShot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.VerticalRotPerShot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile HorizontalRotPerShot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.HorizontalRotPerShot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile MaxVerticalRot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxVerticalRot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile MaxHorizontalRot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxHorizontalRot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile RecoveryStabilizationFactors_Foregrip:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.RecoveryStabilizationFactors_Foregrip}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile RecoveryStabilizationFactors_Twohand:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.RecoveryStabilizationFactors_Twohand}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile RecoveryStabilizationFactors_None:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.RecoveryStabilizationFactors_None}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile MassDriftIntensity:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MassDriftIntensity}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile MassDriftFactors:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MassDriftFactors}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile MaxMassDriftMagnitude:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxMassDriftMagnitude}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile MaxMassMaxRotation:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxMassMaxRotation}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile MassDriftRecoveryFactor:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MassDriftRecoveryFactor}");
                _originalRecoilProfiles[__instance.ObjectWrapper.ItemID] = __instance.RecoilProfile;
                __instance.RecoilProfile = UniversalRecoilControl.CopyAndAdjustRecoilProfile(__instance.RecoilProfile, __instance.ObjectWrapper.ItemID);
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile VertRotPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.VerticalRotPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile HorizontalRotPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.HorizontalRotPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile MaxVerticalRot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxVerticalRot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile MaxHorizontalRot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxHorizontalRot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile VerticalRotRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.VerticalRotRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile HorizontalRotRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.HorizontalRotRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile ZLinearPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.ZLinearPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile ZLinearMax:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.ZLinearMax}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile ZLinearRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.ZLinearRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile XYLinearPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.XYLinearPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile XYLinearMax:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.XYLinearMax}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile XYLinearRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.XYLinearRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile IsConstantRecoil:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.IsConstantRecoil}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile VerticalRotPerShot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.VerticalRotPerShot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile HorizontalRotPerShot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.HorizontalRotPerShot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile MaxVerticalRot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxVerticalRot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile MaxHorizontalRot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxHorizontalRot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile RecoveryStabilizationFactors_Foregrip:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.RecoveryStabilizationFactors_Foregrip}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile RecoveryStabilizationFactors_Twohand:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.RecoveryStabilizationFactors_Twohand}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile RecoveryStabilizationFactors_None:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.RecoveryStabilizationFactors_None}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile MassDriftIntensity:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MassDriftIntensity}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile MassDriftFactors:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MassDriftFactors}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile MaxMassDriftMagnitude:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxMassDriftMagnitude}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile MaxMassMaxRotation:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxMassMaxRotation}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile MassDriftRecoveryFactor:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MassDriftRecoveryFactor}");
            }
            if (__instance.RecoilProfileStocked != null)
            {
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked VertRotPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.VerticalRotPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked HorizontalRotPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.HorizontalRotPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked MaxVerticalRot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxVerticalRot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked MaxHorizontalRot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxHorizontalRot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked VerticalRotRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.VerticalRotRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked HorizontalRotRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.HorizontalRotRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked ZLinearPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.ZLinearPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked ZLinearMax:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.ZLinearMax}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked ZLinearRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.ZLinearRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked XYLinearPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.XYLinearPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked XYLinearMax:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.XYLinearMax}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked XYLinearRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.XYLinearRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked IsConstantRecoil:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.IsConstantRecoil}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked VerticalRotPerShot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.VerticalRotPerShot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked HorizontalRotPerShot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.HorizontalRotPerShot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked MaxVerticalRot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxVerticalRot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked MaxHorizontalRot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxHorizontalRot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked RecoveryStabilizationFactors_Foregrip:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.RecoveryStabilizationFactors_Foregrip}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked RecoveryStabilizationFactors_Twohand:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.RecoveryStabilizationFactors_Twohand}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked RecoveryStabilizationFactors_None:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.RecoveryStabilizationFactors_None}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked MassDriftIntensity:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MassDriftIntensity}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked MassDriftFactors:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MassDriftFactors}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked MaxMassDriftMagnitude:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxMassDriftMagnitude}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked MaxMassMaxRotation:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxMassMaxRotation}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Original recoil profile stocked MassDriftRecoveryFactor:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MassDriftRecoveryFactor}");
                _originalRecoilProfilesStocked[__instance.ObjectWrapper.ItemID] = __instance.RecoilProfileStocked;
                __instance.RecoilProfileStocked = UniversalRecoilControl.CopyAndAdjustRecoilProfile(__instance.RecoilProfileStocked, __instance.ObjectWrapper.ItemID);
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked VertRotPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.VerticalRotPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked HorizontalRotPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.HorizontalRotPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked MaxVerticalRot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxVerticalRot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked MaxHorizontalRot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxHorizontalRot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked VerticalRotRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.VerticalRotRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked HorizontalRotRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.HorizontalRotRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked ZLinearPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.ZLinearPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked ZLinearMax:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.ZLinearMax}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked ZLinearRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.ZLinearRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked XYLinearPerShot:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.XYLinearPerShot}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked XYLinearMax:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.XYLinearMax}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked XYLinearRecovery:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.XYLinearRecovery}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked IsConstantRecoil:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.IsConstantRecoil}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked VerticalRotPerShot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.VerticalRotPerShot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked HorizontalRotPerShot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.HorizontalRotPerShot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked MaxVerticalRot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxVerticalRot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked MaxHorizontalRot_Bipodded:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxHorizontalRot_Bipodded}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked RecoveryStabilizationFactors_Foregrip:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.RecoveryStabilizationFactors_Foregrip}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked RecoveryStabilizationFactors_Twohand:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.RecoveryStabilizationFactors_Twohand}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked RecoveryStabilizationFactors_None:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.RecoveryStabilizationFactors_None}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked MassDriftIntensity:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MassDriftIntensity}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked MassDriftFactors:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MassDriftFactors}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked MaxMassDriftMagnitude:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxMassDriftMagnitude}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked MaxMassMaxRotation:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MaxMassMaxRotation}");
                UniversalRecoilControl.Instance.LogDebugMessage($"Modified recoil profile stocked MassDriftRecoveryFactor:{__instance.ObjectWrapper.ItemID} {__instance.RecoilProfile.MassDriftRecoveryFactor}");
            }
        }

        [HarmonyPatch(typeof(SteamVR_LoadLevel), "Begin")]
        [HarmonyPrefix]
        public static bool BeginPatch()
        {
            _originalRecoilProfiles.Clear();
            _originalRecoilProfilesStocked.Clear();
            // 重新加载配置
            UniversalRecoilControl.Instance.LoadJsonConfig();
            UniversalRecoilControl.Instance.Config.Reload();
            return true;
        }
    }
}