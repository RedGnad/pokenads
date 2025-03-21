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
            // Attendre que ARSession vérifie la disponibilité
            yield return ARSession.CheckAvailability();

            // Selon l'état de ARSession, déterminer la compatibilité
            if (ARSession.state == ARSessionState.Unsupported)
            {
                IsARCoreSupported = false;
                Debug.Log("ARCompatibilityChecker -> ARSession state: Unsupported");
            }
            else
            {
                // On considère que les autres états indiquent un support acceptable
                IsARCoreSupported = true;
                Debug.Log("ARCompatibilityChecker -> ARSession state: " + ARSession.state);
            }

            Debug.Log("ARCompatibilityChecker -> IsARCoreSupported : " + IsARCoreSupported);
        }
    }
}