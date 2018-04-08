﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BeenThereDoneThat
{
    [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
    public class QuickLaunchHangar : MonoBehaviour
    {
        public static string ORBITFILENAME = "VesselOrbit";
        public static string LAUNCHFILENAME = "VesselLaunch";

        private SaveMissionDialog saveMissionDialog;
        private StartMissionDialog startMissionDialog;
        private Dictionary<int, ProtoVessel> prevLaunchVessels;
        private Dictionary<int, ProtoVessel> orbitVessels;

        public static QuickLaunchHangar Instance
        {
            get;
            private set;
        }

        public void Awake()
        {
            Instance = this;
            InitLaunchVehicles();
        }

        private void InitLaunchVehicles()
        {
            prevLaunchVessels = new Dictionary<int, ProtoVessel>();
            orbitVessels = new Dictionary<int, ProtoVessel>();

            string directory = GetHangarPath();
            if (!Directory.Exists(directory))
            {
                return;
            }
            string[] launchVehicleDirectories = Directory.GetDirectories(directory);
            foreach (string launchVehiceDirectory in launchVehicleDirectories)
            {
                string filename = Path.Combine(launchVehiceDirectory, LAUNCHFILENAME + ".craft");
                int key;
                if (File.Exists(filename))
                {
                    ConfigNode prevLaunchRootNode = ConfigNode.Load(filename);
                    ProtoVessel prevlaunchProtoVessel = new ProtoVessel(prevLaunchRootNode, HighLogic.CurrentGame);
                    ProtoLaunchVehicle previousLaunchVehicle = null;
                    ProtoPayload previousPayload = null;
                    if (QuickLauncher.Instance.Split(prevlaunchProtoVessel, out previousLaunchVehicle, out previousPayload))
                    {
                        prevLaunchVessels[previousLaunchVehicle.GetHashCode()] = prevlaunchProtoVessel;
                        key = previousLaunchVehicle.GetHashCode();

                        filename = Path.Combine(launchVehiceDirectory, ORBITFILENAME + ".craft");
                        if (File.Exists(filename))
                        {
                            ConfigNode orbitRootNode = ConfigNode.Load(filename);
                            ProtoVessel orbitProtoVessel = new ProtoVessel(orbitRootNode, HighLogic.CurrentGame);
                            ProtoLaunchVehicle orbitLaunchVehicle = null;
                            ProtoPayload orbitPayload = null;
                            if (QuickLauncher.Instance.Split(orbitProtoVessel, out orbitLaunchVehicle, out orbitPayload))
                            {
                                orbitVessels[key] = orbitProtoVessel;
                            }
                        }
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

        public void OnSaveLaunchVessel(Vessel vessel, LaunchVehicle launchVehicle, QuickLaunchMissionTracker tracker)
        {
            if (vessel.situation != Vessel.Situations.PRELAUNCH)
            {
                ScreenMessages.PostScreenMessage("Can't start, must be pre-launch!", 4, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            startMissionDialog = StartMissionDialog.Create(OnStartMissionDialogDismiss, vessel, launchVehicle, tracker);
        }

        public void OnStartMissionDialogDismiss()
        {
            startMissionDialog = null;
        }

        public ProtoVessel LoadOrbitProtoVessel(Vessel vessel)
        {
            LaunchVehicle launchVehicle = null;
            Payload payload = null;
            if (!QuickLauncher.Instance.Split(vessel.parts, out launchVehicle, out payload))
            {
                return null;
            }

            int key = launchVehicle.GetHashCode();
            if (!orbitVessels.ContainsKey(key))
            {
                return null;
            }
            return orbitVessels[key];
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
            if (!prevLaunchVessels.ContainsKey(key))
            {
                return null;
            }

            return prevLaunchVessels[key];
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
