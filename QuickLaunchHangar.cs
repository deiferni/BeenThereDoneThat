using System.IO;
using UnityEngine;

namespace BeenThereDoneThat
{
    [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
    public class QuickLaunchHangar : MonoBehaviour
    {
        public static string ORBITFILENAME = "VesselOrbit.craft";
        public static string LAUNCHFILENAME = "VesselLaunch.craft";

        private SaveMissionDialog saveMissionDialog;

        public static QuickLaunchHangar Instance
        {
            get;
            private set;
        }

        public void Awake()
        {
            Instance = this;
        }

        public void OnSaveMissionDialogDismiss()
        {
            saveMissionDialog = null;
        }

        public void SaveOrbitVessel(Vessel vessel)
        {
            if (vessel.situation != Vessel.Situations.ORBITING)
            {
                ScreenMessages.PostScreenMessage("Can't save yet, must be orbiting!", 4, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            saveMissionDialog = SaveMissionDialog.Create(OnSaveMissionDialogDismiss, vessel);
        }

        public static ProtoVessel LoadOrbitProtoVessel()
        {
            ConfigNode subModuleRootNode = ConfigNode.Load(MakeHangarPath(ORBITFILENAME));
            return new ProtoVessel(subModuleRootNode, HighLogic.CurrentGame);
        }

        public static ProtoVessel LoadLaunchProtoVessel()
        {
            ConfigNode subModuleRootNode = ConfigNode.Load(MakeHangarPath(LAUNCHFILENAME));
            return new ProtoVessel(subModuleRootNode, HighLogic.CurrentGame);
        }

        public static void SaveLaunchVessel(Vessel vessel)
        {
            SaveVessel(vessel, LAUNCHFILENAME);
        }

        public static void SaveVessel(Vessel vessel, string filename)
        {
            ConfigNode node = new ConfigNode();
            vessel.protoVessel.Save(node);
            node.Save(MakeHangarPath(filename));
        }

        public static string MakeHangarDirectory()
        {
            string directory = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/BeenThereDoneThat/";
            System.IO.Directory.CreateDirectory(directory);
            return directory;
        }

        public static string MakeHangarPath(string filename)
        {
            return System.IO.Path.Combine(MakeHangarDirectory(), filename);
        }
    }
}
