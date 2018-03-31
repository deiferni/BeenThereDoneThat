using System.Collections.Generic;
using UnityEngine;

namespace BeenThereDoneThat
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[]
    {
        GameScenes.FLIGHT,
        GameScenes.TRACKSTATION,
        GameScenes.SPACECENTER,
        GameScenes.EDITOR
    })]
    public class QuickLauncher : ScenarioModule
    {
        public static QuickLauncher Instance
        {
            get;
            private set;
        }

        public override void OnAwake()
        {
            Debug.Log("[BeenThereDoneThat]: AWAKING ORBITCONTROLLER");
            Instance = this;
        }

        public override void OnSave(ConfigNode node)
        {
            Debug.Log("[BeenThereDoneThat]: SAVING ORBITCONTROLLER");
        }

        public override void OnLoad(ConfigNode node)
        {
            Debug.Log("[BeenThereDoneThat]: LOADING ORBITCONTROLLER");
        }

        public bool Split(List<Part> parts, out LaunchVehicle launchVehicle, out Payload payload)
        { 
            Part payloadSeparator = FindPayloadSeparatorPart(parts);
            if (payloadSeparator == null)
            {
                Debug.Log("[BeenThereDoneThat]: No payloadseparator found");
                launchVehicle = null;
                payload = null;
                return false;
            }

            int payloadPartSeparationIndex = payloadSeparator.separationIndex;
            List<Part> payloadParts = new List<Part>();
            List<Part> launchVehicleParts = new List<Part>();
            foreach (Part part in parts)
            {
                if (part.separationIndex >= payloadPartSeparationIndex)
                {
                    launchVehicleParts.Add(part);
                }
                else
                {
                    payloadParts.Add(part);
                }
            }

            launchVehicle = new LaunchVehicle(payloadSeparator, launchVehicleParts);
            payload = new Payload(payloadParts);
            return true;
        }

        public Part FindPayloadSeparatorPart(List<Part> parts)
        {
            foreach (Part part in parts)
            {
                foreach (PayloadSeparatorPart module in part.FindModulesImplementing<PayloadSeparatorPart>())
                {
                    if (module.isPayloadSeparator)
                    {
                        return part;
                    }
                }
            }
            return null;
        }

        public void RememberVessel(string name)
        {
            Orbit orbit = FlightGlobals.ActiveVessel.orbit;
            CelestialBody body = FlightGlobals.ActiveVessel.mainBody;

            int selBodyIndex = FlightGlobals.Bodies.IndexOf(body);
            double sma = orbit.semiMajorAxis;
            double ecc = orbit.eccentricity;
            double inc = orbit.inclination;
            double LAN = orbit.LAN;
            double mna = orbit.meanAnomalyAtEpoch;
            double argPe = orbit.argumentOfPeriapsis;

            Debug.Log(
                string.Format("[BeenThereDoneThat]: REMEMBERING ORBIT> sma: {0} ecc: {1} inc: {2} LAN: {3} mna: {4} argPe: {5}",
                              sma, ecc, inc, LAN, mna, argPe));
            

            string directory = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/BeenThereDoneThat/";
            System.IO.Directory.CreateDirectory(directory);
            string path = System.IO.Path.Combine(directory, name);

            Vessel vessel = FlightGlobals.ActiveVessel;
            ConfigNode node = new ConfigNode();
            vessel.protoVessel.Save(node);
            node.Save(path);
        }
    }
}
