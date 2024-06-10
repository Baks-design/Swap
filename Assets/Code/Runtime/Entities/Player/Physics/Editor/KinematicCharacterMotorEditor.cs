#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace SwapChains.Runtime.Entities.Player
{
    [CustomEditor(typeof(KinematicCharacterMotor))]
    public class KinematicCharacterMotorEditor : Editor
    {
        protected virtual void OnSceneGUI()
        {            
            var motor = target as KinematicCharacterMotor;
            if (motor)
            {
                var characterBottom = motor.transform.position + motor.Capsule.center + (-Vector3.up * (motor.Capsule.height * 0.5f));

                Handles.color = Color.yellow;
                Handles.CircleHandleCap(
                    0, 
                    characterBottom + (motor.transform.up * motor.MaxStepHeight), 
                    Quaternion.LookRotation(motor.transform.up, motor.transform.forward), 
                    motor.Capsule.radius + 0.1f, 
                    EventType.Repaint);
            }
        }
    }
}
#endif