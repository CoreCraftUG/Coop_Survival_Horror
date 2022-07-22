using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreCraft.Networking.Steam;
using Steamworks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
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

            // if (_audioSource != null)
            // UpdateMicrophone();


            SteamUser.VoiceRecord = true;

            mic = Microphone.Start(null, true, 10, FREQUENCY);

            AudioSource audio = GetComponent<AudioSource>();
            audio.clip = AudioClip.Create("test", 10 * FREQUENCY, mic.channels, FREQUENCY, false);
            audio.loop = true;

        }

        const int FREQUENCY = 44100;
        AudioClip mic;
        int lastPos, pos;

        private async void Update()
        {
            MemoryStream stream = new MemoryStream();
            int voiceDataLength = SteamUser.ReadVoiceData(stream);

            byte[] voiceData = SteamUser.ReadVoiceDataBytes(); // Variant 1 // interference \\


            if (voiceData == null || voiceDataLength <= 0)
                return;
            MemoryStream stream2 = new MemoryStream();
            int i = SteamUser.DecompressVoice(stream, voiceDataLength, stream2);

            if (i <= 0)
                return;

            // Debug.Log($"{i}");
            // SteamUser.DecompressVoice(voiceData, stream2);
            // Debug.Log($"{i}");

            byte[] buffer = stream2.GetBuffer(); // Variant 2 // interference \\
            
            await stream2.ReadAsync(buffer); // Variant 3 // interference \\

            // byte[] arr = stream2.ToArray(); // Variant 4 // interference \\


            float[] test = new float[buffer.Length / 4];
            
            Buffer.BlockCopy(buffer, 0, test, 0, test.Length);
            AudioClip clip = AudioClip.Create("voice", test.Length, 1, 44100, false, false);//OnAudioRead, OnAudioSetPosition);
            clip.SetData(test, 0);
            _audioSource.clip = clip;
            _audioSource.Play();


            return;

            if ((pos = Microphone.GetPosition(null)) > 0)
            {
                if (lastPos > pos) lastPos = 0;

                if (pos - lastPos > 0)
                {
                    // Allocate the space for the sample.
                    float[] sample = new float[(pos - lastPos) * mic.channels];

                    // Get the data from microphone.
                    mic.GetData(sample, lastPos);
                    if (sample.Length > 6)
                        Debug.Log($"{sample[0]}, {sample[1]}, {sample[2]}, {sample[3]}, {sample[4]}, {sample[5]}, ");
                    else
                        Debug.Log($"{sample.Length}");


                    byte[] sampleBytes = new byte[sample.Length * 4];
                    Buffer.BlockCopy(sample, 0, sampleBytes, 0, sampleBytes.Length);
                    if (sample.Length > 6)
                        Debug.Log($"{sampleBytes[0]}, {sampleBytes[1]}, {sampleBytes[2]}, {sampleBytes[3]}, {sampleBytes[4]}, {sampleBytes[5]}, ");
                    else
                        Debug.Log($"{sampleBytes.Length}");


                    float[] sampleFloats = new float[sampleBytes.Length / 4];
                    Buffer.BlockCopy(sampleBytes, 0, sampleFloats, 0, sampleFloats.Length);
                    if (sampleFloats.Length > 6)
                        Debug.Log($"{sampleFloats[0]}, {sampleFloats[1]}, {sampleFloats[2]}, {sampleFloats[3]}, {sampleFloats[4]}, {sampleFloats[5]}, ");
                    else
                        Debug.Log($"{sampleFloats.Length}");

                    AudioSource audio = GetComponent<AudioSource>();
                    audio.clip.SetData(sample, lastPos);

                    if (!audio.isPlaying) audio.Play();
                    // SendFloatArrayServerRpc(sampleBytes, lastPos,NetworkManager.Singleton.LocalClientId);
                    lastPos = pos;
                }
            }

            #region old

            return;
            if (!SteamUser.HasVoiceData)
                return;

            Debug.Log($"Has Voice Data");
            Test();
            return;

            // MemoryStream stream = new MemoryStream();
            // int voiceDataLength = SteamUser.ReadVoiceData(stream);
            // if (voiceDataLength <= 0)
            //     return;

            // byte[] voiceData = new byte[voiceDataLength];
            // voiceData = SteamUser.ReadVoiceDataBytes();
            // if (voiceData == null)
            //     return;
            //
            // SendVoiceDataServerRpc(NetworkManager.Singleton.LocalClientId, voiceData);

            #endregion
        }

        private byte[] ConvertFloatToByte(float[] array)
        {
            byte[] returnArray = new byte[0];

            foreach (float f in array)
            {
                byte[] arr = BitConverter.GetBytes(f);
                returnArray.Union(arr);
            }

            return returnArray;
        }

        [ServerRpc]
        private void SendFloatArrayServerRpc(byte[] sample, int lasPos, ulong clientId)
        {
            SendFloatArrayClientRpc(sample,lasPos,clientId);
        }

        [ClientRpc]
        private void SendFloatArrayClientRpc(byte[] sample, int lasPos, ulong clientId)
        {
            // Put the data in the audio source.
            AudioSource audio = GetComponent<AudioSource>();
            float[] sampleFloats = new float[sample.Length / 4];
            Buffer.BlockCopy(sample, 0, sampleFloats, 0, sampleFloats.Length);
            // Debug.Log($"{sampleFloats[0]}, {sampleFloats[1]}, {sampleFloats[2]}, {sampleFloats[3]}, {sampleFloats[4]}, {sampleFloats[5]}, ");
            audio.clip.SetData(sampleFloats, lasPos);

            if (!audio.isPlaying) audio.Play();
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

        public void StartSteamRecording(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Started)
                return;

            MemoryStream stream = new MemoryStream();
            int voiceDataLength = SteamUser.ReadVoiceData(stream);
            // if (voiceDataLength <= 0)
            //     return;

            byte[] voiceData = new byte[voiceDataLength];
            voiceData = SteamUser.ReadVoiceDataBytes();
            if (voiceData == null)
                return;

            SendVoiceDataServerRpc(NetworkManager.Singleton.LocalClientId, voiceData);
        }

        private async void Test()
        {
            MemoryStream stream = new MemoryStream();
            int voiceDataLength = SteamUser.ReadVoiceData(stream);
            // if (voiceDataLength <= 0)
            //     return;

            byte[] voiceData = SteamUser.ReadVoiceDataBytes();
            if (voiceData == null || voiceDataLength <= 0)
                return;


            // int i = SteamUser.DecompressVoice(voiceData, stream);
            byte[] buffer = stream.GetBuffer();
            float seek = stream.Seek(0, SeekOrigin.Begin);
            Debug.Log($"Stream Seek float {seek}");
            AudioClip clip = AudioClip.Create("voice", voiceDataLength, 1, 44100, false, false);//OnAudioRead, OnAudioSetPosition);
            if (!stream.CanRead)
            {
                Debug.Log($"Can't read Stream");
                return;
            }
            
            await stream.ReadAsync(buffer);
            byte[] streamToArray = stream.ToArray();
            Test2(clip, buffer);
            // stream.Dispose();
        }

        private void Test2(AudioClip clip, byte[] voiceData)
        {
            float[] test = new float[voiceData.Length / 4];
            Buffer.BlockCopy(voiceData, 0, test, 0, test.Length);
            clip.SetData(test, 0);
            _audioSource.clip = clip;
            _audioSource.Play();
        }

        [ServerRpc]
        private void SendVoiceDataServerRpc(ulong clientId, byte[] voiceData)
        {
            DecodeVoiceDataClientRpc(clientId,voiceData);
        }

        [ClientRpc]
        private void DecodeVoiceDataClientRpc(ulong clientId, byte[] voiceData)
        {
            MemoryStream stream = new MemoryStream();
            int i = SteamUser.DecompressVoice(voiceData, stream);
            if (stream.CanRead)
            {
                PlayAudioFromStream(stream, voiceData);
            }
            // Debug.Log($"Stream was auch immer das ist: {stream}\nDer komische int den mit DecompressVoice rauswirft: {i}");
            // Debug.Log($"Stream length {stream.Length}");
            // Debug.Log($"Stream position {stream.Position}");
            // Debug.Log($"Stream can read {stream.CanRead}");
            // Debug.Log($"Stream can seek {stream.CanSeek}");
            // Debug.Log($"Stream can write {stream.CanWrite}");
            // Debug.Log($"Stream capacity {stream.Capacity}");
            // Debug.Log($"Stream can timeout {stream.CanTimeout}");
            // // Debug.Log($"Stream read timeout {stream.ReadTimeout}");
            // Debug.Log($"Stream write timeout {stream.WriteTimeout}");
        }

        private async void PlayAudioFromStream(MemoryStream stream, byte[] voiceData)
        {
            byte[] buffer = stream.GetBuffer();
            float seek = stream.Seek(0, SeekOrigin.Begin);
            Debug.Log($"Stream Seek float {seek}");
            await stream.ReadAsync(buffer, 0, buffer.Length);
            // stream.Dispose();
            AudioClip clip = AudioClip.Create("voice", (int)stream.Length, 1, 44100, false,false);//OnAudioRead, OnAudioSetPosition);
            byte[] streamToArray = stream.ToArray();
            float[] test = new float[buffer.Length / 4];
            Buffer.BlockCopy(buffer,0,test,0,test.Length);

            clip.SetData(test, 0);
            _audioSource.clip = clip;
            _audioSource.Play();
        }

        public float[] ConvertByteToFloat(byte[] array)
        {
            float[] floatArr = new float[array.Length / 4];
            for (int i = 0; i < floatArr.Length; i++)
            {
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(array, i * 4, 4);
                }
                floatArr[i] = BitConverter.ToSingle(array, i * 4);
            }
            return floatArr;
        }

        public int position = 0;
        public int samplerate = 44100;
        public float frequency = 440;

        void OnAudioRead(float[] data)
        {
            int count = 0;
            while (count < data.Length)
            {
                data[count] = Mathf.Sin(2 * Mathf.PI * frequency * position / samplerate);
                position++;
                count++;
            }
        }

        void OnAudioSetPosition(int newPosition)
        {
            position = newPosition;
        }
    }
}