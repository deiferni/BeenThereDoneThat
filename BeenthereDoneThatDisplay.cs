using KSP.UI.Screens;
using UnityEngine;

namespace BeenThereDoneThat
{
    [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
    class BeenthereDoneThatButtons : MonoBehaviour
    {
        static bool buttonAdded = false;
        static ApplicationLauncherButton beenThereDoneThatButton = null;
        static ApplicationLauncher.AppScenes buttonScenes = ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT;

        private GUIStyle windowStyle;
        private GUIStyle labelStyle;
        private GUIStyle labelTextStyle;
        private GUIStyle textFieldStyle;
        private GUIStyle buttonStyle;
        private GUIStyle horizontalScrollBarStyle;
        private GUIStyle verticalScrollBarStyle;
        private bool initializedStyles = false;

        private static Rect windowPos = new Rect(500, 400, 300, 75);

        public void Update()
        {
            return;
            if (!ApplicationLauncher.Ready || buttonAdded)
            {
                return;
            }

            beenThereDoneThatButton = InitBeenThereDoneThatButton();
            enabled = true;
        }

        public ApplicationLauncherButton InitBeenThereDoneThatButton()
        {
            ApplicationLauncherButton button = null;
            Texture2D buttonIcon = GameDatabase.Instance.GetTexture("BeenThereDoneThat/Resources/BeenThereDoneThatButton", false);

            if (buttonIcon != null)
            {
                button = ApplicationLauncher.Instance.AddModApplication(
                    OnTrue,
                    OnFalse,
                    OnHover,
                    OnHoverOut,
                    OnEnable,
                    OnDisable,
                    buttonScenes,
                    buttonIcon);

                buttonAdded = true;
            }
            return button;
        }

        void OnGUI()
        {
            return;
            initStyle();
            windowPos = GUILayout.Window(9741, windowPos, OnWindow, "My window", windowStyle);
        }

        public void OnWindow(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Hello KSP", labelStyle);
            GUILayout.Button("Klick me", buttonStyle);

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label("Ohh yeahh", labelStyle);
            GUILayout.Button("Klickkkkk meeeee!", buttonStyle);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void initStyle()
        {
            if (initializedStyles)
            {
                return;
            }
            windowStyle = new GUIStyle(HighLogic.Skin.window);
            windowStyle.stretchWidth = false;
            windowStyle.stretchHeight = false;

            labelStyle = new GUIStyle(HighLogic.Skin.label);
            labelStyle.stretchWidth = false;
            labelStyle.stretchHeight = false;

            labelTextStyle = new GUIStyle(HighLogic.Skin.label);
            labelTextStyle.stretchWidth = false;
            labelTextStyle.stretchHeight = true;
            labelTextStyle.wordWrap = true;

            textFieldStyle = new GUIStyle(HighLogic.Skin.textField);
            textFieldStyle.stretchWidth = false;
            textFieldStyle.stretchHeight = false;

            buttonStyle = new GUIStyle(HighLogic.Skin.button);
            buttonStyle.stretchHeight = false;
            buttonStyle.stretchWidth = false;

            horizontalScrollBarStyle = new GUIStyle(HighLogic.Skin.horizontalScrollbar);
            verticalScrollBarStyle = new GUIStyle(HighLogic.Skin.verticalScrollbar);

            initializedStyles = true;
        }

        public void OnTrue()
        {
            Debug.Log("[BeenthereDoneThatButtons]: onTrue");
        }

        public void OnFalse()
        {
            Debug.Log("[BeenthereDoneThatButtons]: onFalse");
        }

        public void OnHover()
        {
            Debug.Log("[BeenthereDoneThatButtons]: onHover");
        }

        public void OnHoverOut()
        {
            Debug.Log("[BeenthereDoneThatButtons]: onHoverOut");
        }

        public void OnEnable()
        {
            Debug.Log("[BeenthereDoneThatButtons]: onEnable");
        }

        public void OnDisable()
        {
            Debug.Log("[BeenthereDoneThatButtons]: onDisable");
        }

    }
}

