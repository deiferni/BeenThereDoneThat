﻿using System.Collections.Generic;

namespace BeenThereDoneThat
{
    public class QuickLaunchVessel
    {
        public List<QuickLaunchMission> missions;
        public ProtoLaunchVehicle launchVehicle;
        public ProtoVessel launchProtoVessel;
        public string name;

        public QuickLaunchVessel(string vesselName, ProtoVessel prevlaunchProtoVessel, ProtoLaunchVehicle previousLaunchVehicle)
        {
            name = vesselName;
            launchProtoVessel = prevlaunchProtoVessel;
            launchVehicle = previousLaunchVehicle;
            missions = new List<QuickLaunchMission>();
        }

        public override int GetHashCode()
        {
            return launchVehicle.GetHashCode();
        }

        public void AddMission(string missionFilePath, ProtoVessel orbitProtoVessel)
        {
            missions.Add(new QuickLaunchMission(this, missionFilePath, orbitProtoVessel));
        }

        public List<QuickLaunchMission> GetMissions()
        {
            return missions;
        }

        internal void RemoveMission(QuickLaunchMission quickLaunchMission)
        {
            missions.Remove(quickLaunchMission);
        }
    }
}
