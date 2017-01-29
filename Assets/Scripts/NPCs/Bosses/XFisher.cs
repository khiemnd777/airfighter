﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Saitama;
using Saitama.Ships;

namespace Saitama.NPCs.Bosses
{
    public class XFisher : Boss, IMovable, IChasing, IRunningForLife
    {
        private const float RUNNING_FOR_LIFE_HEALTH_PERCENT = 20;
        private const float DEFAULT_HEALTH = 99999;
        private float _runningForLifeHealth;

        private Rigidbody _rigidbody;

        public XFisher(MonoBehaviour mono, Component monoComponent) : base (mono, monoComponent)
        {
            
        }

        public override void Init()
        {
            _rigidbody = _mono.GetComponent<Rigidbody>();
            // compute level at first time
            ComputeLevel();
            // compute health at first time
            ComputeHealth();
        }

        public void Chase(){
            if (_health > _runningForLifeHealth)
            {
                // compute velocity
                ComputeVelocity();

                // detect target position
                var targetPosition = DetectTargetsPosition(Constants.PLAYER_TAG, 1600f);
                if (targetPosition == Vector3.zero)
                {
                    // if target position was zero, the NPC moves with zero rotation
                    var yaw = Utility.CalculateYaw (Vector2.zero, RotationSpeed, Time.fixedDeltaTime);
                    _rigidbody.rotation *= yaw;

                    var pitch = Utility.CalculatePitch (Vector2.zero, RotationSpeed, Time.fixedDeltaTime);
                    _rigidbody.rotation *= pitch;    
                }
                else
                {
                    // if target position was non-zero, the NPC moves to target position
                    Chase(targetPosition);
                }
            }
        }

        public void Chase(GameObject target){
            Chase(target.transform.position);
        }

        public void Chase(Vector3 targetPosition){
            var lookTo = Quaternion.LookRotation(targetPosition - _mono.transform.position);
            var rotation = Quaternion.RotateTowards(_mono.transform.rotation, lookTo, Constants.DEFAULT_MAX_DEGREES_DELTA);
            _rigidbody.MoveRotation(rotation);
        }

        public void RunForLife(){
            if (_health <= _runningForLifeHealth)
            {
                // compute velocity
                ComputeVelocity();

                // the NPC should be moved straigth when it was near death
                var yaw = Utility.CalculateYaw(Vector2.zero, RotationSpeed, Time.fixedDeltaTime);
                _rigidbody.rotation *= yaw;

                var pitch = Utility.CalculatePitch(Vector2.zero, RotationSpeed, Time.fixedDeltaTime);
                _rigidbody.rotation *= pitch;
            }
        }

        public void Move()
        {
            // chasing
            Chase();
            // running for life
            RunForLife();
        }

        public override void ComputeLevel()
        {
            // level be computed via average level total of players
            var targets = GameObject.FindGameObjectsWithTag(Constants.PLAYER_TAG);
            var levelManagers = Utility.GetShipComponents<LevelManager>(targets);
            var levels = levelManagers.Select(levelManager => levelManager.Level);
            var level = levels.Sum();
            level /= targets.Length;
            _level = level;
        }

        public override void ComputeHealth()
        {
            var targets = GameObject.FindGameObjectsWithTag(Constants.PLAYER_TAG);
            var scoreManagers = Utility.GetShipComponents<ScoreManager>(targets);
            var scores = scoreManagers.Select(scoreManager => scoreManager.Score);
            var score = scores.Sum();
            _health = score <= DEFAULT_HEALTH ? DEFAULT_HEALTH : score;
            _runningForLifeHealth = _health * RUNNING_FOR_LIFE_HEALTH_PERCENT / 100;
        }

        private void ComputeVelocity(){
            var velocity = Utility.CalculateVelocity(_rigidbody.rotation, AmbientSpeed, Time.fixedDeltaTime);
            _rigidbody.velocity = velocity;
        }

        private Vector3 DetectTargetsPosition(string tag, float limit){
            var targets = Utility.FindTargetsViaRadar(tag, _mono.transform.position, limit);
            var positions = targets.Select(target => target.transform.position).ToArray();
            var averagePosition = Utility.ComputeAveragePosition(positions);
            return averagePosition;
        }
    }
}

