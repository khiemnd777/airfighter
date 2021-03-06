﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Saitama;
using Saitama.Ships;
using Saitama.Weapons;
using Saitama.Extensions;

public class Utility
{
	public static bool IsInRange(float value, float min, float max){
		return value >= min && value <= max;
	}

	public static bool IsFrontOfCamera(Vector3 position, Camera camera){
		var positionScrPt = camera.WorldToScreenPoint(position);
		return positionScrPt.z > 0;
	}

	public static Vector3 ScreenToWorldPoint(Camera camera, float zIndex = .1f){
        var mousePosition = camera.ScreenToWorldPoint (new Vector3(Input.mousePosition.x, Input.mousePosition.y, zIndex));
		var realX = mousePosition.x * (Screen.width < Screen.height ? ((float)Screen.height / (float)Screen.width) : 1.0f);
		var realY = mousePosition.y * (Screen.width < Screen.height ? 1.0f : ((float)Screen.width / (float)Screen.height));
		return new Vector3 (realX, realY, mousePosition.z);
	}

    public static T GetMonoComponent<T>(string name){
        return GameObject.Find(name).GetComponent<T>();
    }

    public static T GetResource<T>(string path) where T : UnityEngine.Object{
        return (T) Resources.Load(path, typeof(T));
    }

    public static GameObject[] FindTargetsViaRadar(string tag, Vector3 ownPosition, float limit){
        var targets = GameObject.FindGameObjectsWithTag(tag);
        targets = targets.Where(target =>{
            var sqrLen = (target.transform.position - ownPosition).sqrMagnitude;
            return sqrLen <= limit * limit;
        }).ToArray();
        return targets;
    }

    public static Vector3 ComputeAveragePosition(Vector3[] positions){
        var average = Vector3.zero;
        for (var i = 0; i < positions.Length; i++)
        {
            average += positions[i];
        }
        average /= positions.Length;
        return average;
    }

    public static void SplitPoints(AttackerIdentifier attackerIdentifier, float originalScore) {
        if (attackerIdentifier == null)
            return;
        var attackers = attackerIdentifier.GetAttackers();
        var sum = attackerIdentifier.Sum();
        for (var i = 0; i < attackers.Count; i++)
        {
            var attacker = attackers.ElementAt(i);
            var scoreManager = attacker.Key.GetShipComponent<ScoreManager>();
            var score = scoreManager.ComputeScoreViaPercent(sum, attacker.Value, originalScore);
            scoreManager.Increase(Mathf.CeilToInt(score));
        }
    }

    public static int Side(Transform own, Transform target){
        var targetDirection = target.position - own.position;
        var ownForward = own.forward;
        var ownUp = own.up;
        var perp = Vector3.Cross(ownForward, targetDirection); // right vector
        var side = Vector3.Dot(perp, ownUp);
        if (side > 0f)
        {
            return 1; // side is right
        }
        else if (side < 0f)
        {
            return -1; // side is left
        }
        return 0; // side is front/back
    }

	public static T GetShipComponent<T>(GameObject gameObject) where T : ICommonObject{
		var shipController = gameObject.GetComponent<ShipController> ();
		return shipController.ship.GetComponent<T> ();
	}

    public static T[] GetShipComponents<T>(GameObject[] gameObjects) where T:ICommonObject{
        var ts = gameObjects
            .Where(target => HasShipComponent(target))
            .Select(target => target.GetComponent<ShipController>())
            .Select(target => target.ship)
            .Select(target => target.GetComponent<T>());
        return ts.ToArray();
    }

	public static bool HasShipComponent(GameObject gameObject){
		return gameObject.GetComponent<ShipController> () != null;
	}

    public static Ship[] ExtractShipComponent(GameObject[] gameObjects){
        var ships = gameObjects
            .Where(target => HasShipComponent(target))
            .Select(target => target.GetComponent<ShipController>())
            .Select(target => target.ship);
        return ships.ToArray();
    }

	public static Quaternion RotatePrincipalAxes(Vector2 mousePosition, float rotationSpeed){
		var yaw = mousePosition.x * rotationSpeed;
		var pitch = mousePosition.y * rotationSpeed;
		var rot = Quaternion.identity;
		rot.eulerAngles = new Vector3 (-pitch, yaw, .0f);
		return rot;
	}

	public static Quaternion CalculateYaw(Vector2 mousePosition, float rotationSpeed){
		var yaw = mousePosition.x * rotationSpeed;
		var rot = Quaternion.identity;
		rot.eulerAngles = new Vector3 (.0f, yaw, .0f);
		return rot;
	}

	public static Quaternion CalculatePitch(Vector2 mousePosition, float rotationSpeed){
		var pitch = mousePosition.y * rotationSpeed;
		var rot = Quaternion.identity;
		rot.eulerAngles = new Vector3 (-pitch, .0f, .0f);
		return rot;
	}

	public static Vector3 CalculateVelocity(Quaternion rotation, float ambientSpeed){
		var forward = Vector3.forward;
		forward = rotation * forward;
		var velocity = forward * ambientSpeed;
		return velocity;
	}

	public static Quaternion CalculateRoll(float angle){
		var angleAroundZ = Quaternion.identity;
		angleAroundZ.eulerAngles = new Vector3 (.0f, .0f, angle);
		return angleAroundZ;
	}

	public static Quaternion BalanceRolling(Quaternion currentAngle, Quaternion zeroAngle, float timer = .0f){
		return Quaternion.Lerp (currentAngle, zeroAngle, timer);
	}

	public static IEnumerable<Quaternion> BalanceRollingLinear(Quaternion currentAngle, Quaternion zeroAngle, float timer){
		var percent = .0f;
		while (percent <= 1f) {
			percent += Time.fixedDeltaTime * timer;
			yield return BalanceRolling (currentAngle, zeroAngle, percent);
		}
		yield return zeroAngle;
	}

	public static bool CheckCollision(Component component, float maxDistance, int collisionMask){
		var transform = component.transform;
		var ray = new Ray (transform.position, transform.forward);
		return Physics.Raycast (ray, maxDistance, collisionMask, QueryTriggerInteraction.Collide);
	}

	public static void CheckCollision(Component component, float maxDistance, int collisionMask, Action<RaycastHit> action){
		var transform = component.transform;
		var ray = new Ray (transform.position, transform.forward);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, maxDistance, collisionMask, QueryTriggerInteraction.Collide)) {
			action.Invoke (hit);
		}
	}

    public static Quaternion MoveRotation(Transform own, Transform target, float maxDegreesDelta){
        var dist = target.position - own.position;
        var targetRotation = Quaternion.LookRotation (dist, own.up);
        return Quaternion.RotateTowards(own.rotation, targetRotation, maxDegreesDelta);
    }

    public static void MoveRotation(Rigidbody rigid, Transform own, Transform target, float maxDegreesDelta){
        var dist = target.position - own.position;
        var targetRotation = Quaternion.LookRotation (dist, own.up);
        var rotationMoving = Quaternion.RotateTowards(own.rotation, targetRotation, maxDegreesDelta);
        rigid.MoveRotation(rotationMoving);
    }

    public static void MoveRotation(Rigidbody rigid, Transform own, Vector3 dist, float maxDegreesDelta){
        var targetRotation = Quaternion.LookRotation (dist, own.up);
        var rotationMoving = Quaternion.RotateTowards(own.rotation, targetRotation, maxDegreesDelta);
        rigid.MoveRotation(rotationMoving);
    }

    public static void RotateAround(Rigidbody rigid, Transform own, Transform target, Vector3 axis, float targetMagnitude, float velocity, float t){
        own.RotateAround(target.position, axis, velocity * Time.deltaTime);
        var dist = rigid.position - target.position;
        rigid.position = Vector3.Lerp(rigid.position, target.position + dist.normalized * targetMagnitude, t / (dist.magnitude));
    }
}

