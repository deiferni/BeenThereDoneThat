using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BeenThereDoneThat
{
    [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
    public class QuickLaunchHangar : MonoBehaviour
    {
        public static string LAUNCHFILENAME = "VesselLaunch";

        private SaveMissionDialog saveMissionDialog;
        private StartMissionDialog startMissionDialog;
        private QuickLaunchMissionDialog quickLaunchMissionDialog;
        private Dictionary<int, QuickLaunchVessel> quickLaunchVessels;

        public static QuickLaunchHangar Instance
        {
            get;
            private set;
        }

        public void Awake()
        {
            Instance = this;
            InitQuickLaunchVessels();
        }

        private void InitQuickLaunchVessels()
        {
            quickLaunchVessels = new Dictionary<int, QuickLaunchVessel>();

            string directory = GetHangarPath();
            if (!Directory.Exists(directory))
            {
                return;
            }
            string[] launchVehicleDirectories = Directory.GetDirectories(directory);
            foreach (string launchVehiceDirectory in launchVehicleDirectories)
            {
                string launchVehicleFile = Path.Combine(launchVehiceDirectory, LAUNCHFILENAME + ".craft");
                if (!File.Exists(launchVehicleFile))
                {
                    continue;
                }
                ConfigNode prevLaunchRootNode = ConfigNode.Load(launchVehicleFile);
                ProtoVessel prevlaunchProtoVessel = new ProtoVessel(prevLaunchRootNode, HighLogic.CurrentGame);
                ProtoLaunchVehicle previousLaunchVehicle = null;
                ProtoPayload previousPayload = null;
                if (!QuickLauncher.Instance.Split(prevlaunchProtoVessel, out previousLaunchVehicle, out previousPayload))
                {
                    continue;
                }

                string[] parts = launchVehiceDirectory.Split(Path.DirectorySeparatorChar);
                string vesselName = parts[parts.Length-1];
                QuickLaunchVessel quickLaunchVessel = new QuickLaunchVessel(vesselName, prevlaunchProtoVessel, previousLaunchVehicle);
                quickLaunchVessels[quickLaunchVessel.GetHashCode()] = quickLaunchVessel;

                string missionsPath = Path.Combine(launchVehiceDirectory, "Missions");
                if (!Directory.Exists(missionsPath))
                {
                    continue;
                }
                foreach (string missionFilePath in Directory.GetFiles(missionsPath))
                {
                    ConfigNode orbitRootNode = ConfigNode.Load(missionFilePath);
                    ProtoVessel orbitProtoVessel = new ProtoVessel(orbitRootNode, HighLogic.CurrentGame);
                    ProtoLaunchVehicle orbitLaunchVehicle = null;
                    ProtoPayload orbitPayload = null;
                    if (QuickLauncher.Instance.Split(orbitProtoVessel, out orbitLaunchVehicle, out orbitPayload))
                    {
                        quickLaunchVessel.AddMission(missionFilePath, orbitProtoVessel);
                    }
                }
            }
        }

        public void OnSaveOrbitVessel(Vessel vessel, string launchVehicleName)
        {
            if (vessel.situation != Vessel.Situations.ORBITING)
            {
                ScreenMessages.PostScreenMessage("Can't save yet, must be orbiting!", 4, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            saveMissionDialog = SaveMissionDialog.Create(OnSaveMissionDialogDismiss, launchVehicleName, vessel);
        }

        public void OnSaveMissionDialogDismiss()
        {
            saveMissionDialog = null;
        }

        public void OnStartMission(Vessel vessel)
        {
            if (vessel.situation != Vessel.Situations.PRELAUNCH)
            {
                ScreenMessages.PostScreenMessage("Can't start, must be pre-launch!", 4, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            QuickLaunchMissionTracker tracker = vessel.GetComponent<QuickLaunchMissionTracker>();
            LaunchVehicle launchVehicle = null;
            Payload payload = null;

            if (tracker.isTracking)
            {
                ScreenMessages.PostScreenMessage("Already tracking!", 4, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            if (!QuickLauncher.Instance.Split(vessel.parts, out launchVehicle, out payload))
            {
                ScreenMessages.PostScreenMessage("No payload separator available", 4, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            QuickLaunchVessel quickLaunchVessel = null;
            if (quickLaunchVessels.TryGetValue(launchVehicle.GetHashCode(), out quickLaunchVessel))
            {
                string vesselName = quickLaunchVessel.name;
                tracker.StartTracking(vesselName);
                ScreenMessages.PostScreenMessage(string.Format("Started tracking {0}", vesselName), 4, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            else
            {
                startMissionDialog = StartMissionDialog.Create(OnStartMissionDialogDismiss, vessel, launchVehicle, tracker);
            }
        }

        public void Purge(string launchVehicleName)
        {
            Directory.Delete(GetLaunchVehiclePath(launchVehicleName), true);
        }

        public void OnStartMissionDialogDismiss()
        {
            startMissionDialog = null;
        }

        public void OnQuickLaunchMission(Vessel vessel)
        {
            LaunchVehicle launchVehicle = null;
            Payload payload = null;
            if (!QuickLauncher.Instance.Split(vessel.parts, out launchVehicle, out payload))
            {
                ScreenMessages.PostScreenMessage("No payload separator available", 4, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            int key = launchVehicle.GetHashCode();
            if (!quickLaunchVessels.ContainsKey(key))
            {
                ScreenMessages.PostScreenMessage("No missions available", 4, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            QuickLaunchVessel quickLaunchVessel = quickLaunchVessels[key];
            quickLaunchMissionDialog = QuickLaunchMissionDialog.Create(OnQuickLaunchDialogDismissed, quickLaunchVessel, vessel);
        }

        public void OnQuickLaunchDialogDismissed()
        {
            quickLaunchMissionDialog = null;
        }

        public ProtoVessel LoadLaunchProtoVessel(Vessel vessel)
        {
            LaunchVehicle launchVehicle = null;
            Payload payload = null;
            if (!QuickLauncher.Instance.Split(vessel.parts, out launchVehicle, out payload))
            {
                return null;
            }

            int key = launchVehicle.GetHashCode();
            if (!quickLaunchVessels.ContainsKey(key))
            {
                return null;
            }

            return quickLaunchVessels[key].launchProtoVessel;
        }

        public bool ContainsMissionVehicle(string launchVehicleName, string missionName)
        {
            return File.Exists(GetCraftPath(GetMissionsPath(launchVehicleName), missionName));
        }

        public bool ContainsLaunchVehicle(string launchVehicleName)
        {
            return Directory.Exists(GetLaunchVehiclePath(launchVehicleName));
        }

        public void SaveLaunchVessel(Vessel vessel, string launchVehicleName)
        {
            SaveVessel(vessel, GetLaunchVehiclePath(launchVehicleName), LAUNCHFILENAME);
        }

        public void SaveOrbitVessel(Vessel vessel, string launchVehicleName, string missionName)
        {
            SaveVessel(vessel, GetMissionsPath(launchVehicleName), missionName);
        }

        private void SaveVessel(Vessel vessel, string directory, string filename)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string path = GetCraftPath(directory, filename);
            ConfigNode node = new ConfigNode();
            vessel.protoVessel.Save(node);
            node.Save(path);
        }

        private string GetHangarPath()
        {
            return CombinePaths(KSPUtil.ApplicationRootPath, "saves", HighLogic.SaveFolder, "BeenThereDoneThat", "Hangar");
        }

        private string GetLaunchVehiclePath(string launchVehicleName)
        {
            return CombinePaths(GetHangarPath(), launchVehicleName);
        }

        private string GetMissionsPath(string launchVehicleName)
        {
            return CombinePaths(GetLaunchVehiclePath(launchVehicleName), "Missions");
        }

        private string GetCraftPath(string directory, string craftName)
        {
            return Path.Combine(directory, craftName.Trim() + ".craft");
        }

        private string CombinePaths(params string[] pathElements)
        {
            string path = string.Empty;
            foreach (string pathElement in pathElements)
            {
                path = Path.Combine(path, pathElement);
            }
            return path;
        }
    }
}
