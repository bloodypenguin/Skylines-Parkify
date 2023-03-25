using System;
using System.Linq;

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
            try
            {
                FixParkBuildingPlacementModeIfNeeded(__instance);
                ToParkBuildingHelper.ParkifyIfNeeded(__instance);
                if (__instance?.name == "Panda Sanctuary")
                {
                    PatchMonumentSurfaceAndProps(__instance, TerrainModify.Surface.Gravel);
                    return;
                }

                if (__instance?.name == "Zoo")
                {
                    PatchMonumentSurfaceAndProps(__instance, TerrainModify.Surface.None);
                    return;
                }

                if (__instance?.name != "Beachvolley Court" || __instance.m_props == null)
                {
                    return;
                }

                PatchBeachvolleyCourtSurfaceAndProps(__instance);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Parkify - failed to execute post initialize for {__instance?.name}");
                UnityEngine.Debug.LogException(e);
            }
        }

        private static void FixParkBuildingPlacementModeIfNeeded(BuildingInfo prefabInfo)
        {
            if (prefabInfo.m_buildingAI is not ParkBuildingAI ||
                prefabInfo.m_placementStyle == ItemClass.Placement.Procedural ||
                prefabInfo.m_placementMode != BuildingInfo.PlacementMode.Roadside)
            {
                return;
            }
            UnityEngine.Debug.LogError($"Parkify - updated placement mode for park building {prefabInfo?.name}");
            prefabInfo.m_placementMode = BuildingInfo.PlacementMode.PathsideOrGround;
        }

        private static void PatchMonumentSurfaceAndProps(BuildingInfo buildingInfo, TerrainModify.Surface pavementReplacement)
        {
            buildingInfo.m_cellSurfaces = buildingInfo.m_cellSurfaces.Select(surface =>
            {
                if (surface == TerrainModify.Surface.PavementB)
                {
                    return pavementReplacement;
                }

                return surface;
            }).ToArray();
            if (buildingInfo.m_props != null)
            {
                buildingInfo.m_props = buildingInfo.m_props
                    .Where(p => p?.m_prop?.name == null || !p.m_prop.name.Contains("Parking Spaces"))
                    .ToArray();
            }
            buildingInfo.m_hasParkingSpaces &= ~VehicleInfo.VehicleType.Car;
        }

        private static void PatchBeachvolleyCourtSurfaceAndProps(BuildingInfo buildingInfo)
        {
            buildingInfo.m_cellSurfaces = new TerrainModify.Surface[]
            {
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.Gravel,
                TerrainModify.Surface.Gravel,
                TerrainModify.Surface.None,
                TerrainModify.Surface.None,
                TerrainModify.Surface.Gravel,
                TerrainModify.Surface.Gravel,
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