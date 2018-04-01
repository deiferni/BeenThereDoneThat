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

        public ProtoLaunchVehicle(ProtoPartSnapshot payloadSeparator, List<ProtoPartSnapshot> launchVehicleParts)
        {
            separator = payloadSeparator;
            parts = launchVehicleParts;

            foreach (ProtoPartSnapshot part in parts)
            {
                maxSeparationIndex = Math.Max(maxSeparationIndex, part.separationIndex);
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
        }
    }
}
