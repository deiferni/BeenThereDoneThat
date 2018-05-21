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
            DeployParts();
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

            previousLaunchVehicle.DebugVehicle();
            currentLaunchVehicle.DebugParts();

            if (!previousLaunchVehicle.Equals(currentLaunchVehicle))
            {
                Debug.Log("[BeenThereDoneThat]: launch vehicles not equal!");
                return false;
            }
            
            Debug.Log("[BeenThereDoneThat]: launch vehicles equal!");
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

        protected void DeployParts()
        {
            currentPayload.DeployParts();
            Debug.Log("[BeenThereDoneThat]: Extended antenna and solar panels.");
        }
    }
}
