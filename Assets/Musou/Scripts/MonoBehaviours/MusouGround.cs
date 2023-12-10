using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusouEcs
{
    public class MusouGround : MonoBehaviour
    {
        public float width;
        public float height;
        public List<GameObject> mapObjs;
        
        private Vector3 _center = Vector3.zero;
        private Vector2 _wh = Vector2.zero;

        private readonly Vector2[] _dir =
        {
            new(-1, 1),
            new(0, 1),
            new(1, 1),
            new(-1, 0),
            new(0, 0),
            new(1, 0),
            new(-1, -1),
            new(0, -1),
            new(1, -1)
        };

        public void Awake()
        {
            _wh = new Vector2(width, height);
        }

        public void Destroy()
        {
        }

        public void ViewTo(Vector3 target)
        {
            var delta = target - _center;
            var change = false;
            if (Mathf.Abs(delta.x) > width / 2)
            {
                change = true;
                if (delta.x < 0)
                {
                    _center.x -= width;
                }
                else
                {
                    _center.x += width;
                }
            }

            if (Mathf.Abs(delta.y) > height / 2)
            {
                change = true;
                if (delta.y < 0)
                {
                    _center.y -= height;
                }
                else
                {
                    _center.y += height;
                }
            }

            if (change)
            {
                FixBg();
            }
        }

        private void FixBg()
        {
            for (var i = 0; i < mapObjs.Count; i++)
            {
                //根据center确定该图位置
                mapObjs[i].transform.position = _center + (Vector3)(_dir[i] * _wh);
            }
        }
    }
}