using UnityEngine;

namespace BeenThereDoneThat
{
    class StartMissionDialog : MonoBehaviour
    {
        private Callback onDismissCallback;
        private PopupDialog saveDialog;
        private PopupDialog confirmDialog;

        private string launchVehicleName;
        private Vessel vessel;
        private LaunchVehicle launchVehicle;
        private QuickLaunchMissionTracker tracker;

        private static Vector2 anchorMin = new Vector2(0.5f, 0.5f);
        private static Vector2 anchorMax = new Vector2(0.5f, 0.5f);

        public static StartMissionDialog Instance
        {
            get;
            private set;
        }

        public static StartMissionDialog Create(Callback onDismissMenu, Vessel vessel, LaunchVehicle launchVehicle, QuickLaunchMissionTracker tracker)
        {
            GameObject gameObject = new GameObject("BeenThereDoneThat mission start menu");
            StartMissionDialog startMissionDialog = gameObject.AddComponent<StartMissionDialog>();

            startMissionDialog.onDismissCallback = onDismissMenu;
            startMissionDialog.launchVehicleName = vessel.GetDisplayName();
            startMissionDialog.vessel = vessel;
            startMissionDialog.launchVehicle = launchVehicle;
            startMissionDialog.tracker = tracker;

            return startMissionDialog;
        }

        public void Awake()
        {
            Instance = this;
            FlightDriver.SetPause(true, true);
            InputLockManager.SetControlLock("BeenThereDoneThatStartMissionDialog");
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
            InputLockManager.RemoveControlLock("BeenThereDoneThatStartMissionDialog");
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
            MultiOptionDialog dialog = new MultiOptionDialog("Start mission", "Start mission", "Start mission", UISkinManager.GetSkin("MainMenuSkin"),
                            new DialogGUITextInput(launchVehicleName, string.Empty, false, 64, delegate (string name)
                            {
                                launchVehicleName = name;
                                return launchVehicleName;
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
            if (QuickLaunchHangar.Instance.ContainsLaunchVehicle(launchVehicleName))
            {
                saveDialog.gameObject.SetActive(false);
                confirmDialog = PopupDialog.SpawnPopupDialog(anchorMin, anchorMax, new MultiOptionDialog("SavegameConfirmation", "Overwrite " + launchVehicleName, "Overwrite", UISkinManager.GetSkin("MainMenuSkin"), new DialogGUIButton("Overwrite", delegate
                {
                    OnStartConfirmed();
                    DismissConfirmDialog();
                    DismissSaveDialog();
                }, true), new DialogGUIButton("Cancel", delegate
                {
                    DismissConfirmDialog();
                }, true)), false, null, true, string.Empty);
                return;
            }
            if (!CheckFilename(launchVehicleName))
            {
                saveDialog.gameObject.SetActive(false);
                confirmDialog = PopupDialog.SpawnPopupDialog(anchorMin, anchorMax, new MultiOptionDialog("SavegameInvalidName", "Oh crap", "Invalid Filename!", UISkinManager.GetSkin("MainMenuSkin"), new DialogGUIButton("Oh crap", delegate
                {
                    DismissConfirmDialog();
                }, true)), false, null, true, string.Empty);
                return;
            }

            OnStartConfirmed();
            DismissSaveDialog();
        }

        private void OnStartConfirmed()
        {
            QuickLaunchHangar.Instance.SaveLaunchVessel(vessel, launchVehicleName);
            launchVehicle.SaveAsSubmodule(launchVehicleName);
            tracker.StartTracking(launchVehicleName);
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
