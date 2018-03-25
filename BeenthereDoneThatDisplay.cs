using KSP.UI.Screens;
using UnityEngine;

namespace BeenThereDoneThat
{
    [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
    class BeenthereDoneThatButtons : MonoBehaviour
    {
        static public bool buttonAdded = false;
        static protected ApplicationLauncherButton beenThereDoneThatButton = null;

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
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT,
                    buttonIcon);

                buttonAdded = true;
            }
            return button;
        }
    }
}

