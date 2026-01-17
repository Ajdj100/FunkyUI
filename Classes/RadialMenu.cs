using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace FunkyUI.Classes
{
    public class RadialMenuIcon
    {
        public readonly Texture2D Texture;
        public readonly Sprite Sprite;

        public RadialMenuIcon(Texture2D texture) { Texture = texture; Sprite = null; }
        public RadialMenuIcon(Sprite sprite) { Sprite = sprite; Texture = null; }

        public void ApplyTo(Image image)
        {
            if (Sprite != null) { image.sprite = Sprite; image.image = null; }
            else { image.image = Texture; image.sprite = null; }
        }
    }

    public class RadialMenuOption
    {
        public string Name;
        public string Description;
        public RadialMenuIcon Icon;
        public Action Callback;
    }

    public class RadialMenuSlice : VisualElement
    {
        public float StartRad;
        public float EndRad;
        public float InnerRadius;
        public float OuterRadius;
        public Color Color;
        public bool IsHovered;

        private VisualElement _contentGroup;
        private Image _iconImage;
        private Label _nameLabel;

        public RadialMenuSlice()
        {
            generateVisualContent += OnGenerate;
            pickingMode = PickingMode.Ignore;

            _contentGroup = new VisualElement();
            _contentGroup.pickingMode = PickingMode.Ignore;
            _contentGroup.style.position = Position.Absolute;
            _contentGroup.style.alignItems = Align.Center;
            _contentGroup.style.justifyContent = Justify.Center;
            _contentGroup.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50), 0);
            Add(_contentGroup);

            _iconImage = new Image();
            _iconImage.pickingMode = PickingMode.Ignore;
            _iconImage.style.width = 110;
            _iconImage.style.height = 110;
            _iconImage.style.marginBottom = 2;
            _contentGroup.Add(_iconImage);

            _nameLabel = new Label();
            _nameLabel.pickingMode = PickingMode.Ignore;
            _nameLabel.style.fontSize = 17;
            _nameLabel.style.color = new Color(0.9f, 0.9f, 0.9f);
            _nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            var font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(f => f.name.Contains("Bender"));
            if (font != null) _nameLabel.style.unityFont = font;

            _contentGroup.Add(_nameLabel);
        }

        void OnGenerate(MeshGenerationContext ctx)
        {
            var p = ctx.painter2D;
            p.fillColor = Color;

            Vector2 center = contentRect.center;
            int steps = Mathf.CeilToInt((EndRad - StartRad) / (Mathf.PI / 40f));
            float step = (EndRad - StartRad) / steps;

            p.BeginPath();
            for (int i = 0; i <= steps; i++)
            {
                float a = StartRad + step * i;
                p.LineTo(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * OuterRadius);
            }
            for (int i = steps; i >= 0; i--)
            {
                float a = StartRad + step * i;
                p.LineTo(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * InnerRadius);
            }
            p.ClosePath();
            p.Fill();

            // Hover border highlight
            if (IsHovered)
            {
                p.lineWidth = 3.0f;
                float highlightRadius = OuterRadius + p.lineWidth * 0.5f;
                p.strokeColor = Color.white;
                p.BeginPath();
                for (int i = 0; i <= steps; i++)
                {
                    float a = StartRad + step * i;
                    p.LineTo(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * highlightRadius);
                }
                p.Stroke();
            }
        }

        public void SetContents(RadialMenuOption item)
        {
            _nameLabel.text = item.Name.ToUpper();

            if (item.Icon != null)
            {
                _iconImage.style.display = DisplayStyle.Flex;
                item.Icon.ApplyTo(_iconImage);
            }
            else
            {
                _iconImage.style.display = DisplayStyle.None;
            }

            _contentGroup.style.width = 150;
            _contentGroup.style.height = 130;

            RegisterCallback<GeometryChangedEvent>(evt => PositionContent());
        }

        void PositionContent()
        {
            float midRad = (StartRad + EndRad) * 0.5f;
            float midRadius = Mathf.Lerp(InnerRadius, OuterRadius, 0.5f);

            Vector2 center = contentRect.center;
            _contentGroup.style.left = center.x + Mathf.Cos(midRad) * midRadius;
            _contentGroup.style.top = center.y + Mathf.Sin(midRad) * midRadius;
        }
    }

    public class RadialMenu : VisualElement
    {
        private RadialMenuOption[] _items;
        private List<RadialMenuSlice> _slices = new();
        private VisualElement _sliceContainer, _overlayContainer, _centerBg, _separator;
        private Label _centerName, _centerDesc;
        private int _hoverIndex = -1;

        public float InnerRadius = 150f;
        public float OuterRadius = 340f;

        public event Action<int> OnItemSelected;
        public event Action OnClickOutside;

        public void SetVisible(bool visible)
        {
            Plugin.LogSource.LogInfo($"SETVISIBLE CALLED, state {visible}");
            style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (visible)
            {
                SetHoverIndex(-1);
            }
        }

        public void ClearSubscribers()
        {
            OnItemSelected = null;
            OnClickOutside = null;
        }

        public RadialMenu()
        {
            //start hidden
            style.display = DisplayStyle.None;

            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;

            var menuContainer = new VisualElement();
            float diameter = OuterRadius * 2f + 40f;

            //set parent size to ensue full click detection coverage
            style.width = 1920;
            style.height = 1080;

            menuContainer.style.width = diameter;
            menuContainer.style.height = diameter;

            menuContainer.style.position = Position.Absolute;
            menuContainer.style.left = Length.Percent(50);
            menuContainer.style.top = Length.Percent(50);
            menuContainer.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50), 0);
            menuContainer.pickingMode = PickingMode.Ignore;
            Add(menuContainer);

            _sliceContainer = new VisualElement();
            _sliceContainer.StretchToParentSize();
            _sliceContainer.pickingMode = PickingMode.Ignore;
            menuContainer.Add(_sliceContainer);

            _overlayContainer = new VisualElement();
            _overlayContainer.StretchToParentSize();
            _overlayContainer.pickingMode = PickingMode.Ignore;
            _overlayContainer.style.justifyContent = Justify.Center;
            _overlayContainer.style.alignItems = Align.Center;
            menuContainer.Add(_overlayContainer);

            _centerBg = new VisualElement();
            _centerBg.pickingMode = PickingMode.Ignore;
            _centerBg.style.backgroundColor = new Color(0.02f, 0.02f, 0.02f, 0.9f);
            float size = InnerRadius * 2f;
            _centerBg.style.width = _centerBg.style.height = size;
            _centerBg.style.borderTopLeftRadius = _centerBg.style.borderTopRightRadius =
            _centerBg.style.borderBottomLeftRadius = _centerBg.style.borderBottomRightRadius = InnerRadius;
            _centerBg.style.borderLeftColor = _centerBg.style.borderRightColor =
            _centerBg.style.borderTopColor = _centerBg.style.borderBottomColor = new Color(1, 1, 1, 0.15f);
            _centerBg.style.borderLeftWidth = _centerBg.style.borderRightWidth =
            _centerBg.style.borderTopWidth = _centerBg.style.borderBottomWidth = 2;
            _centerBg.style.justifyContent = Justify.Center;
            _centerBg.style.alignItems = Align.Center;
            _overlayContainer.Add(_centerBg);

            var textGroup = new VisualElement();
            textGroup.pickingMode = PickingMode.Ignore;
            textGroup.style.alignItems = Align.Center;
            textGroup.style.justifyContent = Justify.Center;
            textGroup.style.height = 100;
            _centerBg.Add(textGroup);

            var font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(f => f.name.Contains("Bender"));
            if (font == null)
            {
                Plugin.LogSource.LogError("Could not get EFT Font");
            }


            _centerName = new Label();
            _centerName.style.fontSize = 22;
            _centerName.style.color = Color.white;
            _centerName.style.unityFontStyleAndWeight = FontStyle.Bold;
            _centerName.style.unityFont = font;
            textGroup.Add(_centerName);

            _separator = new VisualElement();
            _separator.style.height = 2;
            _separator.style.width = 40;
            _separator.style.backgroundColor = new Color(1, 1, 1, 0.4f);
            _separator.style.marginTop = _separator.style.marginBottom = 4;
            textGroup.Add(_separator);

            _centerDesc = new Label();
            _centerDesc.style.fontSize = 14;
            _centerDesc.style.color = new Color(1, 1, 1, 0.7f);
            _centerDesc.style.unityTextAlign = TextAnchor.UpperCenter;
            _centerDesc.style.whiteSpace = WhiteSpace.Normal;
            _centerDesc.style.maxWidth = size * 0.8f;
            _centerDesc.style.height = 40;
            _centerDesc.style.unityFont = font;
            textGroup.Add(_centerDesc);

            textGroup.style.transitionProperty = new List<StylePropertyName> { "opacity" };
            textGroup.style.transitionDuration = new List<TimeValue> { new TimeValue(0.05f, TimeUnit.Second) };
            textGroup.style.opacity = 0;

            RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button != 0) return;
                int idx = GetSliceIndex(this.WorldToLocal(e.mousePosition));
                if (idx != -1)
                {
                    _items[idx].Callback?.Invoke();
                    OnItemSelected?.Invoke(idx);
                }
                else
                {
                    OnClickOutside?.Invoke();
                }
            });
            RegisterCallback<MouseMoveEvent>(e => SetHoverIndex(GetSliceIndex(this.WorldToLocal(e.mousePosition))));
            RegisterCallback<MouseLeaveEvent>(_ => SetHoverIndex(-1));
        }

        public void SetItems(RadialMenuOption[] items)
        {
            Plugin.LogSource.LogInfo("RadialMenu populating items");
            _items = items;
            _sliceContainer.Clear();
            _slices.Clear();

            if (_items == null || _items.Length == 0) return;
            float sliceRad = 2f * Mathf.PI / _items.Length;

            for (int i = 0; i < _items.Length; i++)
            {
                var slice = new RadialMenuSlice
                {
                    StartRad = i * sliceRad,
                    EndRad = (i + 1) * sliceRad,
                    InnerRadius = InnerRadius,
                    OuterRadius = OuterRadius,
                    Color = new Color(0f, 0f, 0f, 0.35f)
                };
                slice.SetContents(_items[i]);
                slice.StretchToParentSize();
                _sliceContainer.Add(slice);
                _slices.Add(slice);
            }

            Plugin.LogSource.LogInfo("RadialMenu population complete items");

        }

        public void SetHoverIndex(int idx)
        {
            if (_hoverIndex == idx) return;
            _hoverIndex = idx;

            for (int i = 0; i < _slices.Count; i++)
            {
                bool hovered = i == idx;
                _slices[i].Color = hovered ? new Color(0.01f, 0.01f, 0.01f, 0.7f) : new Color(0f, 0f, 0f, 0.35f);
                _slices[i].IsHovered = hovered;
                _slices[i].MarkDirtyRepaint();
            }

            if (idx >= 0 && idx < _items.Length)
            {
                _centerName.text = _items[idx].Name.ToUpper();
                _centerDesc.text = _items[idx].Description;
                _separator.style.opacity = string.IsNullOrEmpty(_items[idx].Description) ? 0 : 1;
                _centerName.parent.style.opacity = 1f;
            }
            else
            {
                _centerName.parent.style.opacity = 0f;
            }
        }

        private int GetSliceIndex(Vector2 localPos)
        {
            Vector2 center = contentRect.center;
            Vector2 dir = localPos - center;
            float dist = dir.magnitude;
            if (dist < InnerRadius || dist > OuterRadius) return -1;
            float angle = Mathf.Atan2(dir.y, dir.x);
            if (angle < 0) angle += 2f * Mathf.PI;
            return Mathf.FloorToInt(angle / (2f * Mathf.PI / _items.Length)) % _items.Length;
        }
    }
}