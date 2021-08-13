using System;
using BehaviourTreeSystem.Runtime.Core;
using TheKiwiCoder;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Node = BehaviourTreeSystem.Runtime.Core.Node;

namespace BehaviourTreeSystem.Editor 
{

    public class NodeView : UnityEditor.Experimental.GraphView.Node {
        public Action<NodeView> OnNodeSelected;
        public Node node;
        public Port input;
        public Port output;

        public NodeView(Node node) : base(AssetDatabase.GetAssetPath(BehaviourTreeSettings.GetOrCreateSettings().nodeXml)) {
            this.node = node;

            UpdateName();

            this.viewDataKey = node.guid;
            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            SetupDataBinding();
        }

        private void UpdateName() => this.title = this.node.name = (string.IsNullOrEmpty(node.overrideName)) ? node.GetName() : node.overrideName;

        private void SetupDataBinding() {
            Label descriptionLabel = this.Q<Label>("description");
            descriptionLabel.bindingPath = "description";
            descriptionLabel.Bind(new SerializedObject(node));
        }

        private void SetupClasses()
        {
            switch (node)
            {
                case ActionNode _:
                    AddToClassList("action");
                    break;
                case CompositeNode _:
                    AddToClassList("composite");
                    break;
                case DecoratorNode _:
                    AddToClassList("decorator");
                    break;
                case RootNode _:
                    AddToClassList("root");
                    break;
            }
        }

        private void CreateInputPorts() {
            if (node is ActionNode) {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            } else if (node is CompositeNode) { 
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            } else if (node is DecoratorNode) {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            } else if (node is RootNode) {

            }

            if (input != null) {
                input.portName = "";
                input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(input);
            }
        }

        private void CreateOutputPorts() {
            if (node is ActionNode) {

            } else if (node is CompositeNode) {
                output = new NodePort(Direction.Output, Port.Capacity.Multi);
            } else if (node is DecoratorNode) {
                output = new NodePort(Direction.Output, Port.Capacity.Single);
            } else if (node is RootNode) {
                output = new NodePort(Direction.Output, Port.Capacity.Single);
            }

            if (output != null) {
                output.portName = "";
                output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(output);
            }
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            Undo.RecordObject(node, "Behaviour Tree (Set Position");
            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;
            EditorUtility.SetDirty(node);
        }

        public override void OnSelected() {
            base.OnSelected();
            if (OnNodeSelected != null) {
                OnNodeSelected.Invoke(this);
            }
        }

        public void SortChildren() {
            if (node is CompositeNode composite) {
                composite.children.Sort(SortByHorizontalPosition);
            }
        }

        private int SortByHorizontalPosition(Node left, Node right) {
            return left.position.x < right.position.x ? -1 : 1;
        }

        public void UpdateState() 
        {

            RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");
            RemoveFromClassList("disable");

            UpdateName();


            if (Application.isPlaying)
            {
                if (!node.reached)
                {
                    RemoveFromClassList("action");
                    RemoveFromClassList("composite");
                    RemoveFromClassList("decorator");
                    RemoveFromClassList("root");
                    
                    AddToClassList("disable");
                    return;
                }
                
                SetupClasses();
                switch (node.state)
                {
                    case Node.State.Running:
                        AddToClassList("running");
                        break;
                    case Node.State.Failure:
                        AddToClassList("failure");
                        break;
                    case Node.State.Success:
                        AddToClassList("success");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            


        }
    }
}