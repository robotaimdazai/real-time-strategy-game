using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class CustomEvent : UnityEvent<object>
{
    
}
public class EventManager : MonoBehaviour
{
       private  Dictionary<string, UnityEvent> _events;
       private  Dictionary<string, CustomEvent> _customEvents;
       private static EventManager _eventManager;

       public static EventManager Instance
       {
              get
              {
                     if (_eventManager==null)
                     {
                            _eventManager = FindObjectOfType<EventManager>();
                            if (!_eventManager)
                            {
                                   Debug.LogError("No active event manager found");
                            }
                            else
                            {
                                   _eventManager.Init();
                            }
                     }
                     return _eventManager;
              }
       }

       private void Init()
       {
              _events = new Dictionary<string, UnityEvent>();
              _customEvents = new Dictionary<string, CustomEvent>();
       }

       public static void AddListener(string id, UnityAction listener)
       {
              UnityEvent unityEvent = null;
              if (Instance._events.TryGetValue(id, out unityEvent))
              {
                     unityEvent.AddListener(listener);
              }
              else
              {
                     unityEvent = new UnityEvent();
                     unityEvent.AddListener(listener);
                     Instance._events.Add(id,unityEvent);
              }
       }
       public static void AddListener(string id, UnityAction<object> listener)
       {
              CustomEvent unityEvent = null;
              if (Instance._customEvents.TryGetValue(id, out unityEvent))
              {
                     unityEvent.AddListener(listener);
              }
              else
              {
                     unityEvent = new CustomEvent();
                     unityEvent.AddListener(listener);
                     Instance._customEvents.Add(id,unityEvent);
              }
       }

       public static void RemoveListener(string id, UnityAction listener)
       {
              if (Instance._events.TryGetValue(id, out var unityEvent))
              {
                     unityEvent.RemoveListener(listener);
              }
              else
              {
                     Debug.LogError("Listener not found");
              }
       }
       public static void RemoveListener(string id, UnityAction<object> listener)
       {
              if (Instance._customEvents.TryGetValue(id, out var unityEvent))
              {
                     unityEvent.RemoveListener(listener);
              }
              else
              {
                     Debug.LogError("Listener not found");
              }
       }

       public static void TriggerEvent(string id)
       {
              if (Instance._events.TryGetValue(id, out var unityEvent))
              {
                     unityEvent.Invoke();
              }
       }
       public static void TriggerEvent(string id, object data)
       {
              if (Instance._customEvents.TryGetValue(id, out var unityEvent))
              {
                     unityEvent.Invoke(data);
              }
       }
}
