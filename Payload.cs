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

        public void DebugParts()
        {
            Debug.Log("[BeenThereDoneThat]: payload parts");
            foreach (Part part in parts)
            {
                Debug.Log(part.name);
            }
        }
    }
}
