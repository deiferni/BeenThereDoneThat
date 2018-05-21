using System.Collections.Generic;
using UnityEngine;

namespace BeenThereDoneThat
{
    [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
    public class PayloadSeparatorPart : PartModule
    {
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true)]
        public bool isPayloadSeparator;

        [KSPEvent(guiActive = true, guiName = "BeenThereDoneThat: Set as payload separator")]
        public void SetPayloadSeparator()
        {
            SetActive(vessel.parts);
        }

        [KSPEvent(guiActiveEditor = true, guiName = "BeenThereDoneThat: Set as payload separator")]
        public void SetEditorPayloadSeparator()
        {
            SetActive(EditorLogic.SortedShipList);
        }

        [KSPEvent(guiActiveEditor = true, guiName = "Debug: launch vehicle/payload parts")]
        public void DebugParentsChildrenEditor()
        {
            LaunchVehicle launchVehicle = null;
            Payload payload = null;
            QuickLauncher.Instance.Split(EditorLogic.SortedShipList, out launchVehicle, out payload);

            payload.DebugParts();
            launchVehicle.DebugParts();
        }

        [KSPEvent(guiActive = true, guiName = "Debug: launch vehicle/payload parts")]
        public void DebugParentsChildrenFlight()
        {
            LaunchVehicle launchVehicle = null;
            Payload payload = null;
            QuickLauncher.Instance.Split(FlightGlobals.ActiveVessel.parts, out launchVehicle, out payload);

            payload.DebugParts();
            launchVehicle.DebugParts();
        }

        private void SetActive(List<Part> parts)
        {
            foreach (Part part in parts)
            {
                foreach (PayloadSeparatorPart module in part.FindModulesImplementing<PayloadSeparatorPart>())
                {
                    if (module.isPayloadSeparator)
                    {
                        Debug.Log(string.Format("[BeenThereDoneThat]: Disabling payload separtor on {0}:{1}", part.name, module.name));
                        module.isPayloadSeparator = false;
                    }
                }
            }
            isPayloadSeparator = true;
        }

        [KSPEvent(guiActive = true, guiName = "BeenThereDoneThat: start mission")]
        public void StartMission()
        {
            PrepareVessel();
            QuickLaunchHangar.Instance.OnStartMission(vessel);
        }

        [KSPEvent(guiActive = true, guiName = "BeenThereDoneThat: end mission")]
        public void RememberOrbit()
        {
            PrepareVessel();
            QuickLaunchMissionTracker tracker = vessel.GetComponent<QuickLaunchMissionTracker>();
            if (!tracker.isTracking)
            {
                Debug.Log("[BeenThereDoneThat]: Not tracking, aborting");
                return;
            }

            QuickLaunchHangar.Instance.OnSaveOrbitVessel(vessel, tracker.launchVehicleName);
        }

        [KSPEvent(guiActive = true, guiName = "BeenThereDoneThat: re-run mission")]
        public void ReRunMission()
        {
            PrepareVessel();
            QuickLaunchHangar.Instance.OnQuickLaunchMission(vessel);
        }

        private void PrepareVessel()
        {
            vessel.BackupVessel();
        }
    }
}
