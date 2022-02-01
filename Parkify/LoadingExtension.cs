using System.Linq;
using CitiesHarmony.API;
using ICities;
using Parkify.HarmonyPatches.BuildingInfoPatch;
using Parkify.OptionsFramework;
using UnityEngine;

namespace Parkify
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private static bool initialized;


        //TODO(bloodypenguin): add option to remove concrete and parking slots

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }

            InitializePrefabPatch.Apply();
        }

        public override void OnReleased()
        {
            base.OnReleased();
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }

            InitializePrefabPatch.Undo();
            initialized = false;
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            if (mode == LoadMode.LoadAsset || mode == LoadMode.NewAsset)
            {
                return;
            }

            if (initialized)
            {
                return;
            }

            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }

            if (OptionsWrapper<Options>.Options.PatchMarina)
            {
                PatchMarinaProps();
            }

            if (OptionsWrapper<Options>.Options.PatchFishingTours)
            {
                PatchFishingToursProps();
            }

            initialized = true;
        }

        private static void PatchFishingToursProps()
        {
            var buildingInfo = PrefabCollection<BuildingInfo>.FindLoaded("3x2_Fishing tours");
            if (buildingInfo?.m_props == null)
            {
                return;
            }

            var list = buildingInfo.m_props.ToList();
            var propInfo = PrefabCollection<PropInfo>.FindLoaded("Yacht");
            if (propInfo == null)
            {
                return;
            }

            var prop = new BuildingInfo.Prop
            {
                m_angle = 270,
                m_probability = 100,
                m_finalProp = propInfo,
                m_position = new Vector3(0, 0, -14),
                m_prop = propInfo,
                m_radAngle = 4.71239f,
                m_fixedHeight = true,
                m_index = 0,
                m_requiredLength = 0
            };
            list.Add(prop);
            buildingInfo.m_props = list.ToArray();
        }

        private static void PatchMarinaProps()
        {
            var buildingInfo = PrefabCollection<BuildingInfo>.FindLoaded("4x4_Marina");
            if (buildingInfo?.m_props == null)
            {
                return;
            }

            var propInfo = PrefabCollection<PropInfo>.FindLoaded("houseboat");
            if (propInfo == null)
            {
                return;
            }

            buildingInfo.m_props[15].m_prop = propInfo;
            buildingInfo.m_props[15].m_finalProp = propInfo;
            buildingInfo.m_props[15].m_position = new Vector3(-13, 0, 3.5f);
            buildingInfo.m_props[15].m_radAngle = 1.5708f;
            buildingInfo.m_props[15].m_angle = 90;
        }
    }
}