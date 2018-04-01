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

        public void DebugParts()
        {
            Debug.Log("[BeenThereDoneThat]: proto payload parts");
            foreach (ProtoPartSnapshot part in parts)
            {
                Debug.Log(part.partName);
            }
        }
    }
}
