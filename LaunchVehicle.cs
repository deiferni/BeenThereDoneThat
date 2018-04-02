﻿using System.Collections.Generic;
using UnityEngine;

namespace BeenThereDoneThat
{
    public class LaunchVehicle
    {
        public List<Part> parts;
        public Part separator;

        public LaunchVehicle(Part payloadSeparator, List<Part> launchVehicleParts)
        {
            parts = launchVehicleParts;
            separator = payloadSeparator;
        }

        public void DebugParts()
        {
            Debug.Log("[BeenThereDoneThat]: launch vehicle parts");
            foreach (Part part in parts)
            {
                Debug.Log(part.name);
            }
            Debug.Log(string.Format("hash: {0})", GetHashCode()));
        }

        public override int GetHashCode()
        {
            // blindly following jon skeets advice from:
            // https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            int hash = 13;

            Dictionary<string, double> resources = new Dictionary<string, double>();
            foreach (Part part in parts)
            {
                hash = hash * 17 + part.partInfo.name.GetHashCode();

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
                hash = hash * 17 + resourceKey.GetHashCode();
                hash = hash * 17 + resources[resourceKey].GetHashCode();
            }

            return hash;
        }

        public bool Equals(LaunchVehicle launchVehicle)
        {
            return launchVehicle.GetHashCode() == GetHashCode();
        }

        public bool Equals(ProtoLaunchVehicle launchVehicle)
        {
            return launchVehicle.GetHashCode() == GetHashCode();
        }

        public void SaveAsSubmodule()
        {
            ShipConstruct shipConstruct = new ShipConstruct("Launched vehicle as submodule", "wohooo", separator);
            ShipConstruction.SaveSubassembly(shipConstruct, "Launched vehicle as submodule");
            Debug.Log("[BeenThereDoneThat]: Saved launched vehicle as submodule");
        }
    }
}
