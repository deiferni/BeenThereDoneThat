using System;
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
        public void SaveLaunchVehivle()
        {
            LaunchVehicle launchVehicle = null;
            Payload payload = null;
            if (!QuickLauncher.Instance.Split(vessel.parts, out launchVehicle, out payload))
            {
                return;
            }

            launchVehicle.SaveAsSubmodule();
            QuickLauncher.Instance.RememberVessel("VesselLaunch.craft");
        }

        [KSPEvent(guiActive = true, guiName = "BeenThereDoneThat: end mission")]
        public void RememberOrbit()
        {
            vessel.BackupVessel();
            QuickLauncher.Instance.RememberVessel("VesselOrbit.craft");
        }

        [KSPEvent(guiActive = true, guiName = "BeenThereDoneThat: re-run mission")]
        public void ReRunMission()
        {
            LaunchVehicle launchVehicle = null;
            Payload payload = null;
            if (!QuickLauncher.Instance.Split(vessel.parts, out launchVehicle, out payload))
            {
                return;
            }

            // check if we can do a re-run
            if (!IsPreviouslyUsedLaunchVessel(launchVehicle))
            {
                Debug.Log("[BeenThereDoneThat]: Can't re-run mission: launch vessel not equal to previous run");
                return;
            }

            // find orbit information and restore orbit
            int orbitSeparationIndex = -1;
            string text = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/BeenThereDoneThat/";
            string text2 = text + "VesselOrbit.craft";
            ConfigNode subModuleRootNode = ConfigNode.Load(text2);

            ProtoVessel protoVessel = new ProtoVessel(subModuleRootNode, HighLogic.CurrentGame);
            OrbitSnapshot protoOrbit = protoVessel.orbitSnapShot;

            int selBodyIndex = protoOrbit.ReferenceBodyIndex;
            double sma = protoOrbit.semiMajorAxis;
            double ecc = protoOrbit.eccentricity;
            double inc = protoOrbit.inclination;
            double LAN = protoOrbit.LAN;
            double mna = protoOrbit.meanAnomalyAtEpoch;
            double argPe = protoOrbit.argOfPeriapsis;
            double epoch = protoOrbit.epoch;

            Debug.Log(
                string.Format("[BeenThereDoneThat]: RESTORING ORBIT> sma: {0} ecc: {1} inc: {2} LAN: {3} mna: {4} argPe: {5} epoch: {6}",
                              sma, ecc, inc, LAN, mna, argPe, epoch));

            // hackishly temporarily set planetarium time 
            double prevTime = Planetarium.fetch.time;
            Planetarium.fetch.time = epoch;
            FlightGlobals.fetch.SetShipOrbit(selBodyIndex, ecc, sma, inc, LAN, argPe, mna, 0);
            // hackishly restore planetarium time
            Planetarium.fetch.time = prevTime;
            Debug.Log("[BeenThereDoneThat]: Put ship into orbit");

            ProtoLaunchVehicle protoLaunchVehicle = null;
            ProtoPayload protoPayload = null;
            if (!QuickLauncher.Instance.Split(protoVessel, out protoLaunchVehicle, out protoPayload))
            {
                return;
            }

            // restore resource levels
            for (int i = 0; i < protoLaunchVehicle.parts.Count; i++)
            {
                ProtoPartSnapshot protoPart = protoLaunchVehicle.parts[i];
                Part vesselPart = launchVehicle.parts[i];
                foreach (ProtoPartResourceSnapshot protoResource in protoPart.resources)
                {
                    string resourceName = protoResource.resourceName;
                    double amount = protoResource.amount;
                    Debug.Log(string.Format("[BeenThereDoneThat]: Found resource {0}: amount: {1}", resourceName, amount));
                    PartResource partResource = vesselPart.Resources.Get(resourceName);
                    partResource.amount = amount;
                    GameEvents.onPartResourceListChange.Fire(vesselPart);
                }
                orbitSeparationIndex = Math.Max(orbitSeparationIndex, protoPart.separationIndex);
            }
            Debug.Log("[BeenThereDoneThat]:  Restored resources");

            // removing burned stages/parts
            if (protoLaunchVehicle.parts.Count < launchVehicle.parts.Count)
            {
                int diff = launchVehicle.parts.Count - protoLaunchVehicle.parts.Count;
                int count = 0;
                List<Part> toDie = new List<Part>();
                Debug.Log(string.Format("[BeenThereDoneThat]: Removing {0} burned parts below stage {1}", diff, orbitSeparationIndex));

                foreach (Part vesselPart in launchVehicle.parts)
                {
                    if (vesselPart.separationIndex > orbitSeparationIndex)
                    {
                        Debug.Log(string.Format("[BeenThereDoneThat]: Removing part {0}", vesselPart.name));
                        toDie.Add(vesselPart);
                        count++;
                    }
                }
                if (count != diff)
                {
                    Debug.Log(string.Format("[BeenThereDoneThat]: Expected to remove {0} but got {1} parts", diff, count));
                }
                else
                {
                    foreach (Part spent in toDie)
                    {
                        spent.Die();
                    }
                }
            }
            else
            {
                Debug.Log("[BeenThereDoneThat]: No parts to remove");
            }
            Debug.Log("[BeenThereDoneThat]: Wohoooo done!");
        }

        public bool IsPreviouslyUsedLaunchVessel(LaunchVehicle launchVehicle)
        {
            // Find proto parts and resouces
            string text = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/BeenThereDoneThat/";
            string text2 = text + "VesselLaunch.craft";

            List<AvailablePart> protoPartInfos = new List<AvailablePart>();
            ConfigNode subModuleRootNode = ConfigNode.Load(text2);
            ProtoVessel protoVessel = new ProtoVessel(subModuleRootNode, HighLogic.CurrentGame);

            ProtoLaunchVehicle protoLaunchVehicle = null;
            ProtoPayload protoPayload = null;
            if (!QuickLauncher.Instance.Split(protoVessel, out protoLaunchVehicle, out protoPayload))
            {
                return false;
            }

            Dictionary<string, double> protoResources = new Dictionary<string, double>();
            protoLaunchVehicle.DebugParts();
            foreach (ProtoPartSnapshot protoPart in protoLaunchVehicle.parts)
            {
                foreach (ProtoPartResourceSnapshot protoResource in protoPart.resources)
                {
                    string resourceName = protoResource.resourceName;
                    double amount = protoResource.amount;
                    Debug.Log(string.Format("[BeenThereDoneThat]: Found resource {0}: amount: {1}", resourceName, amount));

                    if (!protoResources.ContainsKey(resourceName))
                    {
                        protoResources[resourceName] = 0;
                    }
                    protoResources[resourceName] += amount;
                }
                protoPartInfos.Add(protoPart.partInfo);
            }
            foreach (string resourceKey in protoResources.Keys)
            {
                Debug.Log(string.Format("[BeenThereDoneThat]: Total resource {0}: amount: {1}", resourceKey, protoResources[resourceKey]));
            }

            List<AvailablePart> partInfos = new List<AvailablePart>();
            Dictionary<string, double> resources = new Dictionary<string, double>();
            launchVehicle.DebugParts();
            foreach (Part part in launchVehicle.parts)
            {
                partInfos.Add(part.partInfo);
                foreach (PartResource partResource in part.Resources)
                {
                    if (!resources.ContainsKey(partResource.resourceName))
                    {
                        resources[partResource.resourceName] = 0;
                    }
                    resources[partResource.resourceName] += partResource.amount;
                }
            }
            foreach (string resourceKey in resources.Keys)
            {
                Debug.Log(string.Format("[BeenThereDoneThat]: Total resource {0}: amount: {1}", resourceKey, resources[resourceKey]));
            }

            // Compare parts by name and order
            Debug.Log(string.Format("[BeenThereDoneThat]: vessel parts {0}", partInfos.Count));
            Debug.Log(string.Format("[BeenThereDoneThat]: proto vessel parts {0}", protoPartInfos.Count));
            if (partInfos.Count != protoPartInfos.Count)
            {
                Debug.Log("[BeenThereDoneThat]: parts are not equal!");
                return false;
            }

            for (int i = 0; i < partInfos.Count; i++)
            {
                Debug.Log(string.Format("[BeenThereDoneThat]: partInfos part: {0}", partInfos[i].name));
                Debug.Log(string.Format("[BeenThereDoneThat]: protoPartInfos part: {0}", protoPartInfos[i].name));
                if (partInfos[i].name != protoPartInfos[i].name)
                {
                    Debug.Log(string.Format("[BeenThereDoneThat]: parts are not equal at {0}: {1} != {2}", i, partInfos[i].name, protoPartInfos[i].name));
                    return false;
                }

            }
            Debug.Log("[BeenThereDoneThat]: parts are equal!");

            // Compare resouces by amount
            Debug.Log(string.Format("[BeenThereDoneThat]: vessel resources {0}", resources.Count));
            Debug.Log(string.Format("[BeenThereDoneThat]: proto vessel resources {0}", protoResources.Count));
            if (resources.Count != protoResources.Count)
            {
                return false;
            }
            foreach (string resourceKey in resources.Keys)
            {
                if (resources[resourceKey] != protoResources[resourceKey])
                {
                    Debug.Log(string.Format("[BeenThereDoneThat]: resource mismatch {0}: {1} != {2}", resourceKey, resources[resourceKey], protoResources[resourceKey]));
                    return false;
                }
            }
            Debug.Log("[BeenThereDoneThat]: resources are equal!");
            return true;
        }
    }
}
