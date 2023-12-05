using UnityEngine;

namespace MusouEcs
{
    public class MusouCameraContext
    {
        public Transform Target;
        public MusouMapMgr MapMgr;
    }

    public class MusouCamera : MonoBehaviour
    {
        public static Camera Main = null;
        public static MusouCamera Inst = null;

        public float smoothTime = 0.3f;

        private MusouCameraContext _context;
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
            if (_context == null) return;

            var cur = Vector2.SmoothDamp(transform.position, _context.Target.position, ref _velocity, smoothTime);
            transform.position = new Vector3(cur.x, cur.y, -10);
            _context.MapMgr.ViewTo(new Vector3(cur.x, cur.y, 0));
        }

        public void SetContext(MusouCameraContext context)
        {
            _context = context;
            transform.position = context.Target.TransformPoint(new Vector3(0, 0, -10));
            _velocity = Vector2.zero;
        }

        public void RemoveContext()
        {
            _context = null;
        }
    }
}