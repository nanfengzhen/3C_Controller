using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ZZZ.SoundAssets
{
    [CreateAssetMenu(fileName = "Sound", menuName = "Creat/Assets/Sound", order = 0)] //使该脚本能够成为图形化元素
    public class AudioSoundSO : ScriptableObject
    {
        [System.Serializable]
        public class Sound
        {
            public AudioType audioType;
            public AudioClip[] audioClip;

        }

        [SerializeField] private List<Sound> configSound = new List<Sound>();



        public AudioClip GetAudioClips(AudioType audioType)
        {
            if (configSound.Count == 0) return null;

            switch (audioType)
            {
                case AudioType.WALK:
                    return configSound[0].audioClip[Random.Range(0, configSound[0].audioClip.Length)];
                case AudioType.RUN:
                    return configSound[1].audioClip[Random.Range(0, configSound[1].audioClip.Length)];
                case AudioType.DASH:
                    return configSound[2].audioClip[Random.Range(0, configSound[2].audioClip.Length)];
                case AudioType.ATK:
                    return configSound[3].audioClip[Random.Range(0, configSound[3].audioClip.Length)];
                    


            }
            return null;

        }
    }
}


