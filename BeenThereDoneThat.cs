using UnityEngine;

namespace BeenThereDoneThat
{
    [KSPScenario(
        ScenarioCreationOptions.AddToAllGames,
        new GameScenes[] {GameScenes.FLIGHT, GameScenes.EDITOR}
    )]
    public class BeenThereDoneThat : ScenarioModule
    {
        public static BeenThereDoneThat Instance
        {
            get;
            private set;
        }

        public override void OnAwake()
        {
            Debug.Log("[OrbitController]: AWAKING ORBITCONTROLLER");
            BeenThereDoneThat.Instance = this;
        }

        public override void OnSave(ConfigNode node)
        {
            Debug.Log("[OrbitController]: SAVING ORBITCONTROLLER");
        }

        public override void OnLoad(ConfigNode node)
        {
            Debug.Log("[OrbitController]: LOADING ORBITCONTROLLER");
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
                string.Format("[OrbitController]: REMEMBERING ORBIT> sma: {0} ecc: {1} inc: {2} LAN: {3} mna: {4} argPe: {5}",
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
