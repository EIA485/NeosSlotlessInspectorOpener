using BepInEx;
using BepInEx.NET.Common;
using BepInExResoniteShim;
using HarmonyLib;
using FrooxEngine;

namespace SlotlessInspectorOpener
{
    [ResonitePlugin(PluginMetadata.GUID, PluginMetadata.NAME, PluginMetadata.VERSION, PluginMetadata.AUTHORS, PluginMetadata.REPOSITORY_URL)]
    [BepInDependency(BepInExResoniteShim.PluginMetadata.GUID)]
    public class SlotlessInspectorOpener : BasePlugin
    {
        public override void Load() => HarmonyInstance.PatchAll();

        [HarmonyPatch(typeof(InspectorHelper), nameof(InspectorHelper.OpenInspectorForTarget))]
        class SlotlessInspectorOpenerPatch
        {
            static void Prefix(ref IWorldElement target, ref bool openWorkerOnly)
            {
                if (!openWorkerOnly && target.FindNearestParent<Slot>() == null && target.FindNearestParent<User>() == null)
                {
                    openWorkerOnly = true;
                    if (target.Parent?.FindNearestParent<Worker>() != null)
                        target = target.Parent;
                }
            }
        }
    }
}