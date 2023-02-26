using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BTree
{
    public abstract class Tree : MonoBehaviour
    {
        private Node _root = null;
        protected abstract Node SetupTree();
        
        public Node Root => _root;
        
        protected void Start()
        {
            _root = SetupTree();
        }

        private void Update()
        {
            if (_root != null)
                _root.Evaluate();
        }

        
        
    }
}

