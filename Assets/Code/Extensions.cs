using System.Linq;
using UnityEngine;

namespace Code
{
    public static class Extensions
    {
        public static bool HasComponent<T>(this GameObject obj) where T : Component
        {
            return obj.GetComponentsInChildren<T>().FirstOrDefault() != null;
        }
    }
}