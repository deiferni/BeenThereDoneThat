using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeenThereDoneThat
{
    class QuickLaunchMissionDialog : MonoBehaviour
    {
        private Callback onDismissCallback;
        private PopupDialog dialog;

        private static Vector2 anchorMin = new Vector2(0.5f, 0.5f);
        private static Vector2 anchorMax = new Vector2(0.5f, 0.5f);

        private QuickLaunchVessel quickLaunchVessel;
        private Vessel vessel;
        private UISkinDef skin;
        private int selectedMissionIndex;
        private PopupDialog confirmDeleteDialog;

        public static QuickLaunchMissionDialog Instance
        {
            get;
            private set;
        }

        public static QuickLaunchMissionDialog Create(Callback onDismissCallback, QuickLaunchVessel quickLaunchVessel, Vessel vessel)
        {
            GameObject gameObject = new GameObject("BeenThereDoneThat mission start menu");
            QuickLaunchMissionDialog quickLaunchMissionDialog = gameObject.AddComponent<QuickLaunchMissionDialog>();

            quickLaunchMissionDialog.onDismissCallback = onDismissCallback;
            quickLaunchMissionDialog.skin = UISkinManager.GetSkin("MainMenuSkin");
            quickLaunchMissionDialog.quickLaunchVessel = quickLaunchVessel;
            quickLaunchMissionDialog.vessel = vessel;
            quickLaunchMissionDialog.selectedMissionIndex = -1;

            return quickLaunchMissionDialog;
        }

        public void Awake()
        {
            Instance = this;
            FlightDriver.SetPause(true, true);
            InputLockManager.SetControlLock("BeenThereDoneThatQuickLaunchMissionDialog");
        }

        public void Start()
        {
            ShowDialog();
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
            InputLockManager.RemoveControlLock("BeenThereDoneThatQuickLaunchMissionDialog");
            FlightDriver.SetPause(false, true);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                DismissDialog();
            }
            PreventUnpause();
        }

        private void LateUpdate()
        {
            PreventUnpause();
        }

        private void PreventUnpause()
        {
            if (dialog != null && Input.GetKeyUp(KeyCode.Escape) && !FlightDriver.Pause)
            {
                FlightDriver.SetPause(true, true);
            }
        }

        public void ShowDialog()
        {
            if (dialog != null)
            {
                dialog.Dismiss();
            }
            Rect windowRect = new Rect(0.5f, 0.5f, 350f, 500f);
            MultiOptionDialog missionList = new MultiOptionDialog("Start mission", string.Empty, "Start mission", skin, windowRect, this.CreateMissionListDialog());
            dialog = PopupDialog.SpawnPopupDialog(missionList, false, skin, true, string.Empty);
        }

        private DialogGUIBase CreateMissionListDialog()
        {
            DialogGUIContentSizer sizer = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            DialogGUIToggleGroup toggle = new DialogGUIToggleGroup(this.CreateMissionListItems());
            DialogGUIGridLayout layout = new DialogGUIGridLayout(new RectOffset(0, 4, 4, 4), new Vector2(320f, 64f), new Vector2(0f, 0f), GridLayoutGroup.Corner.UpperLeft, GridLayoutGroup.Axis.Horizontal, TextAnchor.UpperLeft, GridLayoutGroup.Constraint.FixedColumnCount, 1, sizer, toggle);
            DialogGUIScrollList scrollList = new DialogGUIScrollList(new Vector2(344f, 425f), false, true, layout);

            DialogGUIButton deleteButton = new DialogGUIButton("Delete", delegate
            {
                ShowDeleteMissionConfirmDialog();
            });
            deleteButton.OptionInteractableCondition = (() => selectedMissionIndex >= 0);

            DialogGUIButton cancelButton = new DialogGUIButton("Cancel", delegate
            {
                DismissDialog();
            });

            DialogGUIButton loadButton = new DialogGUIButton("Load", delegate
            {
                OnMissionSelected();
                DismissDialog();
            });
            loadButton.OptionInteractableCondition = (() => selectedMissionIndex >= 0);

            DialogGUIVerticalLayout dialogGUIVerticalLayout = new DialogGUIVerticalLayout(true, true);
            dialogGUIVerticalLayout.AddChild(scrollList);
            dialogGUIVerticalLayout.AddChild(new DialogGUIHorizontalLayout(deleteButton, cancelButton, loadButton));
            return dialogGUIVerticalLayout;
        }

        private DialogGUIToggleButton[] CreateMissionListItems()
        {
            List<DialogGUIToggleButton> list = new List<DialogGUIToggleButton>();
            DialogGUIVerticalLayout dialogGUIVerticalLayout;
            DialogGUIToggleButton dialogGUIToggleButton;
            List<QuickLaunchMission> missions = quickLaunchVessel.GetMissions();

            for (int i = 0; i < missions.Count; i++)
            {
                int missionIndex = i;
                QuickLaunchMission mission = missions[missionIndex];

                dialogGUIToggleButton = new DialogGUIToggleButton(false, string.Empty, delegate
                {
                    SelectItem(missionIndex);
                }, -1f, 1f);

                dialogGUIVerticalLayout = new DialogGUIVerticalLayout(true, false, 0f, new RectOffset(4, 4, 4, 4), TextAnchor.UpperLeft);
                dialogGUIVerticalLayout.AddChild(new DialogGUILabel(mission.missionName, skin.customStyles[0], true, false));
                DialogGUIVerticalLayout dialogGUIVerticalLayout2 = dialogGUIVerticalLayout;
                Orbit orbit = mission.protoVessel.orbitSnapShot.Load();
                CelestialBody body = orbit.referenceBody;
                string orbitBody = string.Format("<color=#ffffff>Orbiting {0}</color>", body.name);
                dialogGUIVerticalLayout2.AddChild(new DialogGUILabel(orbitBody, skin.customStyles[0], true, false));
                string orbitInfo = string.Format("<color=#ffffff>Ap: {0}, Pe: {1}, Inc: {2}</color>", Utils.FormatAltitude(orbit.ApA), Utils.FormatAltitude(orbit.PeA), Math.Round(orbit.inclination, 3));
                dialogGUIVerticalLayout.AddChild(new DialogGUILabel(orbitInfo, skin.customStyles[0], true, false));

                dialogGUIToggleButton.AddChild(dialogGUIVerticalLayout);
                dialogGUIToggleButton.OptionInteractableCondition = (() => true);
                list.Add(dialogGUIToggleButton);
            }
            return list.ToArray();
        }

        public void OnMissionSelected()
        {
            GetSelectedMission().Launch(vessel);
        }

        private void ShowDeleteMissionConfirmDialog()
        {
            if (dialog != null)
            {
                dialog.gameObject.SetActive(false);
            }

            confirmDeleteDialog = PopupDialog.SpawnPopupDialog(anchorMin, anchorMax, new MultiOptionDialog("ConfirmFileDelete", string.Empty, "Delete Mission", skin, new DialogGUIButton("Delete", delegate
            {
                GetSelectedMission().Delete();
                DismissConfirmDialog();
                ShowDialog();
            }, true), new DialogGUIButton("Cancel", delegate
            {
                DismissConfirmDialog();
            }, true)), false, null, true, string.Empty);
        }

        public void DismissConfirmDialog()
        {
            confirmDeleteDialog.Dismiss();
            confirmDeleteDialog = null;
            if (dialog != null)
            {
                dialog.gameObject.SetActive(true);
            }
        }

        public void DismissDialog()
        {
            if (dialog != null)
            {
                dialog.Dismiss();
                dialog = null;
            }
            onDismissCallback();
            Destroy(base.gameObject);
        }

        private void SelectItem(int missionIndex)
        {
            selectedMissionIndex = missionIndex;
            Debug.Log(string.Format("[BeenThereDoneThat]: Selected mission {0}", missionIndex));
        }

        private QuickLaunchMission GetSelectedMission()
        {
            Debug.Log(string.Format("[BeenThereDoneThat]: Returning mission at {0}", selectedMissionIndex));
            return quickLaunchVessel.GetMissions()[selectedMissionIndex];
        }
    }
}
