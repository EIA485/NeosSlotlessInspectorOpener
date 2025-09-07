using BepInEx;
using BepInEx.NET.Common;
using BepInEx.Configuration;
using BepInExResoniteShim;
using FrooxEngine;
using FrooxEngine.UIX;
using Elements.Core;
using HarmonyLib;
using BepInEx.Logging;
using HarmonyLib.Tools;

namespace SlotlessInspectorOpener
{
	[ResonitePlugin(PluginMetadata.GUID, PluginMetadata.NAME, PluginMetadata.VERSION, PluginMetadata.AUTHORS, PluginMetadata.REPOSITORY_URL)]
	[BepInDependency(BepInExResoniteShim.PluginMetadata.GUID)]
	public class SlotlessInspectorOpener : BasePlugin
	{
		static ConfigEntry<bool> DisableButtonInjection;
		public override void Load()
		{
			DisableButtonInjection = Config.Bind(PluginMetadata.NAME, "DisableButtonInjection", false, "Disables injecting show parent buttons into now valid Worker Inspectors");
			HarmonyInstance.PatchAll();
		}
		[HarmonyPatch]
		class SlotlessInspectorOpenerPatch
		{
			[HarmonyPrefix]
			[HarmonyPatch(typeof(InspectorHelper), nameof(InspectorHelper.OpenInspectorForTarget))]

			static void OpenInspectorForTargetPrefix(ref IWorldElement target, ref bool openWorkerOnly)
			{
				if (!openWorkerOnly && target.FindNearestParent<Slot>() == null && target.FindNearestParent<User>() == null)
				{
					openWorkerOnly = true;
					if (target.Parent?.FindNearestParent<Worker>() is Worker parent)
					{
						target = parent;
					}
				}
			}

			//show button when valid, similar to https://github.com/art0007i/ShowComponentSlot before it was archived
			[HarmonyPostfix]
			[HarmonyPatch(typeof(WorkerInspector), "BuildUIForComponent")]
			static void BuildUIForComponentPostfix(WorkerInspector __instance, Worker worker, bool allowContainer)
			{
				if (DisableButtonInjection.Value || !allowContainer || worker.Parent == null || worker.Parent.FindNearestParent<Worker>() == null|| worker.FindNearestParent<Slot>() != null) return;
				UIBuilder ui = new UIBuilder(__instance.Slot[0][0]);
				RadiantUI_Constants.SetupEditorStyle(ui);
				ui.Style.MinHeight = 24f;
				ui.Style.FlexibleWidth = 0f;
				ui.Style.MinWidth = 40f;
			
				var button = ui.Button(OfficialAssets.Graphics.Icons.Inspector.RootUp, RadiantUI_Constants.Sub.PURPLE);
				var edit = button.Slot[0].AttachComponent<RefEditor>();
				(AccessTools.Field(edit.GetType(), "_targetRef").GetValue(edit) as RelayRef<ISyncRef>).Target = (ISyncRef)AccessTools.Field(__instance.GetType(), "_targetWorker").GetValue(__instance);
				button.Pressed.Target = (ButtonEventHandler)AccessTools.Method(edit.GetType(), "OpenInspectorButton").CreateDelegate(typeof(ButtonEventHandler), edit);
			}
		}
	}
}