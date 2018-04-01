using UnityEngine;

namespace BeenThereDoneThat
{
    public class QuickLaunchHangar
    {
        public static string ORBITFILENAME = "VesselOrbit.craft";
        public static string LAUNCHFILENAME = "VesselLaunch.craft";

        public static ProtoVessel LoadOrbitProtoVessel()
        {
            ConfigNode subModuleRootNode = ConfigNode.Load(MakeHangarPath(ORBITFILENAME));
            return new ProtoVessel(subModuleRootNode, HighLogic.CurrentGame);
        }

        public static ProtoVessel LoadLaunchProtoVessel()
        {
            ConfigNode subModuleRootNode = ConfigNode.Load(MakeHangarPath(LAUNCHFILENAME));
            return new ProtoVessel(subModuleRootNode, HighLogic.CurrentGame);
        }

        public static void SaveOrbitVessel(Vessel vessel)
        {
            Orbit orbit = vessel.orbit;
            CelestialBody body = vessel.mainBody;

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

            SaveVessel(vessel, ORBITFILENAME);
        }

        public static void SaveLaunchVessel(Vessel vessel)
        {
            SaveVessel(vessel, LAUNCHFILENAME);
        }

        protected static void SaveVessel(Vessel vessel, string filename)
        {
            ConfigNode node = new ConfigNode();
            vessel.protoVessel.Save(node);
            node.Save(MakeHangarPath(filename));
        }

        protected static string MakeHangarDirectory()
        {
            string directory = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/BeenThereDoneThat/";
            System.IO.Directory.CreateDirectory(directory);
            return directory;
        }

        protected static string MakeHangarPath(string filename)
        {
            return System.IO.Path.Combine(MakeHangarDirectory(), filename);
        }
    }
}
