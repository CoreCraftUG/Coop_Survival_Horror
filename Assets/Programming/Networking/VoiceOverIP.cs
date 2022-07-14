using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CoreCraft
{
    public class VoiceOverIP : NetworkBehaviour
    {
        [SerializeField] private float _minThreshold = 10.0f;
        [SerializeField] private float _frequency = 0.0f;
        [SerializeField] private int _audioSampleRate = 44100;
        [SerializeField] private string _microphoneName;
        [SerializeField] private FFTWindow _fftWindow;
        [SerializeField] private TMP_Dropdown _microphoneDropdown;
        [SerializeField] private Slider _thresholdSlider;
        [SerializeField] private Slider _sensitivitySlider;

        private List<string> _availableMicrophones = new List<string>();
        private int _sample = 8192;
        private AudioSource _audioSource;/* { get => _audioSource;
            set
            {
                _audioSource = value;
                if (_microphoneName != null)
                    UpdateMicrophone();
            }
        }*/

        void Start()
        {
            // _audioSource = gameObject.GetComponent<AudioSource>(); // TODO: Remove this

            FillMicrophoneDeviceList();
            
            _microphoneDropdown.onValueChanged.AddListener(delegate
            {
                MicDropdownValueChangeHandlerServerRpc(_microphoneDropdown.value);
            });

            _thresholdSlider.value = _minThreshold;
            _thresholdSlider.onValueChanged.AddListener(delegate
            {
                ThresholdSliderValueChangeHandlerServerRpc(_thresholdSlider.value);
            });

            if (_audioSource != null)
                UpdateMicrophone();
        }

        private void FillMicrophoneDeviceList()
        {
            _availableMicrophones.Clear();
            foreach (string device in Microphone.devices)
            {
                _microphoneName ??= device;
                _availableMicrophones.Add(device);
            }
            _microphoneDropdown.ClearOptions();
            _microphoneDropdown.AddOptions(_availableMicrophones);
        }

        private void UpdateMicrophone()
        {
            _audioSource.Stop();

            _audioSource.clip = Microphone.Start(_microphoneName, true, 10, _audioSampleRate);
            _audioSource.loop = true;

            if (Microphone.IsRecording(_microphoneName))
            {
                while (!(Microphone.GetPosition(_microphoneName) > 0)) { } // Wait until the recording has started TODO: Find a better way //Microphone.IsRecording should prevent infinite loop\\

                _audioSource.Play();
            }
            else
            {

            }
        }

        [ServerRpc]
        private void MicDropdownValueChangeHandlerServerRpc(int microphone)
        {
            _microphoneName = _availableMicrophones[microphone];
            if (_audioSource != null)
                UpdateMicrophone();
            MicDropdownValueChangeHandlerClientRpc(microphone);
        }

        [ClientRpc]
        private void MicDropdownValueChangeHandlerClientRpc(int microphone)
        {
            _microphoneName = _availableMicrophones[microphone];
            if (_audioSource != null)
                UpdateMicrophone();
        }

        [ServerRpc]
        private void ThresholdSliderValueChangeHandlerServerRpc(float threshold)
        {
            _minThreshold = threshold;
            ThresholdSliderValueChangeHandlerClientRpc(threshold);
        }

        [ClientRpc]
        private void ThresholdSliderValueChangeHandlerClientRpc(float threshold) => _minThreshold = threshold;

        [ServerRpc(RequireOwnership = false)]
        public void SetAudioSourceServerRpc(ulong audioSourceObject, ulong clientId)
        {
            _audioSource = GetComponent<NetworkObject>().OwnerClientId == clientId ? NetworkManager.Singleton.SpawnManager.SpawnedObjects[audioSourceObject].GetComponent<AudioSource>() : null;
            SetAudioSourceClientRpc(audioSourceObject, clientId);
        }

        [ClientRpc]
        private void SetAudioSourceClientRpc(ulong audioSourceObject, ulong clientId)
            => _audioSource = GetComponent<NetworkObject>().OwnerClientId == clientId ? NetworkManager.Singleton.SpawnManager.SpawnedObjects[audioSourceObject].GetComponent<AudioSource>() : null;
    }
}