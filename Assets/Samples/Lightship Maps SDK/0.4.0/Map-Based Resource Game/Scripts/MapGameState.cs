// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using Niantic.Lightship.Maps.Core.Coordinates;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    internal class MapGameState : MonoBehaviour
    {
        public static MapGameState Instance { get; private set; }

        private readonly Dictionary<ResourceType, int> _resources = new();
        private readonly Dictionary<ResourceType, bool> _resourceProduction = new();

        public Action<StructureType> OnStructureBuilt;
        public Action<ResourceType, int> OnResourceUpdated;

        public enum ResourceType
        {
            Nads,
            Planks,
            Stone,
            Bricks
        }

        public enum StructureType
        {
            Sawmill,
            StoneMason,
            Stronghold
        }

        [Serializable]
        public class StructureCost
        {
            [SerializeField] private ResourceType _resource;
            [SerializeField] private int _amount;

            public ResourceType Resource => _resource;
            public int Amount => _amount;
        }

        [SerializeField]
        private StructureCost[] _structureCosts = Array.Empty<StructureCost>();

        private void Awake()
        {
            Instance = this;
            LoadResources();
        }

        private void LoadResources()
        {
            _resources[ResourceType.Nads] = 0;
            _resources[ResourceType.Planks] = 0;
            _resources[ResourceType.Stone] = 0;
            _resources[ResourceType.Bricks] = 0;

            _resourceProduction[ResourceType.Nads] = true;
        }

        public bool IsResourceProductionEnabled(ResourceType resourceType)
        {
            return _resourceProduction.ContainsKey(resourceType) && _resourceProduction[resourceType];
        }

        public void EnableResourceProduction(ResourceType resourceType, bool isEnabled)
        {
            _resourceProduction[resourceType] = isEnabled;
        }

        public int GetResource(ResourceType resourceType)
        {
            return _resources[resourceType];
        }

        public void AddResource(ResourceType resourceType, int amount)
        {
            _resources[resourceType] += amount;
            OnResourceUpdated?.Invoke(resourceType, _resources[resourceType]);
        }

        public void SpendResource(ResourceType resourceType, int amount)
        {
            _resources[resourceType] -= amount;
            OnResourceUpdated?.Invoke(resourceType, _resources[resourceType]);
        }

        public void StructureBuilt(LatLng coordinates, StructureType structureType)
        {
            var structureCost = _structureCosts[(int)structureType];
            SpendResource(structureCost.Resource, structureCost.Amount);

            OnStructureBuilt?.Invoke(structureType);
        }

        public StructureCost GetStructureCost(StructureType structureType)
        {
            return _structureCosts[(int)structureType];
        }
    }
}
