using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeenThereDoneThat
{
    public class Payload
    {
        public List<Part> parts;

        public Payload(List<Part> payloadParts)
        {
            parts = payloadParts;
        }

        public double getTotalMass()
        {
            double totalMass = 0;
            foreach (Part part in parts)
            {
                totalMass += part.mass + part.GetResourceMass();
            }
            return Math.Round(totalMass, 3);
        }

        public void DebugParts()
        {
            Debug.Log("[BeenThereDoneThat]: payload parts");
            foreach (Part part in parts)
            {
                Debug.Log(part.name);
            }
            Debug.Log(string.Format("Total mass: {0}", getTotalMass()));
        }
    }
}
