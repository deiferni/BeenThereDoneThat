using System.Collections.Generic;
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

            string directory = GetLaunchVehicleDiretoryPath();
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

        public void SaveOrbitVessel(Vessel vessel, string launchVehicleName)
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

        public void SaveLaunchVessel(Vessel vessel, LaunchVehicle launchVehicle, QuickLaunchMissionTracker tracker)
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

        public void SaveVessel(Vessel vessel, string launchVehicleName, string filename)
        {
            ConfigNode node = new ConfigNode();
            vessel.protoVessel.Save(node);
            node.Save(MakeHangarPath(launchVehicleName, filename));
        }

        public string GetLaunchVehicleDiretoryPath()
        {
            string[] pathElements = new string[]
            {
                "saves",
                HighLogic.SaveFolder,
                "BeenThereDoneThat",
                "Missions",
            };
            string directory = KSPUtil.ApplicationRootPath;
            foreach (string pathElement in pathElements)
            {
                directory = Path.Combine(directory, pathElement);
            }
            return directory;
        }

        public string MakeLaunchVehicleDirectory(string launchVehicleName)
        {
            string directory = Path.Combine(GetLaunchVehicleDiretoryPath(), launchVehicleName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory;
        }

        public string MakeHangarPath(string launchVehicleName, string filename)
        {
            filename = filename.Trim() + ".craft";
            return Path.Combine(MakeLaunchVehicleDirectory(launchVehicleName), filename);
        }

        public bool Exists(string launchVehicleName, string filename)
        {
            string path = MakeHangarPath(launchVehicleName, filename);
            return File.Exists(path);
        }

        public bool ContainsLaunchVehicleDirectory(string launchVehicleName)
        {
            string directory = Path.Combine(GetLaunchVehicleDiretoryPath(), launchVehicleName);
            return Directory.Exists(directory);
        }
    }
}
