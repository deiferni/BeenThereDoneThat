using System.IO;
using UnityEngine;

namespace BeenThereDoneThat
{
    class SaveMissionDialog : MonoBehaviour
    {
        private Callback onDismiss;
        private PopupDialog saveDialog;
        private PopupDialog confirmDialog;

        private string saveFilename;
        private Vessel vessel;

        private static Vector2 anchorMin = new Vector2(0.5f, 0.5f);
        private static Vector2 anchorMax = new Vector2(0.5f, 0.5f);

        public static SaveMissionDialog Instance
        {
            get;
            private set;
        }

        public static SaveMissionDialog Create(Callback onDismissMenu, Vessel vessel)
        {
            GameObject gameObject = new GameObject("BeenThereDoneThat mission save menu");
            SaveMissionDialog saveMissionDialog = gameObject.AddComponent<SaveMissionDialog>();
            SaveMissionDialog.Instance = saveMissionDialog;

            saveMissionDialog.onDismiss = onDismissMenu;
            saveMissionDialog.vessel = vessel;

            saveMissionDialog.ShowSaveDialog();
            return saveMissionDialog;
        }

        public void Awake()
        {
            Instance = this;
            FlightDriver.SetPause(true, true);
            InputLockManager.SetControlLock("BeenThereDoneThat SaveMissionDialog");
        }

        private void OnDestroy()
        {
            if (Instance != null)
            {
                if (Instance == this)
                {
                    Instance = null;
                }
            }
            InputLockManager.RemoveControlLock("BeenThereDoneThat SaveMissionDialog");
            FlightDriver.SetPause(false, true);
        }

        private void Update()
        {
            if (saveDialog != null)
            {
                return;
            }
        }


        public void ShowSaveDialog()
        {          
            saveFilename = vessel.GetDisplayName();
            MultiOptionDialog dialog = new MultiOptionDialog("Save mission", "Save mission", "Save mission", UISkinManager.GetSkin("MainMenuSkin"),
                            new DialogGUITextInput(this.saveFilename, false, 64, delegate (string name)
                            {
                                this.saveFilename = name;
                                return this.saveFilename;
                            }, 24f), new DialogGUIButton("Save", delegate
                            {
                                this.ConfirmDialog();
                            }, false), new DialogGUIButton("Cancel", delegate
                            {
                                this.Dismiss();
                            }, true));
            this.saveDialog = PopupDialog.SpawnPopupDialog(anchorMin, anchorMax, dialog, false, null, true, string.Empty);
        }

        public void ConfirmDialog()
        {
            string filename = this.saveFilename + ".craft";
            string path = QuickLaunchHangar.MakeHangarPath(filename);
            if (File.Exists(path))
            {
                this.saveDialog.gameObject.SetActive(false);
                this.confirmDialog = PopupDialog.SpawnPopupDialog(anchorMin, anchorMax, new MultiOptionDialog("SavegameConfirmation", "Overwrite " + this.saveFilename, "Overwrite", UISkinManager.GetSkin("MainMenuSkin"), new DialogGUIButton("Overwrite", delegate
                {
                    QuickLaunchHangar.SaveVessel(vessel, filename);
                    this.confirmDialog.Dismiss();
                    this.Dismiss();
                }, true), new DialogGUIButton("Cancel", delegate
                {
                    this.confirmDialog.Dismiss();
                    this.saveDialog.gameObject.SetActive(true);
                }, true)), false, null, true, string.Empty);
                return;
            }
            if (!this.CheckFilename(this.saveFilename))
            {
                this.confirmDialog = PopupDialog.SpawnPopupDialog(anchorMin, anchorMax, new MultiOptionDialog("SavegameInvalidName", "Oh crap", "Oh crap", UISkinManager.GetSkin("MainMenuSkin"), new DialogGUIButton("Oh crap", delegate
                {
                    this.confirmDialog.Dismiss();
                    this.saveDialog.gameObject.SetActive(true);
                }, true)), false, null, true, string.Empty);
                return;
            }

            QuickLaunchHangar.SaveVessel(vessel, filename);
            Dismiss();
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

        public void Dismiss()
        {
            onDismiss();
            if (saveDialog != null)
            {
                saveDialog.Dismiss();
            }
            Destroy(base.gameObject);
        }
    }
}
