using System;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    internal class MapGameBuilding : MonoBehaviour
    {
        [SerializeField]
        private MapGameState.ResourceType _resourceToConsume;

        [SerializeField]
        private MapGameState.ResourceType _resourceToCreate;

        [SerializeField]
        private float _resourceGenerationRate = 3.0f;

        [SerializeField]
        private FloatingText.FloatingText _floatingTextPrefab;

        private float _lastResourceGenerateTime;

        private void Update()
        {
            if (Time.time > _lastResourceGenerateTime + _resourceGenerationRate)
            {
                _lastResourceGenerateTime = Time.time;
                if (MapGameState.Instance.GetResource(_resourceToConsume) > 0)
                {
                    int amount = 1;
                    MapGameState.Instance.SpendResource(_resourceToConsume, amount);
                    MapGameState.Instance.AddResource(_resourceToCreate, amount);

                    var floatingTextPosition = transform.position + Vector3.up * 30.0f;
                    var floatText = Instantiate(_floatingTextPrefab, floatingTextPosition, Quaternion.identity);
                    floatText.SetText($"+{amount} {_resourceToCreate.ToString()}");
                }
            }
        }
    }
}
