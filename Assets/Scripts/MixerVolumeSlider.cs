using UnityEngine;
using UnityEngine.UI;

public static class MixerVolumeMath
{
    public static float ToLinearVolume(float normalizedValue, float minimumDecibels)
    {
        if (normalizedValue <= 0f)
        {
            return 0f;
        }

        var decibels = Mathf.Max(
            minimumDecibels,
            20f * Mathf.Log10(Mathf.Clamp01(normalizedValue)));
        return Mathf.Pow(10f, decibels / 20f);
    }
}

public sealed class MixerVolumeSlider : MonoBehaviour
{
    [SerializeField] private AudioSource trackAudioSource;
    [SerializeField, Range(-100f, -20f)] private float minimumDecibels = -80f;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }

        if (slider != null)
        {
            slider.onValueChanged.AddListener(SetVolume);
            SetVolume(slider.value);
        }
    }

    private void OnDisable()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(SetVolume);
        }
    }

    public void SetVolume(float normalizedValue)
    {
        if (trackAudioSource != null)
        {
            trackAudioSource.volume = MixerVolumeMath.ToLinearVolume(normalizedValue, minimumDecibels);
        }
    }
}
