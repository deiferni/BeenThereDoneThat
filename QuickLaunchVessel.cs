using System;
using System.Collections.Generic;

namespace BeenThereDoneThat
{
    public class QuickLaunchVessel
    {
        public List<QuickLaunchMission> missions;
        public ProtoLaunchVehicle launchVehicle;
        public ProtoVessel launchProtoVessel;

        public QuickLaunchVessel(ProtoVessel prevlaunchProtoVessel, ProtoLaunchVehicle previousLaunchVehicle)
        {
            launchProtoVessel = prevlaunchProtoVessel;
            launchVehicle = previousLaunchVehicle;
            missions = new List<QuickLaunchMission>();
        }

        public override int GetHashCode()
        {
            return launchVehicle.GetHashCode();
        }

        public void AddMission(string missionFile, ProtoVessel orbitProtoVessel)
        {
            missions.Add(new QuickLaunchMission(missionFile, orbitProtoVessel));
        }

        public List<QuickLaunchMission> GetMissions()
        {
            return missions;
        }

        internal ProtoVessel TMPGetLastVessel()
        {
            QuickLaunchMission lastMission = missions[missions.Count - 1];
            return lastMission.protoVessel;
        }
    }
}
