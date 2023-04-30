using System;
using System.Collections;
using System.Collections.Generic;
using BTree;
using PlasticGui;
using UnityEditor;
using UnityEngine;
using Tree = BTree.Tree;

[ExecuteInEditMode]
public class BehaviourTreeWindow : EditorWindow 
{
    private int _gridSize = 20; // size of each grid cell
    private Color _gridColor = Color.black;
    private Rect _gridRect;
    private Rect _draggableRect;
    private float _zoomSpeed = 0.1f;
    private float _zoomLevel = 1.0f;
    private Texture2D _texture;
    private bool _init = false;
    private Tree _tree; 

    private void OnEnable()
    {
        _texture = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);

        for (int x = 0; x < _texture.width; x++)
        {
            for (int y = 0; y < _texture.height; y++)
            {
                // check if this pixel is on a grid line
                if (x % _gridSize == 0 || y % _gridSize == 0)
                {
                    _texture.SetPixel(x, y, _gridColor);
                }
                else
                {
                    _texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        _texture.Apply();
        _init = false;
    }

    [MenuItem ("Window/Behaviour tree")]

    public static void  ShowWindow () {
        EditorWindow.GetWindow(typeof(BehaviourTreeWindow));
    }
    
    void OnGUI () {

        if (!_init)
        {
            Init();
            _init = true;
        }

        // get selected gameobject
        var active = Selection.activeGameObject;
        bool canDrawTree = false;
        if (active!=null)
        {
            _tree = Selection.activeGameObject.GetComponent<Tree>();
            canDrawTree = !ReferenceEquals(_tree, null);
        }
        //create grid rect
        _gridRect = new Rect(0, 0, _texture.width * _zoomLevel, _texture.height * _zoomLevel);
        _draggableRect = new Rect(_draggableRect.position.x, _draggableRect.position.y, _texture.width * _zoomLevel, _texture.height * _zoomLevel);
        EditorGUIUtility.AddCursorRect(_gridRect, MouseCursor.Pan);
        if (Event.current.type == EventType.MouseDrag && _draggableRect.Contains(Event.current.mousePosition))
        {
            _draggableRect.position += Event.current.delta;
        }
        GUI.DrawTexture(_gridRect, _texture);
        if (canDrawTree && Application.isPlaying)
        {
            var size = new Vector2(200 *  _zoomLevel, 50*_zoomLevel);;
            var rootPos = _draggableRect.center;
            var rootRect = new Rect(rootPos.x, rootPos.y, size.x, size.y);
            GUI.Box(rootRect,_tree.Root.GetType().Name,GUI.skin.button);
            var charBT = _tree as CharacterBT;
            var levelPos = rootRect.position + new Vector2(0, rootRect.height);
            DrawChildren(charBT.Root, levelPos, size);
        }
        
        // zoom code
        if (Event.current.type == EventType.ScrollWheel)
        {
            float zoomDelta = -Event.current.delta.y * _zoomSpeed;
            _zoomLevel = Mathf.Clamp(_zoomLevel + zoomDelta, 1, 2.0f);
        }
        Repaint();
    }

    private void DrawChildren(Node node,Vector2 levelPos, Vector2 size)
    {
        if (node.Children.Count <= 0) return;
        for (int i = 0; i < node.Children.Count; i++)
        {
            var thisNode = node.Children[i];
            Vector2 thisPos = levelPos;
            if (i % 2 != 0)
            {
                thisPos = new Vector2(thisPos.x +size.x *i, thisPos.y );
            }
            else
            {
                thisPos = new Vector2(thisPos.x - size.x *i, thisPos.y );
            }
            
            var childRect = new Rect(thisPos.x, thisPos.y, size.x, size.y);
            var childType = thisNode.GetType();
            GUI.Box(childRect,childType.Name,GUI.skin.button);
            var newLevel = childRect.position + new Vector2(size.x, size.y);
            DrawChildren(thisNode,newLevel,size);
        }
    }

    private void Init()
    {
        _gridRect = new Rect(0, 0, _texture.width * _zoomLevel, _texture.height * _zoomLevel);
        _draggableRect = new Rect(0, 0, _texture.width * _zoomLevel, _texture.height * _zoomLevel);
    }
    
}
