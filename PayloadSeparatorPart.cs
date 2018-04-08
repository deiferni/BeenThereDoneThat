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
        public void DebugParentsChildren()
        {
            LaunchVehicle launchVehicle = null;
            Payload payload = null;
            QuickLauncher.Instance.Split(EditorLogic.SortedShipList, out launchVehicle, out payload);

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
            vessel.BackupVessel();

            QuickLaunchMissionTracker tracker = vessel.GetComponent<QuickLaunchMissionTracker>();
            string launchVehicleName = vessel.name;
            LaunchVehicle launchVehicle = null;
            Payload payload = null;

            if (tracker.isTracking)
            {
                Debug.Log("[BeenThereDoneThat]: Already tracking");
                return;
            }

            if (!QuickLauncher.Instance.Split(vessel.parts, out launchVehicle, out payload))
            {
                return;
            }

            launchVehicle.SaveAsSubmodule(launchVehicleName);
            QuickLaunchHangar.Instance.SaveLaunchVessel(vessel, launchVehicleName);
            tracker.startTracking(launchVehicleName);
        }

        [KSPEvent(guiActive = true, guiName = "BeenThereDoneThat: end mission")]
        public void RememberOrbit()
        {
            vessel.BackupVessel();
            QuickLaunchMissionTracker tracker = vessel.GetComponent<QuickLaunchMissionTracker>();
            if (!tracker.isTracking)
            {
                Debug.Log("[BeenThereDoneThat]: Not tracking, aborting");
                return;
            }

            QuickLaunchHangar.Instance.SaveOrbitVessel(vessel, tracker.launchVehicleName);
        }

        [KSPEvent(guiActive = true, guiName = "BeenThereDoneThat: re-run mission")]
        public void ReRunMission()
        {
            ProtoVessel prevlaunchProtoVessel = QuickLaunchHangar.Instance.LoadLaunchProtoVessel(vessel);
            ProtoVessel orbitProtoVessel = QuickLaunchHangar.Instance.LoadOrbitProtoVessel(vessel);

            if (prevlaunchProtoVessel == null)
            {
                Debug.Log("[BeenThereDoneThat]: No previously launched vessel found, aborting");
                return;
            }

            if (orbitProtoVessel == null)
            {
                Debug.Log("[BeenThereDoneThat]: No orbit vessel found, aborting");
                return;
            }

            new QuickLauch(vessel, prevlaunchProtoVessel, orbitProtoVessel).Liftoff();
        }
    }
}
