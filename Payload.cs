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

        public void DeployParts()
        {
            DeployFairings();
            DeployDeployableParts();

        }

        protected void DeployFairings()
        {
            foreach (Part payloadPart in parts)
            {
                foreach (ModuleProceduralFairing fairing in payloadPart.FindModulesImplementing<ModuleProceduralFairing>())
                {
                    Debug.Log(string.Format("[BeenThereDoneThat]: Deploying fairing {0}", payloadPart.name));
                    fairing.DeployFairing();
                }
            }
        }

        protected void DeployDeployableParts()
        {
            foreach (Part payloadPart in parts)
            {
                foreach (ModuleDeployableAntenna antenna in payloadPart.FindModulesImplementing<ModuleDeployableAntenna>())
                {
                    Debug.Log(string.Format("[BeenThereDoneThat]: Extending antenna {0}", payloadPart.name));
                    antenna.Extend();
                }
                foreach (ModuleDeployableSolarPanel panel in payloadPart.FindModulesImplementing<ModuleDeployableSolarPanel>())
                {
                    Debug.Log(string.Format("[BeenThereDoneThat]: Extending solar panel {0}", payloadPart.name));
                    panel.Extend();
                }
            }
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
