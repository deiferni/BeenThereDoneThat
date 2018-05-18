using UnityEngine;

namespace BeenThereDoneThat
{
    class SaveMissionDialog : MonoBehaviour
    {
        private Callback onDismissCallback;
        private PopupDialog saveDialog;
        private PopupDialog confirmDialog;

        private string launchVehicleName;
        private string missionName;
        private Vessel vessel;

        private static Vector2 anchorMin = new Vector2(0.5f, 0.5f);
        private static Vector2 anchorMax = new Vector2(0.5f, 0.5f);

        private static double CONSIDERED_CIRCULAR = 0.025;
        private static string[] ALTITUDE_UNITS = new string[]{"m", "K", "M", "G", "T", "P", "E", "Z", "Y", "X"};

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
            saveMissionDialog.missionName = GetMissionName(vessel);

            return saveMissionDialog;
        }

        private static string GetMissionName(Vessel vessel)
        {
            Orbit orbit = vessel.GetOrbit();
            string missionName = string.Format("{0} - {1}", orbit.referenceBody.name, GetOrbitAltitude(orbit.ApA));
            if (orbit.eccentricity > CONSIDERED_CIRCULAR)
            {
                missionName = string.Format("{0} - {1}", missionName, GetOrbitAltitude(orbit.PeA));
            }
            return missionName;
        }

        private static string GetOrbitAltitude(double altitude)
        {
            int alt = (int)altitude;
            foreach (string unit in ALTITUDE_UNITS)
            {
                if (alt < 1000)
                {
                    return string.Format("{0}{1}", alt, unit);
                }
                alt /= 1000;
            }
            return "Pretty high";
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
                            new DialogGUITextInput(missionName, string.Empty, false, 64, delegate (string name)
                            {
                                missionName = name;
                                return missionName;
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
            if (QuickLaunchHangar.Instance.ContainsMissionVehicle(launchVehicleName, missionName))
            {
                saveDialog.gameObject.SetActive(false);
                confirmDialog = PopupDialog.SpawnPopupDialog(anchorMin, anchorMax, new MultiOptionDialog("SavegameConfirmation", "Overwrite " + missionName, "Overwrite", UISkinManager.GetSkin("MainMenuSkin"), new DialogGUIButton("Overwrite", delegate
                {
                    OnSaveConfirmed();
                    DismissConfirmDialog();
                    DismissSaveDialog();
                }, true), new DialogGUIButton("Cancel", delegate
                {
                    DismissConfirmDialog();
                }, true)), false, null, true, string.Empty);
                return;
            }
            if (!CheckFilename(missionName))
            {
                saveDialog.gameObject.SetActive(false);
                confirmDialog = PopupDialog.SpawnPopupDialog(anchorMin, anchorMax, new MultiOptionDialog("SavegameInvalidName", "Oh crap", "Invalid Filename!", UISkinManager.GetSkin("MainMenuSkin"), new DialogGUIButton("Oh crap", delegate
                {
                    DismissConfirmDialog();
                }, true)), false, null, true, string.Empty);
                return;
            }

            OnSaveConfirmed();
            DismissSaveDialog();
        }

        private void OnSaveConfirmed()
        {
            QuickLaunchHangar.Instance.SaveOrbitVessel(vessel, launchVehicleName, missionName);
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
