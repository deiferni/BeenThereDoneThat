using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeenThereDoneThat
{
    public class ProtoLaunchVehicle
    {
        public ProtoPartSnapshot separator;
        public List<ProtoPartSnapshot> parts;
        public int maxSeparationIndex = -1;
        public Dictionary<string, double> resources;

        public ProtoLaunchVehicle(ProtoPartSnapshot payloadSeparator, List<ProtoPartSnapshot> launchVehicleParts)
        {
            separator = payloadSeparator;
            parts = launchVehicleParts;

            foreach (ProtoPartSnapshot part in parts)
            {
                maxSeparationIndex = Math.Max(maxSeparationIndex, part.separationIndex);

                foreach (ProtoPartResourceSnapshot partResource in part.resources)
                {
                    if (!resources.ContainsKey(partResource.resourceName))
                    {
                        resources[partResource.resourceName] = 0;
                    }
                    resources[partResource.resourceName] += partResource.amount;
                }
            }
        }

        public void RestoreResources(LaunchVehicle launchVehicle)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                ProtoPartSnapshot protoPart = parts[i];
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
            }
        }

        public void RemoveBurnedParts(LaunchVehicle launchVehicle)
        {
            if (parts.Count < launchVehicle.parts.Count)
            {
                int diff = launchVehicle.parts.Count - parts.Count;
                int count = 0;
                List<Part> toDie = new List<Part>();
                Debug.Log(string.Format("[BeenThereDoneThat]: Removing {0} burned parts below stage {1}", diff, maxSeparationIndex));

                foreach (Part vesselPart in launchVehicle.parts)
                {
                    if (vesselPart.separationIndex > maxSeparationIndex)
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
        }

        public void DebugParts()
        {
            Debug.Log("[BeenThereDoneThat]: proto launch vehicle parts");
            foreach (ProtoPartSnapshot part in parts)
            {
                Debug.Log(part.partName);
            }
            foreach (string resourceKey in resources.Keys)
            {
                double amount = resources[resourceKey].GetHashCode();
                Debug.Log(string.Format("{0}: {1})", resourceKey, amount));
            }
            Debug.Log(string.Format("hash: {0})", GetHashCode()));
        }

        public override int GetHashCode()
        {
            // blindly following jon skeets advice from:
            // https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            int hash = 13;

            Dictionary<string, double> resources = new Dictionary<string, double>();
            foreach (ProtoPartSnapshot part in parts)
            {
                hash = hash * 17 + part.partName.GetHashCode();
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
    }
}
