using FairyGUI;
using Spine.Unity;
using UnityEngine;

namespace MusouEcs
{
    public sealed class MusouMain : MonoBehaviour
    {
        public UIPanel uiPanel;
        public GameObject simulatePlayerPrefab;
        public string mapPath;

        private GComponent _mainView;
        private GTextField _text;
        private JoystickModule _joystick;
        private Coroutine _coroutine;

        private SkeletonAnimation _spineAnimation;
        private Vector2 _playerLastPos;
        private bool _isPlayerStop;

        private MusouMapMgr _mapMgr;

        private int _face = 1;

        private void Start()
        {
            Application.targetFrameRate = 60;

            //ui
            Stage.inst.onKeyDown.Add(OnKeyDown);
            _mainView = uiPanel.ui;
            _text = _mainView.GetChild("n9").asTextField;
            _joystick = new JoystickModule(_mainView);
            _joystick.onMove.Add(__joystickMove);
            _joystick.onEnd.Add(__joystickEnd);

            //地图
            _mapMgr = new MusouMapMgr();
            _mapMgr.Create(mapPath);

            //模拟主角
            var simulatePlayer = Instantiate(simulatePlayerPrefab);
            simulatePlayer.transform.position = Vector3.zero;
            simulatePlayer.transform.localScale = Vector3.one;
            _playerLastPos = Vector3.zero;

            _spineAnimation = simulatePlayer.GetComponent<SkeletonAnimation>();
            _spineAnimation.AnimationState.SetAnimation(0, "idle", true);
            simulatePlayer.GetComponent<MeshRenderer>().sortingOrder = (int)MusouLayer.Player;

            MusouCamera.Inst.SetContext(new MusouCameraContext
            {
                Target = simulatePlayer.transform,
                MapMgr = _mapMgr,
            });
        }

        private void Update()
        {
            var curPos = SharedStaticPlayerData.SharedValue.Data.PlayerPosition;

            if (_playerLastPos == curPos && !_isPlayerStop)
            {
                _spineAnimation.AnimationState.SetAnimation(0, "idle", true);
                _isPlayerStop = true;
            }
            else if (_playerLastPos != curPos && _isPlayerStop)
            {
                _spineAnimation.AnimationState.SetAnimation(0, "walk", true);
                _isPlayerStop = false;
            }

            var delta = curPos - _playerLastPos;
            _spineAnimation.transform.position = curPos;
            DetectionDir(delta);
            _playerLastPos = curPos;
        }

        private void DetectionDir(Vector3 delta)
        {
            //x不变不需要更改方向
            if (delta.x == 0) return;
            var curFace = delta.x > 0 ? 1 : -1;
            if (curFace == _face) return;
            _face = curFace;
            SetDir(_face);
        }

        private void SetDir(int face)
        {
            _spineAnimation.transform.rotation = Quaternion.Euler(0, face > 0 ? 0 : 180, 0);
        }

        private void ChangePlayerMoveDir(Vector3 dir)
        {
            SharedStaticPlayerData.SharedValue.Data.PlayerMoveDir = dir;
        }

        private void __joystickMove(EventContext context)
        {
            var angle = (float)context.data;
            var v3 = (Quaternion.Euler(new Vector3(0, 0, angle)) * new Vector3(1, 0, 0));
            v3.y = -v3.y;
            ChangePlayerMoveDir(v3);
            _text.text = "" + angle;
        }

        private void __joystickEnd()
        {
            ChangePlayerMoveDir(Vector3.zero);
            _text.text = "";
        }

        private void OnKeyDown(EventContext context)
        {
            if (context.inputEvent.keyCode == KeyCode.Escape)
            {
                Application.Quit();
            }
        }
    }
}