using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMLHelper;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace Turrets
{
    public class TurretPrefab : ModPrefab
    {
        private AssetBundle assetBundle;
        private GameObject prefab;

        public TurretPrefab() : base(
            "Turret", 
            "WorldEntities/Tools/Turret", 
            TechTypeHandler.AddTechType("Turret", "Turret", "Shoots stuff."))
        {
            CraftDataHandler.AddBuildable(TechType);
            CraftDataHandler.AddToGroup(TechGroup.ExteriorModules, TechCategory.ExteriorModule, TechType);

            var recipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Titanium, 2)
                }
            };

            CraftDataHandler.SetTechData(TechType, recipe);
        }

        public override GameObject GetGameObject()
        {
            if(assetBundle == null || prefab == null)
            {
                assetBundle = AssetBundle.LoadFromFile("./QMods/Turrets/turret.assets");

                prefab = assetBundle.LoadAsset<GameObject>("Turret Prefab");

                // Set rendering stuff
                var renderers = prefab.GetComponentsInChildren<Renderer>();
                var shader = Shader.Find("MarmosetUBER");

                foreach (var renderer in renderers)
                {
                    renderer.material.shader = shader;
                }

                var skyApplier = prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;

                // Make it a constructable
                var constructable = prefab.AddComponent<Constructable>();
                constructable.allowedInBase = false;
                constructable.allowedInSub = false;
                constructable.allowedOutside = true;
                constructable.allowedOnConstructables = false;
                constructable.allowedOnWall = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedOnGround = true;
                constructable.techType = TechType;
                constructable.model = prefab.FindChild("Base");

                var bounds = prefab.AddComponent<ConstructableBounds>();

                // Add turret monobehavior
                var turret = prefab.AddComponent<Turret>();
                turret.beamPrefab = assetBundle.LoadAsset<GameObject>("Beam");
            }

            return prefab;
        }
    }
}
