namespace BeenThereDoneThat
{
    public class QuickLaunchMission
    {
        public string missionName;
        public ProtoVessel protoVessel;

        public QuickLaunchMission(string name, ProtoVessel orbitProtoVessel)
        {
            missionName = name;
            protoVessel = orbitProtoVessel;
        }
    }
}
