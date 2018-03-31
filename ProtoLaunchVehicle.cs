using System.Collections.Generic;
using UnityEngine;

namespace BeenThereDoneThat
{
    public class ProtoLaunchVehicle
    {
        public ProtoPartSnapshot separator;
        public List<ProtoPartSnapshot> parts;

        public ProtoLaunchVehicle(ProtoPartSnapshot payloadSeparator, List<ProtoPartSnapshot> launchVehicleParts)
        {
            separator = payloadSeparator;
            parts = launchVehicleParts;
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
