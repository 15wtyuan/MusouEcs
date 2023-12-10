using UnityEngine;

namespace MusouEcs
{
    public class MusouCameraContext
    {
        public Transform Target;
        public MusouGround Ground;
    }

    public class MusouCamera : MonoBehaviour
    {
        public static Camera Main;
        public static MusouCamera Inst;
        
        public MusouGround ground;

        public float smoothTime = 0.3f;
        private Vector2 _velocity = Vector2.zero;

        private void Awake()
        {
            Main = GetComponent<Camera>();
            Inst = this;
            DontDestroyOnLoad(Main);
            
            // Main.transparencySortMode = TransparencySortMode.CustomAxis;
            // Main.transparencySortAxis = new Vector3(0, 1, -1);
        }

        private void OnDestroy()
        {
            Main = null;
            Inst = null;
        }

        private void LateUpdate()
        {
            var playerPos = SharedStaticPlayerData.SharedValue.Data.PlayerPosition;
            var cur = Vector2.SmoothDamp(transform.position, playerPos, ref _velocity, smoothTime);
            transform.position = new Vector3(cur.x, cur.y, -10);
            ground.ViewTo(new Vector3(cur.x, cur.y, 0));
        }
    }
}