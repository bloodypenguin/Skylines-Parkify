using System;
using System.Linq;
using Parkify.OptionsFramework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Parkify.HarmonyPatches.BuildingInfoPatch
{
    public static class InitializePrefabPatch
    {
        private static bool deployed;

        public static void Apply()
        {
            if (deployed)
            {
                return;
            }

            PatchUtil.Patch(
                new PatchUtil.MethodDefinition(typeof(BuildingInfo),
                    nameof(BuildingInfo.InitializePrefab)),
                null,
                new PatchUtil.MethodDefinition(typeof(InitializePrefabPatch), nameof(PostInitializePrefab)));

            deployed = true;
        }

        public static void Undo()
        {
            if (!deployed)
            {
                return;
            }

            PatchUtil.Unpatch(
                new PatchUtil.MethodDefinition(typeof(BuildingInfo),
                    nameof(BuildingInfo.InitializePrefab)));

            deployed = false;
        }

        private static void PostInitializePrefab(BuildingInfo __instance)
        {
            if (__instance?.name != "Beachvolley Court" || __instance.m_props == null)
            {
                return;   
            }
            PatchBeachvolleyCourt(__instance);
        }
        
        private static void PatchBeachvolleyCourt(BuildingInfo buildingInfo)
        {
            buildingInfo.m_cellSurfaces = new TerrainModify.Surface[]
            {
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
            };

            var list = buildingInfo.m_props.ToList();
            buildingInfo.m_props = list
                .Where(p =>
                    !p.m_prop.name.Contains("Parking Spaces") && 
                    !p.m_prop.name.Contains("StreetLamp") &&
                    !p.m_prop.name.Contains("Billboard") &&
                    !p.m_prop.name.Contains("kiosk") &&
                    !p.m_prop.name.Contains("beergarden"))
                .ToArray();
            buildingInfo.m_hasParkingSpaces = VehicleInfo.VehicleType.None;
            buildingInfo.m_weakTerrainRuining = true;
        }
    }
}