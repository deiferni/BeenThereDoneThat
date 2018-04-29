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
        private string selectedMission = string.Empty;
        private UISkinDef skin;
        private int selectedEntry;

        public static QuickLaunchMissionDialog Instance
        {
            get;
            private set;
        }

        public static QuickLaunchMissionDialog Create(Callback onDismissCallback, QuickLaunchVessel quickLaunchVessel)
        {
            GameObject gameObject = new GameObject("BeenThereDoneThat mission start menu");
            QuickLaunchMissionDialog quickLaunchMissionDialog = gameObject.AddComponent<QuickLaunchMissionDialog>();

            quickLaunchMissionDialog.onDismissCallback = onDismissCallback;
            quickLaunchMissionDialog.skin = UISkinManager.GetSkin("MainMenuSkin");
            quickLaunchMissionDialog.quickLaunchVessel = quickLaunchVessel;
            quickLaunchMissionDialog.selectedEntry = -1;

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
            Rect windowRect = new Rect(0.5f, 0.5f, 350f, 500f);
            MultiOptionDialog missionList = new MultiOptionDialog("Start mission", string.Empty, "Start mission", skin, windowRect, this.CreateBrowserDialog());
            dialog = PopupDialog.SpawnPopupDialog(missionList, false, skin, true, string.Empty);
        }

        private DialogGUIBase CreateBrowserDialog()
        {
            DialogGUIContentSizer sizer = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            DialogGUIToggleGroup toggle = new DialogGUIToggleGroup(this.CreateBrowserItems());
            DialogGUIGridLayout layout = new DialogGUIGridLayout(new RectOffset(0, 4, 4, 4), new Vector2(320f, 64f), new Vector2(0f, 0f), GridLayoutGroup.Corner.UpperLeft, GridLayoutGroup.Axis.Horizontal, TextAnchor.UpperLeft, GridLayoutGroup.Constraint.FixedColumnCount, 1, sizer, toggle);
            DialogGUIScrollList scrollList = new DialogGUIScrollList(new Vector2(344f, 425f), false, true, layout);

            DialogGUIButton deleteButton = new DialogGUIButton("Delete", delegate
            {
                //this.ShowDeleteFileConfirm();
            });
            deleteButton.OptionInteractableCondition = (() => selectedEntry >= 0);

            DialogGUIButton cancelButton = new DialogGUIButton("Cancel", delegate
            {
                DismissDialog();
            });

            DialogGUIButton loadButton = new DialogGUIButton("Load", delegate
            {
                //this.OnFileSelected(this.selectedEntry.fullFilePath, LoadType.Normal);
                //UnityEngine.Object.Destroy(base.gameObject);
            });
            loadButton.OptionInteractableCondition = (() => selectedEntry >= 0);

            DialogGUIVerticalLayout dialogGUIVerticalLayout = new DialogGUIVerticalLayout(true, true);
            dialogGUIVerticalLayout.AddChild(scrollList);
            dialogGUIVerticalLayout.AddChild(new DialogGUIHorizontalLayout(deleteButton, cancelButton, loadButton));
            return dialogGUIVerticalLayout;
        }

        private DialogGUIToggleButton[] CreateBrowserItems()
        {
            List<DialogGUIToggleButton> list = new List<DialogGUIToggleButton>();
            int i = 0;
            int craftIndex;
            DialogGUIVerticalLayout dialogGUIVerticalLayout;
            DialogGUIToggleButton dialogGUIToggleButton;

            foreach (QuickLaunchMission mission in quickLaunchVessel.GetMissions())
            {
                craftIndex = i;
                dialogGUIToggleButton = new DialogGUIToggleButton(false, string.Empty, delegate
                {
                    this.SelectItem(craftIndex);
                }, -1f, 1f);

                dialogGUIVerticalLayout = new DialogGUIVerticalLayout(true, false, 0f, new RectOffset(4, 4, 4, 4), TextAnchor.UpperLeft);
                dialogGUIVerticalLayout.AddChild(new DialogGUILabel(mission.missionName, skin.customStyles[0], true, false));
                DialogGUIVerticalLayout dialogGUIVerticalLayout2 = dialogGUIVerticalLayout;
                OrbitSnapshot orbit = mission.protoVessel.orbitSnapShot;
                CelestialBody body = FlightGlobals.Bodies[orbit.ReferenceBodyIndex];
                string orbitBody = string.Format("<color=#ffffff>Orbiting {0}</color>", body.name);
                dialogGUIVerticalLayout2.AddChild(new DialogGUILabel(orbitBody, skin.customStyles[0], true, false));
                string orbitInfo = string.Format("<color=#ffffff>SMA: {0}, Inc: {1}</color>", (int)orbit.semiMajorAxis, Math.Round(orbit.inclination, 3));
                dialogGUIVerticalLayout.AddChild(new DialogGUILabel(orbitInfo, skin.customStyles[0], true, false));

                dialogGUIToggleButton.AddChild(dialogGUIVerticalLayout);
                dialogGUIToggleButton.OptionInteractableCondition = (() => true);
                list.Add(dialogGUIToggleButton);
                i++;
            }
            return list.ToArray();
        }

        public void ConfirmDialog()
        {
            DismissDialog();
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

        private void ConfirmLoadGame()
        {
            // XXX onDismissCallback(selectedMission);
            DismissDialog();
        }

        private void SelectItem(int craftIndex)
        {
            selectedEntry = craftIndex;
        }

        protected void OnSelectionChanged(bool haveSelection)
        {
        }

    }
}
