﻿using UnityEngine;
using System;

namespace Saitama {
	public class ScoreManager : CommonObject {

		public Action OnDecreasing;
		public Action OnIncreasing;

		private int _score;

		public ScoreManager(MonoBehaviour mono) : base(mono){
			_score = Constants.DEFAULT_SCORE;
		}

		public int Score{
			get { return _score; }
		}

		public void Increase(int amout){
			_score += amout;
			if (OnIncreasing != null) {
				OnIncreasing.Invoke ();
			}
		}

		public void Decrease(int amout){
			_score -= amout;
			if (OnDecreasing != null) {
				OnDecreasing.Invoke ();
			}
		}

		public ScoreManager GetScoreManager(GameObject target){
			var shipController = target.GetComponent<ShipController> ();
			if (shipController != null) {
				var scoreManager = shipController.ship.GetComponent<ScoreManager> ();
				return scoreManager;
			}
			return null;
		}

		public void Hit(GameObject target, int damage){
			var targetScoreManager = GetScoreManager (target);
			if (targetScoreManager == null)
				return;
			targetScoreManager.Decrease (damage);
			var targetScore = targetScoreManager.Score;
			var score = damage < targetScore ? damage : targetScore;
			Increase (score);
		}
	}
}