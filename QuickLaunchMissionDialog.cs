using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BeenThereDoneThat
{
    class QuickLaunchMissionDialog : MonoBehaviour
    {
        private Callback onDismissCallback;
        private PopupDialog dialog;

        private static Vector2 anchorMin = new Vector2(0.5f, 0.5f);
        private static Vector2 anchorMax = new Vector2(0.5f, 0.5f);

        private string selectedMission = string.Empty;
        private UISkinDef skin;
        private List<DialogGUIToggleButton> items;

        public static QuickLaunchMissionDialog Instance
        {
            get;
            private set;
        }

        public static QuickLaunchMissionDialog Create(Callback onDismissCallback)
        {
            GameObject gameObject = new GameObject("BeenThereDoneThat mission start menu");
            QuickLaunchMissionDialog quickLaunchMissionDialog = gameObject.AddComponent<QuickLaunchMissionDialog>();

            quickLaunchMissionDialog.onDismissCallback = onDismissCallback;
            quickLaunchMissionDialog.skin = UISkinManager.GetSkin("MainMenuSkin");
            quickLaunchMissionDialog.items = new List<DialogGUIToggleButton>();

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
            CreateLoadList();
            MultiOptionDialog missionList = new MultiOptionDialog("Start mission", "Start mission", "Start mission", UISkinManager.GetSkin("MainMenuSkin"),
                            new DialogGUIVerticalLayout(200, 300, this.items.ToArray()),
                            new DialogGUIButton("Save", delegate
                            {
                                ConfirmDialog();
                            }, false), new DialogGUIButton("Cancel", delegate
                            {
                                DismissDialog();
                            }, true));
            dialog = PopupDialog.SpawnPopupDialog(anchorMin, anchorMax, missionList, false, null, true, string.Empty);
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

        protected void OnSelectionChanged(bool haveSelection)
        {
        }

        private void CreateLoadList()
        {
            DialogGUILabel.TextLabelOptions textLabelOptions = new DialogGUILabel.TextLabelOptions();
            textLabelOptions.enableWordWrapping = false;
            textLabelOptions.OverflowMode = TextOverflowModes.Overflow;
            textLabelOptions.resizeBestFit = true;
            textLabelOptions.resizeMinFontSize = 11;
            textLabelOptions.resizeMaxFontSize = 12;
            DialogGUILabel.TextLabelOptions textLabelOptions2 = textLabelOptions;

            foreach (string demo in new string[] { "Click", "meeee", "clikkkkkk", "meee tooo!" })
            {
                DialogGUIToggleButton dialogGUIToggleButton = new DialogGUIToggleButton(false, string.Empty, delegate (bool isActive)
                {
                    selectedMission = demo;
                    if (Mouse.Left.GetDoubleClick(true))
                    {
                        if (isActive)
                        {
                            this.ConfirmLoadGame();
                        }
                    }
                    this.OnSelectionChanged(true);
                }, -1f, 1f);
                dialogGUIToggleButton.guiStyle = this.skin.customStyles[8];
                dialogGUIToggleButton.OptionInteractableCondition = delegate
                {
                    return true;
                };
                DialogGUILabel dialogGUILabel = new DialogGUILabel(demo, this.skin.customStyles[0], true, false);
                dialogGUILabel.textLabelOptions = new DialogGUILabel.TextLabelOptions
                {
                    resizeBestFit = true,
                    resizeMinFontSize = 11,
                    resizeMaxFontSize = 12,
                    enableWordWrapping = false,
                    OverflowMode = TextOverflowModes.Ellipsis
                };
                DialogGUIVerticalLayout dialogGUIVerticalLayout = new DialogGUIVerticalLayout(true, false, 0f, new RectOffset(), TextAnchor.UpperLeft, dialogGUILabel);
                dialogGUIVerticalLayout.AddChild(new DialogGUILabel("XXX " + demo, this.skin.customStyles[1], true, false)
                {
                    textLabelOptions = textLabelOptions2
                });
                dialogGUIVerticalLayout.AddChild(new DialogGUILabel("YYY " + demo, this.skin.customStyles[2], true, false)
                {
                    textLabelOptions = textLabelOptions2
                });

                dialogGUIToggleButton.AddChild(new DialogGUIHorizontalLayout(false, false, 4f, new RectOffset(0, 8, 6, 7), TextAnchor.MiddleLeft, dialogGUIVerticalLayout));
                this.items.Add(dialogGUIToggleButton);
            }

            int count = items.Count;
            for (int j = 0; j<count; j++)
            {
                DialogGUIToggleButton dialogGUIToggleButton2 = this.items[j];
                Stack<Transform> stack = new Stack<Transform>();
                stack.Push(base.transform);
                                    GameObject gameObject = dialogGUIToggleButton2.Create(ref stack, this.skin);
                // XXX gameObject.transform.SetParent(this.scrollListContent, false);
                // XXX dialogGUIToggleButton2.toggle.group = this.listGroup;
            }
        }
    }
}
