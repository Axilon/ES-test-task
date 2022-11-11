using System;
using Interfaces;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.UI;

namespace VideoRecorder
{
    public class VideoRecorderManager : BaseApplicationContextComponent
    {
        [Header("Buttons")] 
        [SerializeField] private Button _startRecording;
        [SerializeField] private Button _endRecording;
        [SerializeField] private Button _encodeLast;
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _recordingColor = Color.red;
        
        [Header("Settings")]
        [SerializeField] private bool _useScreenSize;
        [SerializeField] private bool _initOnAwake;
        [SerializeField] private bool _preserveAudio = true;
        [SerializeField] private VideoBitrateMode _bitrateMode;
        [SerializeField] private int _outputWidth = 640;
        [SerializeField] private int _outputHeigth = 480;
        [Range(0,120)]
        [SerializeField] private float _frameRate = 30;

        private bool _initialized;
        
        private RecorderControllerSettings _recorderControllerSettings;
        private RecorderController _recorderController;
        private MovieRecorderSettings _movieRecorderSettings;
        
        private string _recordingFolder;
        private string _lastClipName;
        
        
        public override void Init()
        {
            CheckOutFolder();
            InitButtons();
            SetButtonsColor(_defaultColor);
            
            if(!_initOnAwake) return;
            InitRecorder();
        }

        private void InitButtons()
        {
            _startRecording.onClick.AddListener(StartRecording);
            _endRecording.onClick.AddListener(StopRecording);
            _encodeLast.onClick.AddListener(EncodeLastClip);
        }
        private void CheckOutFolder()
        {
            var dataPath = Application.dataPath;
            var a = Application.dataPath + "/Recording";
            _recordingFolder = System.IO.Path.Combine(dataPath, "Recording");
            if(System.IO.Directory.Exists(_recordingFolder)) return;
            System.IO.Directory.CreateDirectory(_recordingFolder);
        }
        
        private void InitRecorder()
        {
            if(_initialized) return;
            _recorderControllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            _recorderController = new RecorderController(_recorderControllerSettings);
            _movieRecorderSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            _movieRecorderSettings.name = "My Video Recorder";
            _movieRecorderSettings.Enabled = true;
            _movieRecorderSettings.VideoBitRateMode = _bitrateMode;
            _movieRecorderSettings.ImageInputSettings = new GameViewInputSettings()
            {
                OutputWidth = _useScreenSize ? Screen.width : _outputWidth,
                OutputHeight = _useScreenSize ? Screen.height : _outputHeigth
            };
            _movieRecorderSettings.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MOV;
            _movieRecorderSettings.AudioInputSettings.PreserveAudio = _preserveAudio;
            
            _recorderControllerSettings.FrameRate = _frameRate;
            RecorderOptions.VerboseMode = false;
            
            _initialized = true;
        }

        private void SetButtonsColor(Color color)
        {
            _startRecording.image.color = color;
            _endRecording.image.color = color;
            _encodeLast.image.color = color;
        }
        
        private void StartRecording()
        {
            SetButtonsColor(_recordingColor);
            InitRecorder();
            _movieRecorderSettings.OutputFile = GetFileName(); 
 
            _recorderControllerSettings.AddRecorderSettings(_movieRecorderSettings);
            RecorderOptions.VerboseMode = false;
            _recorderController.PrepareRecording();
            _recorderController.StartRecording();
        }

        private void StopRecording()
        {
            if(!_initialized) return;
            if(_recorderController == null || !_recorderController.IsRecording()) return;
            _recorderController.StopRecording();
            
            SetButtonsColor(_defaultColor);
        }
        
        private string GetFileName()
        {
            _lastClipName = "movie_" + DateTime.Now.ToString("dd_MMM_yyy_HH_mm_ss");
            return System.IO.Path.Combine(_recordingFolder, _lastClipName);
        }

        private void EncodeLastClip()
        {
            if(!_initialized || string.IsNullOrEmpty(_lastClipName) || _recorderController.IsRecording()) return;
            FFmpegManager.EncodeVideo(_recordingFolder,_lastClipName,".mov");
            //FFmpegManager.EncodeVideo("movie_11_Nov_2022_15_17_26","movie_11_Nov_2022_15_17_26");
        }

        private void OnDestroy()
        {
            _startRecording.onClick.RemoveAllListeners();
            _endRecording.onClick.RemoveAllListeners();
            _encodeLast.onClick.RemoveAllListeners();
            
            StopRecording();
            
            _recorderControllerSettings = null;
            _recorderController = null;
            _movieRecorderSettings = null;

            _initialized = false;
        }
    }
}