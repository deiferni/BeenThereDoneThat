using System;
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

            ProtoPartSnapshotTree tree = new ProtoPartSnapshotTree(protoVessel.protoPartSnapshots);

            List<ProtoPartSnapshot> payloadParts;
            List<ProtoPartSnapshot> launchVehicleParts;
            tree.Split(payloadSeparator, out launchVehicleParts, out payloadParts);
            payload = new ProtoPayload(payloadParts);
            launchVehicle = new ProtoLaunchVehicle(payloadSeparator, launchVehicleParts);
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

            Part root = parts[0].localRoot;
            List<Part> payloadParts = new List<Part>();
            List<Part> launchVehicleParts = new List<Part>();

            PartTreeHelper.AddBreadthFirst(root, payloadParts, payloadSeparator);
            PartTreeHelper.AddBreadthFirst(payloadSeparator, launchVehicleParts);

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

    internal class PartTreeHelper
    {
        class PartNameComparer : IComparer<Part>
        {
            public int Compare(Part x, Part y)
            {
                return x.name.CompareTo(y.name);
            }
        }

        internal static void AddBreadthFirst(Part start, List<Part> parts)
        {
            AddBreadthFirst(start, parts, null);
        }

        internal static void AddBreadthFirst(Part start, List<Part> parts, Part stopAt)
        {
            parts.Add(start);
            List<Part> children = new List<Part>(start.children);
            children.Sort(new PartNameComparer());

            foreach (Part child in children)
            {
                if (child != stopAt)
                {
                    AddBreadthFirst(child, parts, stopAt);
                }
            }
        }
    }

    internal class ProtoPartSnapshotTree
    {
        Dictionary<uint, ProtoPartSnapshotNode> tree;
        ProtoPartSnapshotNode root;
        List<ProtoPartSnapshot> protoPartSnapshots;

        internal ProtoPartSnapshotTree(List<ProtoPartSnapshot> snapshots)
        {
            tree = new Dictionary<uint, ProtoPartSnapshotNode>();
            protoPartSnapshots = snapshots;
            root = null;

            foreach (ProtoPartSnapshot part in protoPartSnapshots)
            {
                CreateNode(part);
            }

        }

        internal ProtoPartSnapshotNode CreateNode(ProtoPartSnapshot part)
        {
            ProtoPartSnapshotNode node = null;
            ProtoPartSnapshotNode parentNode = null;

            ProtoPartSnapshot parent = protoPartSnapshots[part.parentIdx];
            if (parent != part)
            {
                parentNode = CreateNode(parent);
            }
            if (!tree.TryGetValue(part.persistentId, out node))
            {
                node = new ProtoPartSnapshotNode(part, parentNode);
                tree.Add(part.persistentId, node);
            }
            if (parentNode == null)
            {
                root = node;
            }
            return node;
        }

        internal void Split(ProtoPartSnapshot separatorPart, out List<ProtoPartSnapshot> launchVehicleParts, out List<ProtoPartSnapshot> payloadParts)
        {
            ProtoPartSnapshotNode splitAt = null;
            if (!tree.TryGetValue(separatorPart.persistentId, out splitAt))
            {
                launchVehicleParts = null;
                payloadParts = null;
                Debug.Log(string.Format("Could not split tree, part not found {0}", separatorPart.partName));
                return;
            }

            launchVehicleParts = new List<ProtoPartSnapshot>();
            payloadParts = new List<ProtoPartSnapshot>();

            root.AddBreadthFirst(payloadParts, splitAt);
            splitAt.AddBreadthFirst(launchVehicleParts);
        }
    }

    internal class ProtoPartSnapshotNode : IComparable
    {
        ProtoPartSnapshot part;
        ProtoPartSnapshotNode parent;
        List<ProtoPartSnapshotNode> children;

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            ProtoPartSnapshotNode otherNode = obj as ProtoPartSnapshotNode;
            if (otherNode == null)
            {
                throw new ArgumentException("Object is not a ProtoPartSnapshotNode");
            }
            return part.partName.CompareTo(otherNode.part.partName);
        }

        internal ProtoPartSnapshotNode(ProtoPartSnapshot protoPart, ProtoPartSnapshotNode parentNode)
        {
            part = protoPart;
            parent = parentNode;
            children = new List<ProtoPartSnapshotNode>();
            if (parent != null)
            {
                parent.AddChild(this);
            }
        }

        internal void AddChild(ProtoPartSnapshotNode child)
        {
            children.Add(child);
        }

        internal void AddBreadthFirst(List<ProtoPartSnapshot> parts)
        {
            AddBreadthFirst(parts, null);
        }

        internal void AddBreadthFirst(List<ProtoPartSnapshot> parts, ProtoPartSnapshotNode stopAt)
        {
            parts.Add(part);
            children.Sort();

            foreach (ProtoPartSnapshotNode child in children)
            {
                if (child != stopAt)
                {
                    child.AddBreadthFirst(parts, stopAt);
                }
            }
        }
    }
}
