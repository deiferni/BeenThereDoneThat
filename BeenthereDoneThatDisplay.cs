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

        public void Update()
        {
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

