using UnityEngine;

namespace BeenThereDoneThat
{
    class SaveMissionDialog : MonoBehaviour
    {
        private Callback onDismissCallback;
        private PopupDialog saveDialog;
        private PopupDialog confirmDialog;

        private string launchVehicleName;
        private string saveFilename;
        private Vessel vessel;

        private static Vector2 anchorMin = new Vector2(0.5f, 0.5f);
        private static Vector2 anchorMax = new Vector2(0.5f, 0.5f);

        public static SaveMissionDialog Instance
        {
            get;
            private set;
        }

        public static SaveMissionDialog Create(Callback onDismissMenu, string launchVehicleName, Vessel vessel)
        {
            GameObject gameObject = new GameObject("BeenThereDoneThat mission save menu");
            SaveMissionDialog saveMissionDialog = gameObject.AddComponent<SaveMissionDialog>();

            saveMissionDialog.onDismissCallback = onDismissMenu;
            saveMissionDialog.launchVehicleName = launchVehicleName;
            saveMissionDialog.vessel = vessel;
            saveMissionDialog.saveFilename = vessel.GetDisplayName();

            return saveMissionDialog;
        }

        public void Awake()
        {
            Instance = this;
            FlightDriver.SetPause(true, true);
            InputLockManager.SetControlLock("BeenThereDoneThatSaveMissionDialog");
        }

        public void Start()
        {
            ShowSaveDialog();
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
            InputLockManager.RemoveControlLock("BeenThereDoneThatSaveMissionDialog");
            FlightDriver.SetPause(false, true);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (confirmDialog != null)
                {
                    DismissConfirmDialog();
                }
                else
                {
                    DismissSaveDialog();
                }
            }
            PreventUnpause();
        }

        private void LateUpdate()
        {
            PreventUnpause();
        }

        private void PreventUnpause()
        {
            if (saveDialog != null && Input.GetKeyUp(KeyCode.Escape) && !FlightDriver.Pause)
            {
                FlightDriver.SetPause(true, true);
            }
        }

        public void ShowSaveDialog()
        {          
            MultiOptionDialog dialog = new MultiOptionDialog("Save mission", "Save mission", "Save mission", UISkinManager.GetSkin("MainMenuSkin"),
                            new DialogGUITextInput(saveFilename, string.Empty, false, 64, delegate (string name)
                            {
                                saveFilename = name;
                                return saveFilename;
                            }, 24f), new DialogGUIButton("Save", delegate
                            {
                                ConfirmDialog();
                            }, false), new DialogGUIButton("Cancel", delegate
                            {
                                DismissSaveDialog();
                            }, true));
            saveDialog = PopupDialog.SpawnPopupDialog(anchorMin, anchorMax, dialog, false, null, true, string.Empty);
        }

        public void ConfirmDialog()
        {
            if (QuickLaunchHangar.Instance.Exists(launchVehicleName, saveFilename))
            {
                saveDialog.gameObject.SetActive(false);
                confirmDialog = PopupDialog.SpawnPopupDialog(anchorMin, anchorMax, new MultiOptionDialog("SavegameConfirmation", "Overwrite " + saveFilename, "Overwrite", UISkinManager.GetSkin("MainMenuSkin"), new DialogGUIButton("Overwrite", delegate
                {
                    QuickLaunchHangar.Instance.SaveVessel(vessel, launchVehicleName, saveFilename);
                    DismissConfirmDialog();
                    DismissSaveDialog();
                }, true), new DialogGUIButton("Cancel", delegate
                {
                    DismissConfirmDialog();
                }, true)), false, null, true, string.Empty);
                return;
            }
            if (!CheckFilename(saveFilename))
            {
                saveDialog.gameObject.SetActive(false);
                confirmDialog = PopupDialog.SpawnPopupDialog(anchorMin, anchorMax, new MultiOptionDialog("SavegameInvalidName", "Oh crap", "Invalid Filename!", UISkinManager.GetSkin("MainMenuSkin"), new DialogGUIButton("Oh crap", delegate
                {
                    DismissConfirmDialog();
                }, true)), false, null, true, string.Empty);
                return;
            }

            QuickLaunchHangar.Instance.SaveVessel(vessel, launchVehicleName, saveFilename);
            DismissSaveDialog();
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

        public void DismissSaveDialog()
        {
            if (saveDialog != null)
            {
                saveDialog.Dismiss();
                saveDialog = null;
            }
            onDismissCallback();
            Destroy(base.gameObject);
        }

        public void DismissConfirmDialog()
        {
            confirmDialog.Dismiss();
            confirmDialog = null;
            if (saveDialog != null)
            {
                saveDialog.gameObject.SetActive(true);
            }
        }
    }
}
