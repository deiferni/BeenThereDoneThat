using System.Collections.Generic;
using UnityEngine;

namespace BeenThereDoneThat
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[]
    {
        GameScenes.FLIGHT,
        GameScenes.TRACKSTATION,
        GameScenes.SPACECENTER,
        GameScenes.EDITOR
    })]
    public class QuickLauncher : ScenarioModule
    {
        public static QuickLauncher Instance
        {
            get;
            private set;
        }

        public override void OnAwake()
        {
            Debug.Log("[BeenThereDoneThat]: AWAKING ORBITCONTROLLER");
            Instance = this;
        }

        public override void OnSave(ConfigNode node)
        {
            Debug.Log("[BeenThereDoneThat]: SAVING ORBITCONTROLLER");
        }

        public override void OnLoad(ConfigNode node)
        {
            Debug.Log("[BeenThereDoneThat]: LOADING ORBITCONTROLLER");
        }

        public bool Split(ProtoVessel protoVessel, out ProtoLaunchVehicle launchVehicle, out ProtoPayload payload)
        {
            ProtoPartSnapshot payloadSeparator = FindPayloadSeparatorPart(protoVessel.protoPartSnapshots);
            if (payloadSeparator == null)
            {
                Debug.Log("[BeenThereDoneThat]: No payloadseparator found");
                launchVehicle = null;
                payload = null;
                return false;
            }

            int payloadPartSeparationIndex = payloadSeparator.separationIndex;
            List<ProtoPartSnapshot> payloadParts = new List<ProtoPartSnapshot>();
            List<ProtoPartSnapshot> launchVehicleParts = new List<ProtoPartSnapshot>();
            foreach (ProtoPartSnapshot part in protoVessel.protoPartSnapshots)
            {
                if (part.separationIndex >= payloadPartSeparationIndex)
                {
                    launchVehicleParts.Add(part);
                }
                else
                {
                    payloadParts.Add(part);
                }
            }

            launchVehicle = new ProtoLaunchVehicle(payloadSeparator, launchVehicleParts);
            payload = new ProtoPayload(payloadParts);
            return true;
        }

        public bool Split(List<Part> parts, out LaunchVehicle launchVehicle, out Payload payload)
        { 
            Part payloadSeparator = FindPayloadSeparatorPart(parts);
            if (payloadSeparator == null)
            {
                Debug.Log("[BeenThereDoneThat]: No payloadseparator found");
                launchVehicle = null;
                payload = null;
                return false;
            }

            int payloadPartSeparationIndex = payloadSeparator.separationIndex;
            List<Part> payloadParts = new List<Part>();
            List<Part> launchVehicleParts = new List<Part>();
            foreach (Part part in parts)
            {
                if (part.separationIndex >= payloadPartSeparationIndex)
                {
                    launchVehicleParts.Add(part);
                }
                else
                {
                    payloadParts.Add(part);
                }
            }

            launchVehicle = new LaunchVehicle(payloadSeparator, launchVehicleParts);
            payload = new Payload(payloadParts);
            return true;
        }

        public ProtoPartSnapshot FindPayloadSeparatorPart(List<ProtoPartSnapshot> parts)
        {
            foreach (ProtoPartSnapshot part in parts)
            {
                ProtoPartModuleSnapshot module = part.FindModule("PayloadSeparatorPart");
                if (module == null)
                {
                    continue;
                }
                if (bool.Parse(module.moduleValues.GetValue("isPayloadSeparator")))
                {
                    return part;
                }
            }
            return null;
        }

        public Part FindPayloadSeparatorPart(List<Part> parts)
        {
            foreach (Part part in parts)
            {
                foreach (PayloadSeparatorPart module in part.FindModulesImplementing<PayloadSeparatorPart>())
                {
                    if (module.isPayloadSeparator)
                    {
                        return part;
                    }
                }
            }
            return null;
        }
    }
}
