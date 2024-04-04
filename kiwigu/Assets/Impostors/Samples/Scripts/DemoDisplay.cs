using System;
using System.Linq;
using Impostors.Managers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Impostors.Samples
{
    [AddComponentMenu("")]
    public class DemoDisplay : MonoBehaviour
    {
        [SerializeField]
        private Slider _lodBiasSlider;

        [SerializeField]
        private Slider _textureSizeScaleSlider;

        [SerializeField]
        private Slider _cutoffSlider;

        [SerializeField]
        private Toggle _debugModeToggle;

        [SerializeField]
        private Toggle _debugCascadesToggle;

        [SerializeField]
        private Toggle _debugFadingToggle;

        private void Start()
        {
            _lodBiasSlider.value = QualitySettings.lodBias;
            _cutoffSlider.value = ImpostorLODGroupsManager.Instance.cutoff;

            var manager = ImpostorLODGroupsManager.Instance.CameraManagers.First();
            _debugModeToggle.isOn = manager.debugModeEnabled;
            _debugCascadesToggle.isOn = manager.debugCascadesModeEnabled;
            _debugFadingToggle.isOn = manager.debugFadingEnabled;
            _textureSizeScaleSlider.value = manager.textureSizeScale;
            
            
            _lodBiasSlider.onValueChanged.AddListener(OnLoadBiasSliderValueChanged);
            _cutoffSlider.onValueChanged.AddListener(OnCutoffSliderValueChanged);
            _textureSizeScaleSlider.onValueChanged.AddListener(_ => UpdateAllCameraManagers());
            _debugModeToggle.onValueChanged.AddListener(_ => UpdateAllCameraManagers());
            _debugCascadesToggle.onValueChanged.AddListener(_ => UpdateAllCameraManagers());
            _debugFadingToggle.onValueChanged.AddListener(_ => UpdateAllCameraManagers());
        }

        private void OnLoadBiasSliderValueChanged(float value)
        {
            QualitySettings.lodBias = value;
        }

        private void OnCutoffSliderValueChanged(float value)
        {
            ImpostorLODGroupsManager.Instance.cutoff = value;
        }

        private void UpdateAllCameraManagers()
        {
            foreach (var manager in ImpostorLODGroupsManager.Instance.CameraManagers)
            {
                manager.textureSizeScale = _textureSizeScaleSlider.value;
                manager.debugCascadesModeEnabled = _debugCascadesToggle.isOn;
                manager.debugFadingEnabled = _debugFadingToggle.isOn;
                manager.debugModeEnabled = _debugModeToggle.isOn || manager.debugCascadesModeEnabled || manager.debugFadingEnabled;
                manager.debugColor = _debugModeToggle.isOn ? new Color(0, 1, 0, 0) : Color.clear;
            }
        }
    }
}