using UnityEngine;

namespace BeenThereDoneThat
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class OrbitController : MonoBehaviour
    {
        public int selBodyIndex;
        public double sma;
        public double ecc;
        public double inc;
        public double LAN;
        public double mna;
        public double argPe;
        public bool hasData = false;

        public static OrbitController Instance
        {
            get;
            set;
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
        }

        public void RememberOrbit()
        {
            Orbit orbit = FlightGlobals.ActiveVessel.orbit;
            CelestialBody body = FlightGlobals.ActiveVessel.mainBody;

            selBodyIndex = FlightGlobals.Bodies.IndexOf(body);
            sma = orbit.semiMajorAxis;
            ecc = orbit.eccentricity;
            inc = orbit.inclination;
            LAN = orbit.LAN;
            mna = orbit.meanAnomalyAtEpoch;
            argPe = orbit.argumentOfPeriapsis;

            hasData = true;

            Debug.Log(
                string.Format("sma: {0} ecc: {1} inc: {2} LAN: {3} mna: {4} argPe: {5}",
                sma, ecc, inc, LAN, mna, argPe));
        }

        public void RestoreOrbit()
        {
            if (!hasData) {
                ScreenMessages.PostScreenMessage(
                    "No orbit has been remembered yet.",
                    5.0f,
                    ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            Debug.Log(
                string.Format("sma: {0} ecc: {1} inc: {2} LAN: {3} mna: {4} argPe: {5}",
                sma, ecc, inc, LAN, mna, argPe));

            FlightGlobals.fetch.SetShipOrbit(selBodyIndex, ecc, sma, inc, LAN, argPe, mna, 0);
        }

    }
}
