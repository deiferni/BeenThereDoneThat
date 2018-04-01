using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeenThereDoneThat
{
    public class ProtoPayload
    {
        public List<ProtoPartSnapshot> parts;

        public ProtoPayload(List<ProtoPartSnapshot> payloadParts)
        {
            parts = payloadParts;
        }

        public double getTotalMass()
        {
            double totalMass = 0;
            foreach (ProtoPartSnapshot part in parts)
            {
                totalMass += part.mass;
                foreach (ProtoPartResourceSnapshot resource in part.resources)
                {
                    PartResourceDefinition resourceInfo = PartResourceLibrary.Instance.GetDefinition(resource.resourceName);
                    totalMass += resource.amount * resourceInfo.density;
                }
            }
            return Math.Round(totalMass, 3);
        }

        public void DebugParts()
        {
            Debug.Log("[BeenThereDoneThat]: proto payload parts");
            foreach (ProtoPartSnapshot part in parts)
            {
                Debug.Log(part.partName);
            }
            Debug.Log(string.Format("Total mass: {0}", getTotalMass()));
        }
    }
}
