using System.Collections.Generic;
using UnityEngine;

namespace BeenThereDoneThat
{
    public class QuickLauch
    {
        private Vessel vessel;
        private ProtoVessel prevlaunchProtoVessel;
        private ProtoVessel orbitProtoVessel;
        private LaunchVehicle currentLaunchVehicle = null;
        private Payload currentPayload = null;
        private ProtoLaunchVehicle previousLaunchVehicle = null;
        private ProtoPayload previousPayload = null;
        private ProtoLaunchVehicle orbitLaunchVehicle = null;
        private ProtoPayload orbitPayload = null;

        public QuickLauch(Vessel vessel, ProtoVessel prevlaunchProtoVessel, ProtoVessel orbitProtoVessel)
        {
            this.vessel = vessel;
            this.prevlaunchProtoVessel = prevlaunchProtoVessel;
            this.orbitProtoVessel = orbitProtoVessel;
        }

        public void Liftoff()
        {
            if (!InitVehicles())
            {
                return;
            }
            if (!IsPreviouslyUsedLaunchVehicle())
            {
                Debug.Log("[BeenThereDoneThat]: Can't re-run mission: launch vessel not equal to previous run");
                return;
            }
            if (MassExceedsPreviousMass())
            {
                Debug.Log("[BeenThereDoneThat]: Can't re-run mission: payload too heavy");
                currentPayload.DebugParts();
                previousPayload.DebugParts();
                return;
            }
            SetOrbit();
            SetResources();
            RemoveBurnedParts();
            Debug.Log("[BeenThereDoneThat]: Wohoooo done!");
        }

        protected bool InitVehicles()
        {
            if (!QuickLauncher.Instance.Split(vessel.parts, out currentLaunchVehicle, out currentPayload))
            {
                return false;
            }

            // load previously used launch vessel
            if (!QuickLauncher.Instance.Split(prevlaunchProtoVessel, out previousLaunchVehicle, out previousPayload))
            {
                return false;
            }

            // load vessel that arrived in orbit
            if (!QuickLauncher.Instance.Split(orbitProtoVessel, out orbitLaunchVehicle, out orbitPayload))
            {
                return false;
            }

            return true;
        }

        protected bool IsPreviouslyUsedLaunchVehicle()
        {
            List<AvailablePart> protoPartInfos = new List<AvailablePart>();
            Dictionary<string, double> protoResources = new Dictionary<string, double>();

            previousLaunchVehicle.DebugParts();

            foreach (ProtoPartSnapshot protoPart in previousLaunchVehicle.parts)
            {
                foreach (ProtoPartResourceSnapshot protoResource in protoPart.resources)
                {
                    string resourceName = protoResource.resourceName;
                    double amount = protoResource.amount;
                    Debug.Log(string.Format("[BeenThereDoneThat]: Found resource {0}: amount: {1}", resourceName, amount));

                    if (!protoResources.ContainsKey(resourceName))
                    {
                        protoResources[resourceName] = 0;
                    }
                    protoResources[resourceName] += amount;
                }
                protoPartInfos.Add(protoPart.partInfo);
            }
            foreach (string resourceKey in protoResources.Keys)
            {
                Debug.Log(string.Format("[BeenThereDoneThat]: Total resource {0}: amount: {1}", resourceKey, protoResources[resourceKey]));
            }

            List<AvailablePart> partInfos = new List<AvailablePart>();
            Dictionary<string, double> resources = new Dictionary<string, double>();
            currentLaunchVehicle.DebugParts();
            foreach (Part part in currentLaunchVehicle.parts)
            {
                partInfos.Add(part.partInfo);
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
                Debug.Log(string.Format("[BeenThereDoneThat]: Total resource {0}: amount: {1}", resourceKey, resources[resourceKey]));
            }

            // Compare parts by name and order
            Debug.Log(string.Format("[BeenThereDoneThat]: vessel parts {0}", partInfos.Count));
            Debug.Log(string.Format("[BeenThereDoneThat]: proto vessel parts {0}", protoPartInfos.Count));
            if (partInfos.Count != protoPartInfos.Count)
            {
                Debug.Log("[BeenThereDoneThat]: parts are not equal!");
                return false;
            }

            for (int i = 0; i < partInfos.Count; i++)
            {
                Debug.Log(string.Format("[BeenThereDoneThat]: partInfos part: {0}", partInfos[i].name));
                Debug.Log(string.Format("[BeenThereDoneThat]: protoPartInfos part: {0}", protoPartInfos[i].name));
                if (partInfos[i].name != protoPartInfos[i].name)
                {
                    Debug.Log(string.Format("[BeenThereDoneThat]: parts are not equal at {0}: {1} != {2}", i, partInfos[i].name, protoPartInfos[i].name));
                    return false;
                }

            }
            Debug.Log("[BeenThereDoneThat]: parts are equal!");

            // Compare resouces by amount
            Debug.Log(string.Format("[BeenThereDoneThat]: vessel resources {0}", resources.Count));
            Debug.Log(string.Format("[BeenThereDoneThat]: proto vessel resources {0}", protoResources.Count));
            if (resources.Count != protoResources.Count)
            {
                return false;
            }
            foreach (string resourceKey in resources.Keys)
            {
                if (resources[resourceKey] != protoResources[resourceKey])
                {
                    Debug.Log(string.Format("[BeenThereDoneThat]: resource mismatch {0}: {1} != {2}", resourceKey, resources[resourceKey], protoResources[resourceKey]));
                    return false;
                }
            }
            Debug.Log("[BeenThereDoneThat]: resources are equal!");
            return true;
        }

        protected bool MassExceedsPreviousMass()
        {
            return previousPayload.getTotalMass() < currentPayload.getTotalMass();
        }

        protected void SetOrbit()
        {
            OrbitSnapshot protoOrbit = orbitProtoVessel.orbitSnapShot;

            int selBodyIndex = protoOrbit.ReferenceBodyIndex;
            double sma = protoOrbit.semiMajorAxis;
            double ecc = protoOrbit.eccentricity;
            double inc = protoOrbit.inclination;
            double LAN = protoOrbit.LAN;
            double mna = protoOrbit.meanAnomalyAtEpoch;
            double argPe = protoOrbit.argOfPeriapsis;
            double epoch = protoOrbit.epoch;

            Debug.Log(
                string.Format("[BeenThereDoneThat]: RESTORING ORBIT> sma: {0} ecc: {1} inc: {2} LAN: {3} mna: {4} argPe: {5} epoch: {6}",
                              sma, ecc, inc, LAN, mna, argPe, epoch));

            // hackishly temporarily set planetarium time 
            double prevTime = Planetarium.fetch.time;
            Planetarium.fetch.time = epoch;
            FlightGlobals.fetch.SetShipOrbit(selBodyIndex, ecc, sma, inc, LAN, argPe, mna, 0);
            // hackishly restore planetarium time
            Planetarium.fetch.time = prevTime;
            Debug.Log("[BeenThereDoneThat]: Put ship into orbit");
        }

        protected void SetResources()
        {
            orbitLaunchVehicle.RestoreResources(currentLaunchVehicle);
            Debug.Log("[BeenThereDoneThat]: Restored resources");
        }

        protected void RemoveBurnedParts()
        {
            orbitLaunchVehicle.RemoveBurnedParts(currentLaunchVehicle);
            Debug.Log("[BeenThereDoneThat]: Removed burned parts/stages");
        }
    }
}
