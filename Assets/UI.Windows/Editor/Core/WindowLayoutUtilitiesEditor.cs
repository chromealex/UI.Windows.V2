using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    
    public class WindowLayoutUtilities {

        private struct Item {

            public string name;
            public float value;

        }
        
        public static void DrawLayout(ref int selectedIndexAspect, ref Vector2 tabsScrollPosition, WindowLayout windowLayout, Rect r) {

            var offset = 20f;
            var aspect = 4f / 3f;
            if (Selection.objects.Length == 1) {
             
                var items = new Item[] {
                    new Item() { name = "4:3", value = 4f / 3f },
                    new Item() { name = "16:9", value = 16f / 9f },
                    new Item() { name = "16:10", value = 16f / 10f },
                    new Item() { name = "5:4", value = 5f / 4f },
                    new Item() { name = "2:1", value = 2f / 1f },

                    new Item() { name = "3:4", value = 3f / 4f },
                    new Item() { name = "9:16", value = 9f / 16f },
                    new Item() { name = "10:16", value = 10f / 16f },
                    new Item() { name = "4:5", value = 4f / 5f },
                    new Item() { name = "1:2", value = 1f / 2f },
                };

                var tabs = items.Select(x => new GUITab(x.name, null)).ToArray();
                selectedIndexAspect = GUILayoutExt.DrawTabs(selectedIndexAspect, ref tabsScrollPosition, tabs);

                //var variants = items.Select(x => x.name).ToArray();
                //selectedIndexAspect = GUI.SelectionGrid(r, selectedIndexAspect, variants, variants.Length);
                aspect = items[selectedIndexAspect].value;

            } else {

                offset = 0f;

            }

            var used = new HashSet<WindowLayout>();
            WindowLayoutUtilities.DrawLayout(aspect, windowLayout, r, offset, used: used);
            
        }
        
        public static bool DrawLayout(float aspect, WindowLayout windowLayout, Rect r, float offset = 20f, HashSet<WindowLayout> used = null) {

            if (used.Contains(windowLayout) == true) return false;
            used.Add(windowLayout);

            var rSource = r;
            
            var rectOffset = r;
            if (offset > 0f) {
            
                rectOffset.x += offset;
                rectOffset.y += offset;
                rectOffset.height -= offset * 2f;
                rectOffset.width -= offset * 2f;

                var tWidth = rectOffset.height * aspect;
                if (tWidth > rectOffset.width) {
            
                    rectOffset.y += rectOffset.height * 0.5f;
                    rectOffset.height = rectOffset.width / aspect;
                    rectOffset.y -= rectOffset.height * 0.5f;

                } else {

                    rectOffset.x += rectOffset.width * 0.5f;
                    rectOffset.width = rectOffset.height * aspect;
                    rectOffset.x -= rectOffset.width * 0.5f;

                }

            } else {
            
                GUILayoutExt.DrawRect(rectOffset, new Color(0f, 0f, 0f, 0.4f));

            }
            
            GUILayoutExt.DrawRect(rectOffset, new Color(0f, 0f, 0f, 0.2f));
            GUILayoutExt.DrawBoxNotFilled(rectOffset, 1f, new Color(0.7f, 0.7f, 0.3f, 0.5f));

            GUI.BeginClip(r);

            var resolution = windowLayout.canvasScaler.referenceResolution;
            /*windowLayout.rectTransform.anchoredPosition = new Vector2(r.x, r.y);
            windowLayout.rectTransform.sizeDelta = new Vector2(r.width, r.height);
            windowLayout.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            windowLayout.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            windowLayout.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            windowLayout.rectTransform.localRotation = Quaternion.identity;
            windowLayout.rectTransform.localScale = Vector3.one;*/

            r = rectOffset;
            
            {

                if (r.width > 0f && r.height > 0f) {
                    
                    Vector2 screenSize = new Vector2(r.width, r.height);

                    var sizeDelta = Vector2.zero;
                    float scaleFactor = 0;
                    switch (windowLayout.canvasScaler.screenMatchMode)
                    {
                        case UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                        {
                            const float kLogBase = 2;
                            // We take the log of the relative width and height before taking the average.
                            // Then we transform it back in the original space.
                            // the reason to transform in and out of logarithmic space is to have better behavior.
                            // If one axis has twice resolution and the other has half, it should even out if widthOrHeight value is at 0.5.
                            // In normal space the average would be (0.5 + 2) / 2 = 1.25
                            // In logarithmic space the average is (-1 + 1) / 2 = 0
                            float logWidth = Mathf.Log(screenSize.x / windowLayout.canvasScaler.referenceResolution.x, kLogBase);
                            float logHeight = Mathf.Log(screenSize.y / windowLayout.canvasScaler.referenceResolution.y, kLogBase);
                            float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, windowLayout.canvasScaler.matchWidthOrHeight);
                            scaleFactor = Mathf.Pow(kLogBase, logWeightedAverage);
                            break;
                        }
                        case UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand:
                        {
                            scaleFactor = Mathf.Min(screenSize.x / windowLayout.canvasScaler.referenceResolution.x, screenSize.y / windowLayout.canvasScaler.referenceResolution.y);
                            break;
                        }
                        case UnityEngine.UI.CanvasScaler.ScreenMatchMode.Shrink:
                        {
                            scaleFactor = Mathf.Max(screenSize.x / windowLayout.canvasScaler.referenceResolution.x, screenSize.y / windowLayout.canvasScaler.referenceResolution.y);
                            break;
                        }
                    }

                    if (scaleFactor > 0f) {

                        sizeDelta = new Vector2(screenSize.x / scaleFactor, screenSize.y / scaleFactor);
                        windowLayout.rectTransform.sizeDelta = sizeDelta;
                        windowLayout.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        windowLayout.rectTransform.localScale = Vector3.one;
                        resolution = windowLayout.rectTransform.sizeDelta;

                    }

                }

            }
            
            var labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.alignment = TextAnchor.LowerLeft;

            var isHighlighted = false;
            var highlightedIndex = -1;
            var highlightedRect = Rect.zero;
            for (int i = 0; i < windowLayout.layoutElements.Length; ++i) {

                var element = windowLayout.layoutElements[i];
                if (element == null) {

                    windowLayout.ValidateEditor();
                    return false;

                }
                
                var rect = WindowLayoutUtilities.GetRect(windowLayout.rectTransform, element.rectTransform, r, resolution, offset > 0f);
                if (rect.Contains(Event.current.mousePosition) == true) {
                    
                    if (highlightedIndex >= 0 && highlightedRect.width * highlightedRect.height < rect.width * rect.height) {

                        continue;
                        
                    }
                    
                    highlightedIndex = i;
                    highlightedRect = rect;
                    isHighlighted = true;

                }

            }

            var hasInnerHighlight = false;
            for (int i = 0; i < windowLayout.layoutElements.Length; ++i) {

                var element = windowLayout.layoutElements[i];
                var rect = WindowLayoutUtilities.GetRect(windowLayout.rectTransform, element.rectTransform, r, resolution, offset > 0f);

                using (new GUILayoutExt.GUIColorUsing(highlightedIndex < 0 || i == highlightedIndex ? Color.white : new Color(1f, 1f, 1f, 0.6f))) {
                    WindowSystemSidePropertyDrawer.DrawLayoutMode(rect, element.rectTransform);
                }

                if (element.innerLayout != null) {

                    hasInnerHighlight = WindowLayoutUtilities.DrawLayout(aspect, element.innerLayout, rect, offset: 0f, used: used);
                    //WindowLayoutUtilities.DrawLayoutElements(highlightedIndex, rect, resolution, element.innerLayout, used);

                }

            }

            if (highlightedIndex >= 0 && hasInnerHighlight == false) {
                
                var element = windowLayout.layoutElements[highlightedIndex];
                var rect = highlightedRect;
                
                var padding = 6f;
                var color = new Color(1f, 1f, 0f, 0.5f);
                var content = new GUIContent(element.name);
                GUI.Label(new Rect(padding, 0f, rSource.width, rSource.height - padding), content, labelStyle);
                var labelWidth = labelStyle.CalcSize(content).x + 10f;
                GUILayoutExt.DrawRect(new Rect(padding, rSource.height - 1f - padding, labelWidth, 1f), color);
                var p1 = new Vector3(labelWidth + padding, rSource.height - 1f - padding);
                var p2 = new Vector3(rect.x, rect.y);
                Handles.color = color;
                Handles.DrawLine(p1, p2);
                    
                GUILayoutExt.DrawBoxNotFilled(rect, 1f, new Color(1f, 1f, 1f, 0.2f));

            }
            
            GUI.EndClip();

            return isHighlighted;

        }

        private static Rect GetRect(RectTransform root, RectTransform child, Rect r, Vector2 resolution, bool withOffset) {
            
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(root, child);
            var rect = Rect.MinMaxRect(bounds.min.x, resolution.y - bounds.max.y, bounds.max.x, resolution.y - bounds.min.y);
            rect = WindowLayoutUtilities.GetRectScaled(rect, resolution, new Vector2(r.width, r.height));
            rect.x += r.width * 0.5f;
            rect.y -= r.height * 0.5f;
            if (withOffset == true) rect.x += r.x;
            if (withOffset == true) rect.y += r.y - 22f;

            return rect;

        }

        private static Rect GetRectScaled(Rect rect, Vector2 sourceResolution, Vector2 targetResolution) {

            var scaleX = targetResolution.x / sourceResolution.x;
            var scaleY = targetResolution.y / sourceResolution.y;
            
            return new Rect(
                rect.x * scaleX,
                rect.y * scaleY,
                rect.width * scaleX,
                rect.height * scaleY);
            
        }

    }

}
