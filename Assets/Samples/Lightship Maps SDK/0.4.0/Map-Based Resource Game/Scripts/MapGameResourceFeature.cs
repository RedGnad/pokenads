using System;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    internal class MapGameResourceFeature : MonoBehaviour
    {
        [SerializeField]
        private MapGameState.ResourceType _resourceType;

        [SerializeField]
        private int _maxUnits;
        public MapGameState.ResourceType ResourceType => _resourceType;

        private int _currentUnits;
        private float _resourceIncreaseTime;

        public bool ResourcesAvailable => _currentUnits > 0;

        public int GainResources()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                return 0;
            }

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance > 30.0f)
            {
                return 0;
            }

            if (_currentUnits <= 0)
            {
                return 0;
            }

            int currentUnits = _currentUnits;
            _currentUnits = 0;
            
            FloatingTextManager.Instance?.ShowText($"+{currentUnits} nads", transform.position);

            ScreenManager.Instance?.ShowSecondScreen();
            
            ParkFeatureSpawner.MarkFeatureCollected(transform.position);

            Destroy(gameObject);

            return currentUnits;
        }

        private void Update()
        {
            if (Time.time > _resourceIncreaseTime + 2.0f)
            {
                _resourceIncreaseTime = Time.time;
                _currentUnits = Mathf.Min(_currentUnits + 1, _maxUnits);
            }
        }
    }
}