using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ICities;
using Parkify.OptionsFramework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Parkify
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private static bool initialized;

        private static List<string> monumentWhitelist = new List<string>
        {
            "Official Park",
            "Fancy Fountain",
            "Traffic Park",
            "Central Park",
            "Panda Sanctuary",
            "Zoo",
            "Statue of Industry",
            "StatueOfWealth",
            "Lazaret Plaza",
            "Statue of Shopping",
            "Plaza of the Dead",
            "Meteorite Park",
            "Fountain of LifeDeath",
            "Friendly Neighborhood",
            "Helicopter Park",
            "Lungs of the City",
            "Sparkly Unicorn Rainbow Park",
            "Central Park", //TODO(earalov): make possible to use as the main gate? make sub buildings parks too
            "Bird and Bee Haven",
            "Igloo Hotel",
            "Ice_Fishing_Pond",
            "Sleigh Ride",
            "Sphinx Of Scenarios",
            "Disaster Memorial",
            "Frozen Fountain"
        };

        //TODO(earalov): add option to remove concrete and parking slots

        public override void OnReleased()
        {
            base.OnReleased();
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

            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
            {
                var prefabInfo = PrefabCollection<BuildingInfo>.GetLoaded(i);
                if (prefabInfo == null || prefabInfo.name == null)
                {
                    continue;
                }
                if (!(prefabInfo.m_buildingAI?.GetType() == typeof(ParkAI) || prefabInfo.m_buildingAI is MonumentAI) ||
                    prefabInfo.name == null)
                {
                    continue;
                }

                if (prefabInfo.m_buildingAI is MonumentAI)
                {
                    if (!OptionsWrapper<Options>.Options.PatchVanillaUniqueBuildings ||
                        !monumentWhitelist.Contains(prefabInfo.name))
                    {
                        continue;
                    }
                }


                var parkType = GetParkType(prefabInfo.name);
                ToParkBuildingInfo(prefabInfo, parkType);
            }

            if (OptionsWrapper<Options>.Options.PatchMarina)
            {
                PatchMarina();
            }

            if (OptionsWrapper<Options>.Options.PatchFishingTours)
            {
                PatchFishingTours();
            }

            initialized = true;
        }

        private DistrictPark.ParkType GetParkType(string prefabInfoName)
        {
            //TODO(earalov): load type for workshop assets
            switch (prefabInfoName)
            {
                case "Zoo": //TODO(earalov): make possible to use as zoo main gate
                case "Panda Sanctuary": //TODO(earalov): make possible to use as zoo main gate
                    return DistrictPark.ParkType.Zoo;
                case "bouncer_castle":
                case "MerryGoRound":
                case "MagickaPark":
                case "Traffic Park":
                case "Sphinx Of Scenarios":
                    return DistrictPark.ParkType.AmusementPark;
                case "9x15_RidingStable":
                case "Lungs of the City":
                case "Bird and Bee Haven":
                case "Public Firepit":
                case "Igloo Hotel":
                case "Sleigh Ride":
                    return DistrictPark.ParkType.NatureReserve;
            }

            return DistrictPark.ParkType.Generic;
        }

        private void ToParkBuildingInfo(BuildingInfo prefabInfo, DistrictPark.ParkType parkType)
        {
            var newAI = ReplaceAI<ParkBuildingAI>(prefabInfo);
            if (newAI == null)
            {
                return;
            }

            if (prefabInfo.m_placementMode == BuildingInfo.PlacementMode.Roadside)
            {
                prefabInfo.m_placementMode = BuildingInfo.PlacementMode.PathsideOrGround;
            }

            newAI.m_parkType = parkType;
        }

        private void ToDecoration(BuildingInfo prefabInfo, bool allowOverlap)
        {
            var newAI = ReplaceAI<DecorationBuildingAI>(prefabInfo);
            if (newAI == null)
            {
                return;
            }

            newAI.m_allowOverlap = allowOverlap;
        }

        private void ToDummy(BuildingInfo prefabInfo)
        {
            ReplaceAI<DummyBuildingAI>(prefabInfo);
        }

        private static void PatchFishingTours()
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

        private static void PatchMarina()
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

        private T ReplaceAI<T>(BuildingInfo prefabInfo) where T : BuildingAI
        {
            var oldAI = prefabInfo.m_buildingAI;
            if (oldAI == null)
            {
                return null;
            }

            Object.Destroy(oldAI);
            var newAI = prefabInfo.gameObject.AddComponent<T>();
            CopyAttributes(oldAI, newAI);
            newAI.m_info = prefabInfo;
            prefabInfo.m_buildingAI = newAI;
            return newAI;
        }

        private void CopyAttributes(PrefabAI src, PrefabAI dst)
        {
            var oldAIFields = src.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                           BindingFlags.FlattenHierarchy);
            var newAIFields = dst.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                           BindingFlags.FlattenHierarchy);

            var newAIFieldDic = new Dictionary<string, FieldInfo>(newAIFields.Length);
            foreach (var field in newAIFields)
            {
                newAIFieldDic.Add(field.Name, field);
            }

            foreach (var fieldInfo in oldAIFields)
            {
                newAIFieldDic.TryGetValue(fieldInfo.Name, out FieldInfo newAIField);
                try
                {
                    if (newAIField != null && newAIField.GetType() == fieldInfo.GetType())
                    {
                        newAIField.SetValue(dst, fieldInfo.GetValue(src));
                    }
                }
                catch (NullReferenceException)
                {
                }
            }
        }
    }
}