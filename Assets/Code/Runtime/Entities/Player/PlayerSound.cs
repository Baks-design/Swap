using System;
using System.Linq;
using SwapChains.Runtime.Entities.Damages;
using SwapChains.Runtime.Entities.Player;
using SwapChains.Runtime.Utilities.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SwapChains.Runtime.Audio
{
    [Serializable]
    public class PlayerSound
    {
        [Header("Settings")]
        [SerializeField, Range(0.1f, 1f)] float groundCheckRadius = 0.1f;

        [Header("Refs")]
        [SerializeField] AudioSource footStepsSource;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip[] hurtAudioClips;
        [SerializeField] AudioClip[] deadAudioClips;
        [SerializeField] MaterialMatchEntry[] MaterialMatchList;
        readonly Collider[] m_CollidersBuffer = new Collider[16];

        public AudioClip[] HurtAudioClips => hurtAudioClips;
        public AudioClip[] DeadAudioClips => deadAudioClips;

        public void Update(PlayerController controller) => FootstepsHandle(controller);

        public void Play(AudioClip clip)
        {
            var value = Random.value;
            audioSource.pitch = value < 0.5f ? value + 0.5f : value;
            audioSource.PlayOneShot(clip);
        }

        void FootstepsHandle(PlayerController controller)
        {
            var count = Physics.OverlapSphereNonAlloc(controller.Transform.localPosition, groundCheckRadius, m_CollidersBuffer, ~(1 << 30));
            for (var i = 0; i < count; ++i)
            {
                var renderer = m_CollidersBuffer[i].gameObject.GetComponentInChildren<Renderer>();
                if (renderer)
                {
                    for (var j = 0; j < renderer.sharedMaterials.Length; j++)
                    {
                        for (var k = 0; k < MaterialMatchList.Length; k++)
                        {
                            if (MaterialMatchList[i].Materials.Contains(renderer.sharedMaterials[i]))
                            {
                                if (footStepsSource.resource != MaterialMatchList[i].RandomContainer)
                                {
                                    footStepsSource.Stop();
                                    footStepsSource.resource = MaterialMatchList[i].RandomContainer;
                                    footStepsSource.Play();
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}