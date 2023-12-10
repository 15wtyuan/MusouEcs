using UnityEngine;

namespace MusouEcs
{
    public sealed class MusouMain : MonoBehaviour
    {
        public static MusouMain Inst;

        private GridSearchBurst _gsb;
        public GridSearchBurst Gsb => _gsb;

        private void Awake()
        {
            Inst = this;

            // Application.targetFrameRate = 60;

            // 暂时找个地方存一下
            _gsb = new GridSearchBurst(-1, 28);

            // 初始化一下
            SharedStaticPlayerData.SharedValue.Data.PlayerMoveDir = Vector3.zero;
        }

        private void OnDestroy()
        {
            Inst = null;
        }

        public void ChangePlayerMoveDir(Vector2 dir)
        {
            SharedStaticPlayerData.SharedValue.Data.PlayerMoveDir = dir;
        }
    }
}