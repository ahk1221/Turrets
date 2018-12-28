using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMLHelper.V2.Handlers;
using Harmony;
using System.Reflection;
using UnityEngine;

namespace Turrets
{
    public class Main
    {
        public static TurretPrefab prefab;

        public static void Patch()
        {
            prefab = new TurretPrefab();
            PrefabHandler.RegisterPrefab(prefab);

            HarmonyInstance.Create("com.ahk1221.turrets").PatchAll(Assembly.GetExecutingAssembly());

            Console.WriteLine("[Turrets] Succesfully patched!");
        }
    }

    [HarmonyPatch(typeof(Player), "Update")]
    public class Player_Update_Patch
    {
        static void Prefix()
        {
            if(Input.GetKeyDown(KeyCode.K))
            {
                GameObject go = GameObject.Instantiate(Main.prefab.GetGameObject());
                //go.GetComponent<Rigidbody>().isKinematic = true;

                go.transform.position = Player.main.transform.position;
            }
        }
    }
}
