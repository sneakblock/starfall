using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSpectrum : MonoBehaviour
{
    public AudioSource audioSource;
    private const int _samples = 512;

    private static float[] _spectrumData;
    private static readonly float[] FreqBands = new float[8];
    private static readonly float[] BandBuffer = new float[8];
    private readonly float[] _bufferDecrease = new float[8];

    private float[] _freqBandHighest = new float[8];
    public static float[] audioBand = new float[8];
    public static float[] audioBandBuffer = new float[8];

    public static float amplitude, amplitudeBuffer;
    private float _amplitudeHighest;
    
    // Start is called before the first frame update
    private void Start()
    {
        ResetSpectrum();
    }

    public void ResetSpectrum()
    {
        _spectrumData = new float[_samples];
    }

    // Update is called once per frame
    private void Update()
    {
        if (!audioSource) return;
        GetSpectrumData(audioSource);
        GetFreqBands(_spectrumData);
        MakeBandBuffer();
        CreateAudioBands();
    }

    private void GetSpectrumData(AudioSource a)
    {
        a.GetSpectrumData(_spectrumData, 0, FFTWindow.Blackman);
    }

    private void GetFreqBands(float[] sampleArr)
    {
        // Mentions file is 44100HZ
        // 44100 / 512 = about 86 hz per sample
        
        // ummmmmm?

        int count = 0;

        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int) Mathf.Pow(2, i);
            if (i == 7) sampleCount += 2;
            
            for (int j = 0; j < sampleCount; j++)
            {
                average += sampleArr[count] * (count + 1);
                count++;
            }
            average /= count;
            FreqBands[i] = average * 10;
        }
    }

    private void MakeBandBuffer()
    {
        for (int i = 0; i < 8; i++)
        {
            if (FreqBands[i] > BandBuffer[i])
            {
                BandBuffer[i] = FreqBands[i];
                _bufferDecrease[i] = .005f;
            }

            if (FreqBands[i] < BandBuffer[i])
            {
                BandBuffer[i] -= _bufferDecrease[i];
                _bufferDecrease[i] *= 1.2f;
            }
        }
    }

    /// <summary>
    /// AudioBand and AudioBandBuffer will always be between 0 and 1.
    /// </summary>
    void CreateAudioBands()
    {
        for (int i = 0; i < 8; i++)
        {
            if (FreqBands[i] > _freqBandHighest[i]) _freqBandHighest[i] = FreqBands[i];
            audioBand[i] = FreqBands[i] / _freqBandHighest[i];
            audioBandBuffer[i] = BandBuffer[i] / _freqBandHighest[i];
        }
    }

    void GetAmplitude()
    {
        float currAmplitude = 0f;
        float currAmplitudeBuffer = 0f;
        for (int i = 0; i < 8; i++)
        {
            currAmplitude += audioBand[i];
            currAmplitudeBuffer += audioBandBuffer[i];
        }

        if (currAmplitude > _amplitudeHighest) _amplitudeHighest = currAmplitude;

        amplitude = currAmplitude / _amplitudeHighest;
        amplitudeBuffer = currAmplitudeBuffer / _amplitudeHighest;
    }
}
