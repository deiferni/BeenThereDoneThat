namespace BeenThereDoneThat
{
    class QuickLaunchMissionTracker : VesselModule
    {
        [KSPField(isPersistant = true)]
        protected bool isTracking = false;

        public void SetActive()
        {
            isTracking = true;
        }
    }
}
