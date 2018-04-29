using System.IO;
using UnityEngine;

namespace BeenThereDoneThat
{
    public class QuickLaunchMission
    {
        public string missionName;
        public string missionFilePath;
        public ProtoVessel protoVessel;
        private QuickLaunchVessel quickLaunchVessel;

        public QuickLaunchMission(QuickLaunchVessel parent, string filePath, ProtoVessel orbitProtoVessel)
        {
            missionFilePath = filePath;
            missionName = Path.GetFileNameWithoutExtension(filePath);
            protoVessel = orbitProtoVessel;
            quickLaunchVessel = parent;
        }

        public void Launch(Vessel vessel)
        {
            ProtoVessel prevlaunchProtoVessel = QuickLaunchHangar.Instance.LoadLaunchProtoVessel(vessel);

            if (prevlaunchProtoVessel == null)
            {
                Debug.Log("[BeenThereDoneThat]: No previously launched vessel found, aborting");
                return;
            }

            if (protoVessel == null)
            {
                Debug.Log("[BeenThereDoneThat]: No orbit vessel found, aborting");
                return;
            }

            new QuickLauch(vessel, prevlaunchProtoVessel, protoVessel).Liftoff();
        }

        public void Delete()
        {
            quickLaunchVessel.RemoveMission(this);
            File.Delete(missionFilePath);
        }
    }
}
