using System.Collections.Generic;
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
        }

        public void SaveAsSubmodule()
        {
            ShipConstruct shipConstruct = new ShipConstruct("Launched vehicle as submodule", "wohooo", separator);
            ShipConstruction.SaveSubassembly(shipConstruct, "Launched vehicle as submodule");
            Debug.Log("[BeenThereDoneThat]: Saved launched vehicle as submodule");
        }
    }
}
