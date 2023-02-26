using System.Collections;
using System.Collections.Generic;


namespace BTree
{
    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    public class Node
    {
        private Node _parent;
        private Dictionary<string, object> _dataContext =
            new Dictionary<string, object>();
        
        protected NodeState _state;
        protected List<Node> _children = new List<Node>();
        
        
        public NodeState State { get => _state; }
        public Node() { _parent = null; }
        public Node Parent { get => _parent; }
        public List<Node> Children { get => _children; }
        public bool HasChildren { get => _children.Count > 0; }
        public virtual NodeState Evaluate() => NodeState.FAILURE;
        
        public Node(List<Node> children) : this()
        {
            SetChildren(children);
        }
        
        public void SetChildren(List<Node> children)
        {
            foreach (Node c in children)
                Attach(c);
        }
        public void Attach(Node child)
        {
            _children.Add(child);
            child._parent = this;
        }
        public void Detach(Node child)
        {
            _children.Remove(child);
            child._parent = null;
        }
        public object GetData(string key)
        {
            object value = null;
            if (_dataContext.TryGetValue(key, out value))
                return value;

            Node node = _parent;
            while (node != null)
            {
                value = node.GetData(key);
                
                if (value != null)
                    return value;
                node = node._parent;
            }
            return null;
        }
        public bool ClearData(string key)
        {
            if (_dataContext.ContainsKey(key))
            {
                _dataContext.Remove(key);
                return true;
            }

            Node node = _parent;
            while (node != null)
            {
                bool cleared = node.ClearData(key);
                if (cleared)
                    return true;
                node = node._parent;
            }
            return false;
        }
        public void SetData(string key, object value)
        {
            _dataContext[key] = value;
        }
    } 
}

