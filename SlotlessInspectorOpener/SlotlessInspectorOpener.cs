using NeosModLoader;
using HarmonyLib;
using FrooxEngine;

namespace SlotlessInspectorOpener
{
    public class SlotlessInspectorOpener : NeosMod
    {
        public override string Name => "SlotlessInspectorOpener";
        public override string Author => "eia485";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/EIA485/NeosSlotlessInspectorOpener";
        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("net.eia485.SlotlessInspectorOpener");
            harmony.PatchAll();
        }

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