using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    public class ARCompatibilityChecker : MonoBehaviour
    {
        public static bool IsARCoreSupported { get; private set; } = false;

        IEnumerator Start()
        {
            yield return ARSession.CheckAvailability();

            if (ARSession.state == ARSessionState.Unsupported)
            {
                IsARCoreSupported = false;
                Debug.Log("ARCompatibilityChecker -> ARSession state: Unsupported");
            }
            else
            {
                IsARCoreSupported = true;
                Debug.Log("ARCompatibilityChecker -> ARSession state: " + ARSession.state);
            }

            Debug.Log("ARCompatibilityChecker -> IsARCoreSupported : " + IsARCoreSupported);
        }
    }
}