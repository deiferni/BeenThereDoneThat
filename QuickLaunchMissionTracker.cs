namespace BeenThereDoneThat
{
    public class QuickLaunchMissionTracker : VesselModule
    {
        [KSPField(isPersistant = true)]
        public bool isTracking = false;

        [KSPField(isPersistant = true)]
        public string launchVehicleName = string.Empty;

        public void startTracking(string name)
        {
            isTracking = true;
            launchVehicleName = name;
        }
    }
}
