using System.Collections.Generic;
using UnityEngine;

namespace MusouEcs
{
    public class MusouMapMgr
    {
        private Vector3 _center = Vector3.zero;
        private List<SpriteRenderer> _renderers = new();
        private float _width;
        private float _height;
        private Vector2 _wh = Vector2.zero;
        private GameObject _root;

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

        public void Create(string texturePath)
        {
            _root = new GameObject("MapMgrRoot");
            // 加载图片
            var tex = Resources.Load<Texture2D>(texturePath); //todo
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            _width = sprite.texture.width / sprite.pixelsPerUnit;
            _height = sprite.texture.height / sprite.pixelsPerUnit;
            _wh = new Vector2(_width, _height);
            // 生成9张renderer
            for (var i = 0; i < 9; i++)
            {
                var go = new GameObject("MapBg" + (i + 1))
                {
                    transform =
                    {
                        parent = _root.transform
                    }
                };
                var spriteRender = go.AddComponent<SpriteRenderer>();
                spriteRender.sprite = sprite;
                _renderers.Add(spriteRender);
                spriteRender.sortingOrder = (int)MusouLayer.Bg;
            }

            FixBg();
        }

        public void Destroy()
        {
            Object.Destroy(_root);
            _renderers.Clear();
            _renderers = null;
        }

        public void ViewTo(Vector3 target)
        {
            var delta = target - _center;
            var change = false;
            if (Mathf.Abs(delta.x) > _width / 2)
            {
                change = true;
                if (delta.x < 0)
                {
                    _center.x -= _width;
                }
                else
                {
                    _center.x += _width;
                }
            }

            if (Mathf.Abs(delta.y) > _height / 2)
            {
                change = true;
                if (delta.y < 0)
                {
                    _center.y -= _height;
                }
                else
                {
                    _center.y += _height;
                }
            }

            if (change)
            {
                FixBg();
            }
        }

        private void FixBg()
        {
            for (var i = 0; i < _renderers.Count; i++)
            {
                //根据center确定该图位置
                _renderers[i].transform.position = _center + (Vector3)(_dir[i] * _wh);
            }
        }
    }
}