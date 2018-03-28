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

        [KSPEvent(guiActive = true, guiName = "Debug available parts")]
        public void DebugPartAvailableParts()
        {
            // Find proto parts and resouces
            string text = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/Subassemblies/";
            string text2 = text + "My subassembly" + ".craft";
            List<AvailablePart> protoPartInfos = new List<AvailablePart>();
            ConfigNode subModuleRootNode = ConfigNode.Load(text2);
            Dictionary<string, double> protoResources = new Dictionary<string, double>();
            Debug.Log(string.Format("path {0}", text2));
            Debug.Log(string.Format("Part laoder is ready: {0}", PartLoader.Instance.IsReady()));
            Debug.Log(string.Format("amount of loaded parts: {0}", PartLoader.Instance.loadedParts.Count));
            Debug.Log(string.Format("amount of parts: {0}", PartLoader.Instance.parts.Count));
            foreach (ConfigNode partNode in subModuleRootNode.nodes)
            {
                string partname = KSPUtil.GetPartName(partNode.GetValue("part"));
                Debug.Log(string.Format("partname: {0}", partname));
                AvailablePart thepart = PartLoader.getPartInfoByName(partname);
                Debug.Log(string.Format("the part: {0}", thepart));
                protoPartInfos.Add(thepart);

                foreach (ConfigNode moduleNode in partNode.nodes)
                {
                    if (moduleNode.name != "RESOURCE")
                    {
                        continue;
                    }

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
                return;
            }

            for (int i = 0; i < partInfos.Count; i++)
            {
                Debug.Log(string.Format("partInfos part: {0}", partInfos[i].name));
                Debug.Log(string.Format("protoPartInfos part: {0}", protoPartInfos[i].name));
                if (partInfos[i].name != protoPartInfos[i].name) {
                    Debug.Log(string.Format("parts are not equal at {0}: {1} != {2}", i, partInfos[i].name, protoPartInfos[i].name));
                    return;
                }

            }
            Debug.Log("parts are equal!");

            // Compare resouces by amount
            Debug.Log(string.Format("vessel resources {0}", resources.Count));
            Debug.Log(string.Format("proto vessel resources {0}", protoResources.Count));
            if (resources.Count != protoResources.Count)
            {
                return;
            }
            foreach (string resourceKey in resources.Keys)
            {
                if (resources[resourceKey] != protoResources[resourceKey])
                {
                    Debug.Log(string.Format("resource mismatch {0}: {1} != {2}", resourceKey, resources[resourceKey], protoResources[resourceKey]));
                    return;
                }
            }
            Debug.Log("resources are equal!");
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

        [KSPEvent(guiActive = true, guiName = "Save launch vehicle submodule")]
        public void SaveLaunchVehivleSubModuleFlight()
        {
            foreach (Part part in vessel.parts)
            {
                foreach (PayloadSeparatorPart module in part.FindModulesImplementing<PayloadSeparatorPart>())
                {
                    if (module.isPayloadSeparator)
                    {
                        ShipConstruct shipConstruct = new ShipConstruct("Launched vehicle as submodule", "wohooo", part);
                        ShipConstruction.SaveSubassembly(shipConstruct, "Launched vehicle as submodul");

                        Debug.Log("Saved launched vessel as submodule");
                        return;
                    }
                }
            }
            Debug.Log("No payloadseparator found");
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

        [KSPEvent(guiActive = true, guiName = "Remember orbit.")]
        public void RememberOrbit()
        {
            OrbitController.Instance.RememberOrbit();
        }

        [KSPEvent(guiActive = true, guiName = "Restore remembered orbit.")]
        public void RestoreOrbit()
        {
            OrbitController.Instance.RestoreOrbit();
        }
    }
}
