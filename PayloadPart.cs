using UnityEngine;

namespace BeenThereDoneThat
{
    [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
    public class PayloadSeparatorPart : PartModule
    {
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true)]
        public bool isPayloadNode;

        [KSPEvent(guiActive = true, guiName = "Put me into orbit. NOW!")]
        public void PutMeIntoOrbitNow()
        {
            ScreenMessages.PostScreenMessage(
                "Putting the damn thing into orbit.",
                5.0f,
                ScreenMessageStyle.UPPER_CENTER);

            CelestialBody body = FlightGlobals.ActiveVessel.mainBody;
            int selBodyIndex = FlightGlobals.Bodies.IndexOf(body);
            double sma = body.minOrbitalDistance * 1.1;

            double ecc = 0;
            double inc = 0;
            double LAN = 0;
            double mna = 0;
            double argPe = 0;
            double ObT = 0;

            Debug.Log(string.Format("Setting 'sma' to {0} based on minimal distance {1}", sma, body.minOrbitalDistance));

            FlightGlobals.fetch.SetShipOrbit(selBodyIndex, ecc, sma, inc, LAN, mna, argPe, ObT);
        }
    }
}
