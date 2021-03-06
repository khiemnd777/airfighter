﻿using UnityEngine;
using System;
using System.Collections;
using Saitama.FlyableObjects;

namespace Saitama.FlyableControls
{
    public abstract class FlyableControl : Updater, IFlyableControl
	{
        protected FlyableControl (MonoBehaviour mono) 
			: base(mono)
		{
			
		}

		public abstract void Thrust();
		public abstract void Yaw(Vector2 mousePosition);
		public abstract void Pitch(Vector2 mousePosition);
		public abstract void Roll();
		public abstract void Roll(float angle);

		public virtual void HandleInputEvents(){
			
		}
	}
}

