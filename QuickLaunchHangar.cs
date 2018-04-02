using System.IO;
using UnityEngine;

namespace BeenThereDoneThat
{
    [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
    public class QuickLaunchHangar : MonoBehaviour
    {

        private enum FilenameCheckResults
        {
            Empty,
            AlreadyExists,
            InvalidCharacters,
            Good
        }

        public static string ORBITFILENAME = "VesselOrbit.craft";
        public static string LAUNCHFILENAME = "VesselLaunch.craft";

        private PopupDialog confirmDialog;
        private PopupDialog saveDialog;
        private string saveFilename;

        public static QuickLaunchHangar Instance
        {
            get;
            private set;
        }

        public void Awake()
        {
            Instance = this;
        }

        public void SaveOrbitVessel(Vessel vessel)
        {
            if (vessel.situation != Vessel.Situations.ORBITING)
            {
                ScreenMessages.PostScreenMessage("Can't save yet, must be orbiting!", 4, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            Orbit orbit = vessel.orbit;
            CelestialBody body = vessel.mainBody;


            this.saveFilename = "VesselOrbit";
            MultiOptionDialog dialog = new MultiOptionDialog("Save mission", "Save mission", "Save mission", UISkinManager.GetSkin("MainMenuSkin"),
                            new DialogGUITextInput(this.saveFilename, false, 64, delegate (string name)
                            {
                                this.saveFilename = name;
                                return this.saveFilename;
                            }, 24f), new DialogGUIButton("Save", delegate
                            {
                                this.ConfirmDialog(vessel);
                            }, false), new DialogGUIButton("Cancel", delegate
                            {
                                this.onSaveDialogDismiss();
                            }, true));
            this.saveDialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), dialog, false, null, true, string.Empty);

            int selBodyIndex = FlightGlobals.Bodies.IndexOf(body);
            double sma = orbit.semiMajorAxis;
            double ecc = orbit.eccentricity;
            double inc = orbit.inclination;
            double LAN = orbit.LAN;
            double mna = orbit.meanAnomalyAtEpoch;
            double argPe = orbit.argumentOfPeriapsis;

            Debug.Log(
                string.Format("[BeenThereDoneThat]: REMEMBERING ORBIT> sma: {0} ecc: {1} inc: {2} LAN: {3} mna: {4} argPe: {5}",
                              sma, ecc, inc, LAN, mna, argPe));
        }

        public void ConfirmDialog(Vessel vessel)
        {
            string filename = this.saveFilename + ".craft";
            string path = MakeHangarPath(filename);
            if (File.Exists(path))
            {
                this.saveDialog.gameObject.SetActive(false);
                this.confirmDialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog("SavegameConfirmation", "Overwrite " + this.saveFilename, "Overwrite", UISkinManager.GetSkin("MainMenuSkin"), new DialogGUIButton("Overwrite", delegate
                {
                    SaveVessel(vessel, filename);
                    this.confirmDialog.Dismiss();
                    this.onSaveDialogDismiss();
                }, true), new DialogGUIButton("Cancel", delegate
                {
                    this.confirmDialog.Dismiss();
                    this.saveDialog.gameObject.SetActive(true);
                }, true)), false, null, true, string.Empty);
                return;
            }
            if (!this.CheckFilename(this.saveFilename))
            {
                this.confirmDialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog("SavegameInvalidName", "Oh crap", "Oh crap", UISkinManager.GetSkin("MainMenuSkin"), new DialogGUIButton("Oh crap", delegate
                {
                    this.confirmDialog.Dismiss();
                    this.saveDialog.gameObject.SetActive(true);
                }, true)), false, null, true, string.Empty);
                return;
            }

            SaveVessel(vessel, filename);
            onSaveDialogDismiss();
        }

        private bool CheckFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return false;
            }
            if (KSPUtil.SanitizeFilename(filename) != filename)
            {
                return false;
            }
            return true;
        }

        public void onSaveDialogDismiss()
        {
            this.saveDialog.Dismiss();
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

        protected static void SaveVessel(Vessel vessel, string filename)
        {
            ConfigNode node = new ConfigNode();
            vessel.protoVessel.Save(node);
            node.Save(MakeHangarPath(filename));
        }

        protected static string MakeHangarDirectory()
        {
            string directory = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/BeenThereDoneThat/";
            System.IO.Directory.CreateDirectory(directory);
            return directory;
        }

        protected static string MakeHangarPath(string filename)
        {
            return System.IO.Path.Combine(MakeHangarDirectory(), filename);
        }
    }
}
