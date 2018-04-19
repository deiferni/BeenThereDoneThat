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

        private string selectedMission = string.Empty;
        private UISkinDef skin;

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
            MultiOptionDialog missionList = new MultiOptionDialog("Start mission", string.Empty, "Start mission", UISkinManager.GetSkin("MainMenuSkin"), windowRect, this.CreateBrowserDialog());
            dialog = PopupDialog.SpawnPopupDialog(missionList, false, this.skin, true, string.Empty);
        }

        private DialogGUIBase CreateBrowserDialog()
        {
            DialogGUIContentSizer sizer = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            DialogGUIToggleGroup toggle = new DialogGUIToggleGroup(this.CreateBrowserItems());
            DialogGUIGridLayout layout = new DialogGUIGridLayout(new RectOffset(0, 4, 4, 4), new Vector2(320f, 64f), new Vector2(0f, 0f), GridLayoutGroup.Corner.UpperLeft, GridLayoutGroup.Axis.Horizontal, TextAnchor.UpperLeft, GridLayoutGroup.Constraint.FixedColumnCount, 1, sizer, toggle);
            DialogGUIScrollList scrollList = new DialogGUIScrollList(new Vector2(344f, 425f), false, true, layout);
            DialogGUIButton dialogGUIButton = new DialogGUIButton("Delete", delegate
            {
                //this.ShowDeleteFileConfirm();
            });
            dialogGUIButton.OptionInteractableCondition = delegate
            {
                // int result;
                int result = 0;
                //if (this.selectedEntry != null)
                //{
                //    while (true)
                //    {
                //        switch (3)
                //        {
                //            case 0:
                //                break;
                //            default:
                //                goto end_IL_0008;
                //        }
                //        continue;
                //        continue;
                //        end_IL_0008:
                //        break;
                //    }
                //    if (true)
                //    {
                //        ;
                //    }
                //    result = ((!this.selectedEntry.isStock) ? 1 : 0);
                //}
                //else
                //{
                //    result = 0;
                //}
                return (byte)result != 0;
            };
            DialogGUIButton dialogGUIButton2 = new DialogGUIButton("Cancel", delegate
            {
                //if (this.OnBrowseCancelled != null)
                //{
                //    while (true)
                //    {
                //        switch (1)
                //        {
                //            case 0:
                //                break;
                //            default:
                //                goto end_IL_0008;
                //        }
                //        continue;
                //        continue;
                //        end_IL_0008:
                //        break;
                //    }
                //    if (true)
                //    {
                //        ;
                //    }
                //    this.OnBrowseCancelled();
                //}
                //UnityEngine.Object.Destroy(base.gameObject);
            });
            DialogGUIButton dialogGUIButton3 = new DialogGUIButton("Load", delegate
            {
                //this.OnFileSelected(this.selectedEntry.fullFilePath, LoadType.Normal);
                //UnityEngine.Object.Destroy(base.gameObject);
            });
            //dialogGUIButton3.OptionInteractableCondition = (() => this.selectedEntry != null);
            DialogGUIVerticalLayout dialogGUIVerticalLayout = new DialogGUIVerticalLayout(true, true);
            dialogGUIVerticalLayout.AddChild(scrollList);
            dialogGUIVerticalLayout.AddChild(new DialogGUIHorizontalLayout(dialogGUIButton, dialogGUIButton2, dialogGUIButton3));
            return dialogGUIVerticalLayout;
        }

        private DialogGUIToggleButton[] CreateBrowserItems()
        {
            List<DialogGUIToggleButton> list = new List<DialogGUIToggleButton>();
            int i = 0;
            int craftIndex;
            DialogGUIVerticalLayout dialogGUIVerticalLayout;
            DialogGUIToggleButton dialogGUIToggleButton;
            string[] craftList = new string[] { "Click", "meeee", "clikkkkkk", "meee tooo!", "Click", "meeee", "clikkkkkk", "meee tooo!", "Click", "meeee", "clikkkkkk", "meee tooo!" };

            for (int count = craftList.Length; i < count; dialogGUIToggleButton = new DialogGUIToggleButton(() => "Click" == craftList[craftIndex], string.Empty, delegate
            {
                //this.SelectItem(craftIndex);
            }, -1f, 1f), dialogGUIToggleButton.AddChild(dialogGUIVerticalLayout), dialogGUIToggleButton.OptionInteractableCondition = (() => true), list.Add(dialogGUIToggleButton), i++)
            {

                craftIndex = i;
                dialogGUIVerticalLayout = new DialogGUIVerticalLayout(true, false, 0f, new RectOffset(4, 4, 4, 4), TextAnchor.UpperLeft);
                dialogGUIVerticalLayout.AddChild(new DialogGUILabel(craftList[i], this.skin.customStyles[0], true, false));
                DialogGUIVerticalLayout dialogGUIVerticalLayout2 = dialogGUIVerticalLayout;
                object[] obj = new object[7]
                {
                "<color=#ffffff>",
                1337,
                null,
                null,
                null,
                null,
                null
                };
                obj[2] = "things";
                obj[3] = " in ";
                obj[4] = 42;
                obj[5] = "other things";
                obj[6] = "</color>";
                dialogGUIVerticalLayout2.AddChild(new DialogGUILabel(string.Concat(obj), this.skin.customStyles[0], true, false));
                dialogGUIVerticalLayout.AddChild(new DialogGUILabel("<color=#ffffff>Cost: " + "very MUCH!!!!!" + "</color>", this.skin.customStyles[0], true, false));
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

        protected void OnSelectionChanged(bool haveSelection)
        {
        }

    }
}
