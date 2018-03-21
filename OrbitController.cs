using System;
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
    public class OrbitController : ScenarioModule
    {
        public int selBodyIndex = -1;
        public double sma;
        public double ecc;
        public double inc;
        public double LAN;
        public double mna;
        public double argPe;

        public static OrbitController Instance
        {
            get;
            private set;
        }

        public override void OnAwake()
        {
            Debug.Log("[OrbitController]: AWAKING ORBITCONTROLLER");
            OrbitController.Instance = this;
        }

        public override void OnSave(ConfigNode node)
        {
            Debug.Log("[OrbitController]: SAVING ORBITCONTROLLER");

            if (selBodyIndex < 0)
            {
                return;
            }

            ConfigNode bddtnode = node.AddNode("BeenThereDoneThat");
            bddtnode.AddValue("selBodyIndex", selBodyIndex);
            bddtnode.AddValue("sma", sma);
            bddtnode.AddValue("ecc", ecc);
            bddtnode.AddValue("inc", inc);
            bddtnode.AddValue("LAN", LAN);
            bddtnode.AddValue("mna", mna);
            bddtnode.AddValue("argPe", argPe);
        }

        public override void OnLoad(ConfigNode node)
        {
            Debug.Log("[OrbitController]: LOADING ORBITCONTROLLER");

            if (!node.HasNode("BeenThereDoneThat"))
            {
                return;
            }
               
            ConfigNode bddtnode = node.GetNode("BeenThereDoneThat");
            selBodyIndex = int.Parse(bddtnode.GetValue("selBodyIndex"));
            sma = double.Parse(bddtnode.GetValue("sma"));
            ecc = double.Parse(bddtnode.GetValue("ecc"));
            inc = double.Parse(bddtnode.GetValue("inc"));
            LAN = double.Parse(bddtnode.GetValue("LAN"));
            mna = double.Parse(bddtnode.GetValue("mna"));
            argPe = double.Parse(bddtnode.GetValue("argPe"));
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

            Debug.Log(
                string.Format("[OrbitController]: REMEMBERING ORBIT> sma: {0} ecc: {1} inc: {2} LAN: {3} mna: {4} argPe: {5}",
                              sma, ecc, inc, LAN, mna, argPe));
            

            string directory = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/BeenThereDoneThat/";
            System.IO.Directory.CreateDirectory(directory);
            string path = System.IO.Path.Combine(directory, "LastVessel.craft");

            Vessel vessel = FlightGlobals.ActiveVessel;
            ConfigNode node = new ConfigNode();
            vessel.protoVessel.Save(node);
            node.Save(path);
        }

        public void RestoreOrbit()
        {
            if (selBodyIndex < 0)
            {
                ScreenMessages.PostScreenMessage(
                    "No orbit has been remembered yet.",
                    5.0f,
                    ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            
            Debug.Log(
                string.Format("[OrbitController]: RESTORING ORBIT> sma: {0} ecc: {1} inc: {2} LAN: {3} mna: {4} argPe: {5}",
                              sma, ecc, inc, LAN, mna, argPe));


            string directory = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/BeenThereDoneThat/";
            string path = System.IO.Path.Combine(directory, "LastVessel.craft");

            ConfigNode node = ConfigNode.Load(path);
            ProtoVessel proto = new ProtoVessel(node, HighLogic.CurrentGame);
            proto.vesselName = "Last Vessel";

            foreach (ProtoPartSnapshot p in proto.protoPartSnapshots)
            {
                if (p.protoModuleCrew != null && p.protoModuleCrew.Count != 0)
                {
                    List<ProtoCrewMember> cl = p.protoModuleCrew;
                    List<ProtoCrewMember> clc = new List<ProtoCrewMember>(cl);

                    foreach (ProtoCrewMember c in clc)
                    {
                        p.RemoveCrew(c);
                    }
                }
            }

            proto.Load(HighLogic.CurrentGame.flightState);
            Vessel vessel = proto.vesselRef;

            System.Random rand = new System.Random();
            Vector3d offset = new Vector3d(
                (double)rand.Next(10, 30),
                (double)rand.Next(10, 30),
                (double)rand.Next(10, 30));

            Debug.Log(
                string.Format("[OrbitController]: orbit offset> x: {0} y: {1} z: {2}",
                              offset.x, offset.y, offset.z));

            // vessel.orbit.UpdateFromStateVectors(vessel.orbit.pos + offset, vessel.orbit.vel, vessel.orbit.referenceBody, Planetarium.GetUniversalTime());
            // FlightGlobals.fetch.SetShipOrbit(selBodyIndex, ecc, sma, inc, LAN, argPe, mna, 0);
        }

    }
}
