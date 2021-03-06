﻿using UnityEngine;
using System;
using Saitama;

namespace Saitama{
    public class LevelManager : Updater
    {
		
		public Action<int> OnIncreased;
		private int _level = Constants.MIN_LEVEL;

        public LevelManager(MonoBehaviour mono) : base(mono){

		}

		public int Level { get { return _level; } }

        void TwoPunch(){
            Upgrade();
        }

        public void InitLevel(Action<int> init){
            init.Invoke(_level);
        }
		public void Upgrade(){
			if (IsMax())
				return;
			var scoreManager = _parent.GetComponent<ScoreManager> ();
			var score = scoreManager.Score;
			var nextLevel = _level + 1;
			var upgradedScore = GetUpgradedScoreByLevel (nextLevel);
			if (score >= upgradedScore) {
				_level = nextLevel;
				if (OnIncreased != null) {
					OnIncreased.Invoke (_level);
				}
			}
		}

		public bool IsMax(){
			return _level == Constants.MAX_LEVEL;
		}

        public virtual void ComputeLevel(int level){
            _level = level;
        }

		public int GetUpgradedScoreByLevel(int currentLevel){
			var next = Constants.DEFAULT_SCORE;
			var _fibo = 0;
			for (var i = 0; i < (currentLevel + 1); i++) {
				var temp = _fibo;
				_fibo = next;
				next = temp + next;
			}
			return _fibo;
		}
	}
}