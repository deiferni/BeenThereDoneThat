using System.Collections.Generic;
using UnityEngine;

namespace BeenThereDoneThat
{
    [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
    public class PayloadSeparatorPart : PartModule
    {
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true)]
        public bool isPayloadSeparator;

        [KSPEvent(guiActive = true, guiName = "Set as payload separator")]
        public void SetPayloadSeparator()
        {
            SetActive(vessel.parts);
        }

        [KSPEvent(guiActiveEditor = true, guiName = "Set as payload separator")]
        public void SetEditorPayloadSeparator()
        {
            SetActive(EditorLogic.SortedShipList);
        }

        private void SetActive(List<Part> parts)
        {
            foreach (Part part in parts)
            {
                foreach (PayloadSeparatorPart module in part.FindModulesImplementing<PayloadSeparatorPart>())
                {
                    if (module.isPayloadSeparator)
                    {
                        Debug.Log(string.Format("Disabling payload separtor on {0}:{1}", part.name, module.name));
                        module.isPayloadSeparator = false;
                    }
                }
            }
            isPayloadSeparator = true;
        }

        [KSPEvent(guiActive = true, guiName = "Destroy last part")]
        public void DestroyLastPart()
        {
            Part lastPart = vessel.parts[vessel.parts.Count - 1];
            lastPart.Die();
        }

        [KSPEvent(guiActiveEditor = true, guiName = "Save launch vehicle submodule")]
        public void SaveLaunchVehivleSubModule()
        {
            List<Part> parts = new List<Part>();

            foreach (Part part in EditorLogic.SortedShipList)
            {
                foreach (PayloadSeparatorPart module in part.FindModulesImplementing<PayloadSeparatorPart>())
                {
                    if (module.isPayloadSeparator)
                    {

                        ShipConstruct shipConstruct = new ShipConstruct("My subassembly", "wohooo", part);
                        int inverseStage = EditorLogic.RootPart.inverseStage;
                        foreach (Part pt in shipConstruct.parts)
                        {
                            pt.inverseStage -= inverseStage;
                        }
                        ShipConstruction.SaveSubassembly(shipConstruct, "My subassembly");
                        return;
                    }
                }
            }

            Debug.Log("No payloadseparator found");
        }

        [KSPEvent(guiActive = true, guiName = "BeenThereDoneThat: start mission")]
        public void SaveLaunchVehivle()
        {
            foreach (Part part in vessel.parts)
            {
                foreach (PayloadSeparatorPart module in part.FindModulesImplementing<PayloadSeparatorPart>())
                {
                    if (module.isPayloadSeparator)
                    {
                        ShipConstruct shipConstruct = new ShipConstruct("Launched vehicle as submodule", "wohooo", part);
                        ShipConstruction.SaveSubassembly(shipConstruct, "Launched vehicle as submodule");

                        Debug.Log("Saved launched vessel as submodule");

                        OrbitController.Instance.RememberVessel("VesselLaunch.craft");
                        return;
                    }
                }
            }
            Debug.Log("No payloadseparator found");
        }

        [KSPEvent(guiActive = true, guiName = "BeenThereDoneThat: end mission")]
        public void RememberOrbit()
        {
            vessel.BackupVessel();
            OrbitController.Instance.RememberVessel("VesselOrbit.craft");
        }

        [KSPEvent(guiActive = true, guiName = "BeenThereDoneThat: re-run mission")]
        public void ReRunMission()
        {
            // check if we can do a re-run
            if (!IsPreviouslyUsedLaunchVessel())
            {
                Debug.Log("Can't re-run mission: launch vessel not equal to previous run");
                return;
            }

            // find orbit information and restore orbit
            string text = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/BeenThereDoneThat/";
            string text2 = text + "VesselOrbit.craft";
            ConfigNode subModuleRootNode = ConfigNode.Load(text2);
            ConfigNode orbitNode = subModuleRootNode.GetNode("ORBIT");
            int orbitStage = int.Parse(subModuleRootNode.GetValue("stg"));

            int selBodyIndex = int.Parse(orbitNode.GetValue("REF"));
            double sma = double.Parse(orbitNode.GetValue("SMA"));
            double ecc = double.Parse(orbitNode.GetValue("ECC"));
            double inc = double.Parse(orbitNode.GetValue("INC"));
            double LAN = double.Parse(orbitNode.GetValue("LAN"));
            double mna = double.Parse(orbitNode.GetValue("MNA"));
            double argPe = double.Parse(orbitNode.GetValue("LPE"));
            double epoch = double.Parse(orbitNode.GetValue("EPH"));

            Debug.Log(
                string.Format("[OrbitController]: RESTORING ORBIT> sma: {0} ecc: {1} inc: {2} LAN: {3} mna: {4} argPe: {5} epoch: {6}",
                              sma, ecc, inc, LAN, mna, argPe, epoch));

            // hackishly temporarily set planetarium time 
            double prevTime = Planetarium.fetch.time;
            Planetarium.fetch.time = epoch;
            FlightGlobals.fetch.SetShipOrbit(selBodyIndex, ecc, sma, inc, LAN, argPe, mna, 0);
            // hackishly restore planetarium time
            Planetarium.fetch.time = prevTime;
            Debug.Log("Put ship into orbit");

            // restore resource levels
            bool foundProtoSeparator = false;
            List<AvailablePart> protoPartInfos = new List<AvailablePart>();
            ConfigNode[] protoPartNodes = subModuleRootNode.GetNodes("PART");
            for (int i = 0; i < protoPartNodes.Length; i++)
            {
                ConfigNode protoPartNode = protoPartNodes[i];
                if (!foundProtoSeparator)
                {
                    foreach (ConfigNode maybePayloadSeparatorModule in protoPartNode.GetNodes("MODULE"))
                    {
                        if (maybePayloadSeparatorModule.GetValue("name") != "PayloadSeparatorPart")
                        {
                            continue;
                        }

                        if (bool.Parse(maybePayloadSeparatorModule.GetValue("isPayloadSeparator")))
                        {
                            foundProtoSeparator = true;
                        }
                    }
                }
                if (!foundProtoSeparator)
                {
                    continue;
                }

                Part vesselPart = vessel.Parts[i];
                foreach (ConfigNode moduleNode in protoPartNode.GetNodes("RESOURCE"))
                {
                    string resourceName = moduleNode.GetValue("name");
                    double amount = double.Parse(moduleNode.GetValue("amount"));
                    Debug.Log(string.Format("Found resource {0}: amount: {1}", resourceName, amount));
                    PartResource partResource = vesselPart.Resources.Get(resourceName);
                    partResource.amount = amount;
                    GameEvents.onPartResourceListChange.Fire(vesselPart);
                }
            }
            Debug.Log("Restored resources");

            // removing burned stages/parts
            if (protoPartNodes.Length < vessel.parts.Count)
            {
                int diff = vessel.parts.Count - protoPartNodes.Length;
                int count = 0;
                List<Part> toDie = new List<Part>();
                Debug.Log(string.Format("Removing {0} burned parts below stage {1}", diff, orbitStage));

                foreach (Part vesselPart in vessel.parts)
                {
                    if (vesselPart.defaultInverseStage > orbitStage)
                    {
                        Debug.Log(string.Format("Removing part {0}", vesselPart.name));
                        toDie.Add(vesselPart);
                        count++;
                    }
                }
                if (count != diff)
                {
                    Debug.Log(string.Format("Expected to remote {0} but got {1} parts", diff, count));
                }
                else
                {
                    toDie.Reverse();
                    foreach (Part spent in toDie)
                    {
                        spent.Die();
                    }
                }
            }
            else
            {
                Debug.Log("No parts to remove");
            }
            Debug.Log("Wohoooo done!");
        }

        public bool IsPreviouslyUsedLaunchVessel()
        {
            // Find proto parts and resouces
            string text = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/BeenThereDoneThat/";
            string text2 = text + "VesselLaunch.craft";
            bool foundProtoSeparator = false;
            List<AvailablePart> protoPartInfos = new List<AvailablePart>();
            ConfigNode subModuleRootNode = ConfigNode.Load(text2);
            Dictionary<string, double> protoResources = new Dictionary<string, double>();
            Debug.Log(string.Format("path {0}", text2));
            Debug.Log(string.Format("Part laoder is ready: {0}", PartLoader.Instance.IsReady()));
            Debug.Log(string.Format("amount of loaded parts: {0}", PartLoader.Instance.loadedParts.Count));
            Debug.Log(string.Format("amount of parts: {0}", PartLoader.Instance.parts.Count));
            foreach (ConfigNode partNode in subModuleRootNode.GetNodes("PART"))
            {
                if (!foundProtoSeparator)
                {
                    foreach (ConfigNode maybePayloadSeparatorModule in partNode.GetNodes("MODULE"))
                    {
                        if (maybePayloadSeparatorModule.GetValue("name") != "PayloadSeparatorPart")
                        {
                            continue;
                        }

                        if (bool.Parse(maybePayloadSeparatorModule.GetValue("isPayloadSeparator")))
                        {
                            foundProtoSeparator = true;
                        }
                    }
                }
                if (!foundProtoSeparator)
                {
                    continue;
                }

                string partname = partNode.GetValue("name");
                Debug.Log(string.Format("partname: {0}", partname));
                AvailablePart thepart = PartLoader.getPartInfoByName(partname);
                Debug.Log(string.Format("the part: {0}", thepart));
                protoPartInfos.Add(thepart);

                foreach (ConfigNode moduleNode in partNode.GetNodes("RESOURCE"))
                {
                    string resourceName = moduleNode.GetValue("name");
                    double amount = double.Parse(moduleNode.GetValue("amount"));
                    Debug.Log(string.Format("Found resource {0}: amount: {1}", resourceName, amount));

                    if (!protoResources.ContainsKey(resourceName))
                    {
                        protoResources[resourceName] = 0;
                    }
                    protoResources[resourceName] += amount;
                }
            }
            foreach (string resourceKey in protoResources.Keys)
            {
                Debug.Log(string.Format("Total resource {0}: amount: {1}", resourceKey, protoResources[resourceKey]));
            }

            // Find vessel parts and resouces
            List<AvailablePart> partInfos = new List<AvailablePart>();
            Dictionary<string, double> resources = new Dictionary<string, double>();
            // just hope the vessel parts are ordered somehow
            bool found = false;
            foreach (Part part in vessel.parts)
            {
                if (!found)
                {
                    foreach (PayloadSeparatorPart module in part.FindModulesImplementing<PayloadSeparatorPart>())
                    {
                        if (module.isPayloadSeparator)
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (found)
                {
                    Debug.Log(string.Format("the part: {0}", part.partInfo.name));
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
            }
            foreach (string resourceKey in resources.Keys)
            {
                Debug.Log(string.Format("Total resource {0}: amount: {1}", resourceKey, resources[resourceKey]));
            }

            // Compare parts by name and order
            Debug.Log(string.Format("vessel parts {0}", partInfos.Count));
            Debug.Log(string.Format("proto vessel parts {0}", protoPartInfos.Count));
            if (partInfos.Count != protoPartInfos.Count)
            {
                Debug.Log("parts are not equal!");
                return false;
            }

            for (int i = 0; i < partInfos.Count; i++)
            {
                Debug.Log(string.Format("partInfos part: {0}", partInfos[i].name));
                Debug.Log(string.Format("protoPartInfos part: {0}", protoPartInfos[i].name));
                if (partInfos[i].name != protoPartInfos[i].name)
                {
                    Debug.Log(string.Format("parts are not equal at {0}: {1} != {2}", i, partInfos[i].name, protoPartInfos[i].name));
                    return false;
                }

            }
            Debug.Log("parts are equal!");

            // Compare resouces by amount
            Debug.Log(string.Format("vessel resources {0}", resources.Count));
            Debug.Log(string.Format("proto vessel resources {0}", protoResources.Count));
            if (resources.Count != protoResources.Count)
            {
                return false;
            }
            foreach (string resourceKey in resources.Keys)
            {
                if (resources[resourceKey] != protoResources[resourceKey])
                {
                    Debug.Log(string.Format("resource mismatch {0}: {1} != {2}", resourceKey, resources[resourceKey], protoResources[resourceKey]));
                    return false;
                }
            }
            Debug.Log("resources are equal!");
            return true;
        }

        [KSPEvent(guiActive = true, guiName = "Put me into orbit. NOW!")]
        public void PutMeIntoOrbitNow()
        {
            ScreenMessages.PostScreenMessage(
                "Putting the damn thing into orbit.",
                5.0f,
                ScreenMessageStyle.UPPER_CENTER);

            CelestialBody body = FlightGlobals.ActiveVessel.mainBody;
            int selBodyIndex = FlightGlobals.Bodies.IndexOf(body);
            double sma = body.minOrbitalDistance * 1.1;

            double ecc = 0;
            double inc = 0;
            double LAN = 0;
            double mna = 0;
            double argPe = 0;
            double ObT = 0;

            Debug.Log(string.Format("Setting 'sma' to {0} based on minimal distance {1}", sma, body.minOrbitalDistance));

            FlightGlobals.fetch.SetShipOrbit(selBodyIndex, ecc, sma, inc, LAN, mna, argPe, ObT);
        }
    }
}
