﻿using UnityEngine;
using System;

namespace Saitama.Weapons
{
    public class CollisionChecker
	{
		public Action<Component, Collider[]> OnHit;

		private LayerMask _targets;
        private float _radius;

        public CollisionChecker (LayerMask targets)
		{
			_targets = targets;
		}

        public CollisionChecker (LayerMask targets, float radius)
        {
            _targets = targets;
            _radius = radius;
        }

        public float Radius { get { return _radius; } set { _radius = value; } }

        public virtual void Check (Component component, float radius = 1.0f, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide)
		{
            var r = radius == 1.0f ? _radius : radius;
			var transform = component.transform;
            var colliders = Physics.OverlapSphere (transform.position, r, _targets, queryTriggerInteraction);
            if (OnHit != null)
            {
                OnHit.Invoke(component, colliders);
            }
		}
	}
}