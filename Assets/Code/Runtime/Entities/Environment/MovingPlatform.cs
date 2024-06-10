﻿using KBCore.Refs;
using SwapChains.Runtime.Entities.Player;
using UnityEngine;
using UnityEngine.Playables;

namespace SwapChains.Runtime.Entities.Environment
{
    [RequireComponent(typeof(PhysicsMover))]
    public class MovingPlatform : MonoBehaviour, IMoverController
    {
        [SerializeField, Self] PhysicsMover Mover;
        [SerializeField] PlayableDirector Director;
        Transform _transform;

        void OnValidate() => this.ValidateRefs();

        void Start()
        {
            _transform = transform;
            Mover.MoverController = this;
        }

        // This is called every FixedUpdate by our PhysicsMover in order to tell it what pose it should go to
        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            // Remember pose before animation
            _transform.GetPositionAndRotation(out var _positionBeforeAnim, out var _rotationBeforeAnim);

            // Update animation
            EvaluateAtTime(Time.time);

            // Set our platform's goal pose to the animation's
            goalPosition = _transform.position;
            goalRotation = _transform.rotation;

            // Reset the actual transform pose to where it was before evaluating. 
            // This is so that the real movement can be handled by the physics mover; not the animation
            _transform.SetPositionAndRotation(_positionBeforeAnim, _rotationBeforeAnim);
        }

        public void EvaluateAtTime(double time)
        {
            Director.time = time % Director.duration;
            Director.Evaluate();
        }
    }
}