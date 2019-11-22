using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;

namespace CloneDroneVR
{
    public static class TransportUtils
    {
        public static void TransportTo(Transform item, Vector3 startPosition, Quaternion startRotation, Vector3 endPosition, Quaternion endRotation, float time, Action onComplete = null)
        {
            StaticCoroutineRunner.StartStaticCoroutine(transportTo(item, startPosition, startRotation, endPosition, endRotation, time, onComplete));
        }
        static IEnumerator transportTo(Transform item, Vector3 startPosition, Quaternion startRotation, Vector3 endPosition, Quaternion endRotation, float time, Action onComplete)
        {
            float endTime = Time.time + time;
            while(Time.time < endTime)
            {
                float t = 1 - (endTime - Time.time)/time;
                item.transform.position = Vector3.Lerp(startPosition, endPosition, t);
                item.transform.rotation = Quaternion.Lerp(startRotation, endRotation, t);

                yield return null;
            }
            item.transform.position = endPosition;
            item.transform.rotation = endRotation;

            if(onComplete != null)
                onComplete();
        }
    }
}
