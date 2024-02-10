using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Aikom.FunctionalAnimation
{
    public class UpdateManager : MonoBehaviour
    {
        private static UpdateManager _instance;
        private static Dictionary<int, IManagedObject> _managedObjects = new Dictionary<int, IManagedObject>();
        private static int _id = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateInstance()
        {   
            if(_instance != null)
                return;
            var obj = new GameObject("UpdateManager");
            DontDestroyOnLoad(obj);
            _instance = obj.AddComponent<UpdateManager>();
            Application.quitting += _instance.CleanUp;
        }

        private void Update()
        {
            foreach (var obj in _managedObjects)
            {
                obj.Value.OnUpdate();
            }
        }

        public static int RegisterObject(IManagedObject obj)
        {   
            _id++;
            _managedObjects.Add(_id, obj);
            return _id;
        }

        public static void UnregisterObject(int id)
        {
            if (!_managedObjects.ContainsKey(id))
                return;
            _managedObjects.Remove(id);
        }

        private void CleanUp()
        {
            _managedObjects.Clear();
            Destroy(gameObject);
        }
    }
}

