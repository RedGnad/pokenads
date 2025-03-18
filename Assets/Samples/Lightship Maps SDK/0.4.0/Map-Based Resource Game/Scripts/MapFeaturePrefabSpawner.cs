using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Builders.Standard.Objects;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Features;
using Niantic.Lightship.Maps.Samples.Common.Utilities;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    [PublicAPI]
    public class MapFeaturePrefabSpawner : AreaObjectBuilder
    {
        [SerializeField]
        private MapGameState.ResourceType _resourceType;

        public override void Build(IMapTile mapTile, GameObject parent)
        {
            if (MapGameState.Instance.IsResourceProductionEnabled(_resourceType))
            {
                base.Build(mapTile, parent);
            }
        }

        protected override Quaternion GetObjectRotation(IMapTileFeature feature)
        {
            return QuaternionEx.RandomLookRotation();
        }
    }
}
