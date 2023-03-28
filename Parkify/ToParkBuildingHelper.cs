using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Parkify.OptionsFramework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Parkify;

public static class ToParkBuildingHelper
{
    private static List<string> monumentExcludeList = new List<string>
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
        "Central Park", //TODO(bloodypenguin): make possible to use as the main gate? make sub buildings parks too
        "Bird and Bee Haven",
        "Igloo Hotel",
        "Ice_Fishing_Pond",
        "Sleigh Ride",
        "Sphinx Of Scenarios",
        "Disaster Memorial",
        "Frozen Fountain",
        "PDX17_Five Story Pagora",//Temple
        "Pyramid of Safety",
        "The Statue Of Colossalus"
        "Ski Resort Building",
        "Snowcastle Restaurant",
        "Chirps Thumbs Up Plaza",
        "Korean Style Temple",
        "Bronze Cow",
        "Bronze Panda",
        "Financial Plaza 01",//Underground Garden Plaza
        "Financial Plaza 02",//Elevated Plaza
    };


    public static bool ShouldPatchMonument(BuildingInfo prefabInfo)
    {
        if (!OptionsWrapper<Options>.Options.PatchVanillaUniqueBuildings)
        {
            return false;
        }

        return prefabInfo.m_buildingAI is MonumentAI && monumentExcludeList.Contains(prefabInfo.name);
    }

    private static DistrictPark.ParkType GetParkType(string prefabInfoName)
    {
        //TODO(bloodypenguin): load type for workshop assets
        switch (prefabInfoName)
        {
            case "Zoo":
            case "Panda Sanctuary":
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

    public static void ParkifyIfNeeded(BuildingInfo prefabInfo)
    {
        if (prefabInfo.m_placementStyle == ItemClass.Placement.Procedural)
        {
            return;
        }

        if (!(prefabInfo.m_buildingAI?.GetType() == typeof(ParkAI) ||
              prefabInfo.m_buildingAI is MonumentAI) ||
            prefabInfo.name == null)
        {
            return;
        }

        if (prefabInfo.m_buildingAI is MonumentAI && !ShouldPatchMonument(prefabInfo))
        {
            return;
        }

        var parkType = GetParkType(prefabInfo.name);
        ToParkBuildingInfo(prefabInfo, parkType);
        UnityEngine.Debug.Log($"Parkify - {prefabInfo?.name} was successfully parkified");
    }


    private static void ToParkBuildingInfo(BuildingInfo prefabInfo, DistrictPark.ParkType parkType)
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

    private static void ToDecoration(BuildingInfo prefabInfo, bool allowOverlap)
    {
        var newAI = ReplaceAI<DecorationBuildingAI>(prefabInfo);
        if (newAI == null)
        {
            return;
        }

        newAI.m_allowOverlap = allowOverlap;
    }

    private static void ToDummy(BuildingInfo prefabInfo)
    {
        ReplaceAI<DummyBuildingAI>(prefabInfo);
    }

    private static T ReplaceAI<T>(BuildingInfo prefabInfo) where T : BuildingAI
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

    private static void CopyAttributes(PrefabAI src, PrefabAI dst)
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
