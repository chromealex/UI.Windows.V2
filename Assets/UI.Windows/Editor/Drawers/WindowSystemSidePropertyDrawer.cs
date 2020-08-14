using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows.Utilities;

    internal class LayoutDropdownWindow : PopupWindowContent {

        public WindowSystemSidePropertyDrawer.Layout selectedX;
        public WindowSystemSidePropertyDrawer.Layout selectedY;
        public System.Action<WindowSystemSidePropertyDrawer.Layout, WindowSystemSidePropertyDrawer.Layout> callback;
        public bool drawMiddle;
        
        public override Vector2 GetWindowSize() {
            
            return new Vector2(140f, 140f);
            
        }

        public override void OnGUI(Rect rect) {

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return) {
            
                this.editorWindow.Close();
                
            }

            const float offset = 10f;
            const float size = 40f;

            for (int x = 0; x < 3; ++x) {

                for (int y = 0; y < 3; ++y) {

                    if (this.drawMiddle == false && x == 1 && y == 1) {
                        
                        continue;
                        
                    }
                    
                    var hMode = (WindowSystemSidePropertyDrawer.Layout)x;
                    var vMode = (WindowSystemSidePropertyDrawer.Layout)y;
                    
                    var buttonRect = rect;
                    buttonRect.x = offset + size * x;
                    buttonRect.y = offset + size * y;
                    buttonRect.width = size;
                    buttonRect.height = size;
                    if (WindowSystemSidePropertyDrawer.DrawButton(buttonRect, hMode, vMode) == true) {
                        
                        if (this.callback != null) this.callback.Invoke(hMode, vMode);
                        this.editorWindow.Close();
                        
                    }

                    if (hMode == this.selectedX && vMode == this.selectedY) {
                        
                        GUILayoutExt.DrawBoxNotFilled(buttonRect, 1f, Color.white);
                        
                    }
                    
                }
                
            }
            
        }

    }

    [CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Components.Side))]
    public class WindowSystemSidePropertyDrawer : PropertyDrawer {

        public enum Layout {

            Min = 0,
            Middle = 1,
            Max = 2,

        }
        static float[] kPivotsForModes = new float[] { 0f, 0.5f, 1f };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            
            return 40f;
            
        }

        public static bool DrawButton(Rect contentRect, Layout hMode, Layout vMode) {
            
            const float padding = 4f;
            const float size = 40f;

            var inner = contentRect;
            inner.x += padding;
            inner.width -= padding * 2f;
            inner.y += padding;
            inner.height -= padding * 2f;
            
            var style = new GUIStyle(EditorStyles.miniButton);
            style.stretchHeight = true;
            style.fixedHeight = 0f;
            style.normal.scaledBackgrounds[0] = null;
            style.onNormal.scaledBackgrounds[0] = Texture2D.whiteTexture;
            var oldColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, contentRect.Contains(Event.current.mousePosition) ? 0.3f : 0f);
            if (GUI.Button(contentRect, string.Empty, style) == true) {

                return true;

            }
            GUI.color = oldColor;

            GUILayoutExt.DrawBoxNotFilled(contentRect, 1f, new Color(1f, 1f, 1f, 0.3f), padding);
            GUILayoutExt.DrawBoxNotFilled(contentRect, 1f, new Color(1f, 1f, 1f, 0.5f), padding * 3f);

            var vLine = inner;
            vLine.y += (size - padding * 2f) * kPivotsForModes[(int)vMode] - 0.5f;
            vLine.height = 1f;
            vLine.x += 1f;
            vLine.width -= 2f;
            GUILayoutExt.DrawRect(vLine, new Color(0.7f, 0.3f, 0.3f, 1));

            var hLine = inner;
            hLine.x += (size - padding * 2f) * kPivotsForModes[(int)hMode] - 0.5f;
            hLine.width = 1f;
            hLine.y += 1f;
            hLine.height -= 2f;
            GUILayoutExt.DrawRect(hLine, new Color(0.7f, 0.3f, 0.3f, 1));
            
            var pivot = new Vector2(
                Mathf.Lerp(inner.xMin, inner.xMax, kPivotsForModes[(int)hMode]),
                Mathf.Lerp(inner.yMin, inner.yMax, kPivotsForModes[(int)vMode])
            );
            
            GUILayoutExt.DrawRect(new Rect(pivot.x - 1f, pivot.y - 1f, 3f, 3f), new Color(0.8f, 0.6f, 0.0f, 1));
            
            return false;

        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var labelRect = position;
            labelRect.width = EditorGUIUtility.labelWidth;
            GUI.Label(labelRect, label);

            const float size = 40f;

            var side = (UnityEngine.UI.Windows.Components.Side)property.enumValueIndex;
            this.GetMode(side, out var hMode, out var vMode);
            
            var contentRect = position;
            contentRect.x += labelRect.width;
            contentRect.width = size;

            if (DrawButton(contentRect, hMode, vMode) == true) {
                
                var win = new LayoutDropdownWindow();
                win.drawMiddle = false;
                win.callback = (h, v) => {
                    
                    var mode = this.GetMode(h, v);
                    property.serializedObject.Update();
                    property.enumValueIndex = (int)mode;
                    property.serializedObject.ApplyModifiedProperties();

                };
                win.selectedX = hMode;
                win.selectedY = vMode;
                PopupWindow.Show(contentRect, win);
                
            }

        }

        private void GetMode(UnityEngine.UI.Windows.Components.Side side, out Layout hMode, out Layout vMode) {

            hMode = Layout.Max;
            vMode = Layout.Max;
            
            switch (side) {
                case UnityEngine.UI.Windows.Components.Side.Bottom:
                    hMode = Layout.Middle;
                    vMode = Layout.Max;
                    break;
                case UnityEngine.UI.Windows.Components.Side.Left:
                    hMode = Layout.Min;
                    vMode = Layout.Middle;
                    break;
                case UnityEngine.UI.Windows.Components.Side.Right:
                    hMode = Layout.Max;
                    vMode = Layout.Middle;
                    break;
                case UnityEngine.UI.Windows.Components.Side.Top:
                    hMode = Layout.Middle;
                    vMode = Layout.Min;
                    break;
                case UnityEngine.UI.Windows.Components.Side.Middle:
                    hMode = Layout.Middle;
                    vMode = Layout.Middle;
                    break;
                case UnityEngine.UI.Windows.Components.Side.BottomLeft:
                    hMode = Layout.Min;
                    vMode = Layout.Max;
                    break;
                case UnityEngine.UI.Windows.Components.Side.BottomRight:
                    hMode = Layout.Max;
                    vMode = Layout.Max;
                    break;
                case UnityEngine.UI.Windows.Components.Side.TopLeft:
                    hMode = Layout.Min;
                    vMode = Layout.Min;
                    break;
                case UnityEngine.UI.Windows.Components.Side.TopRight:
                    hMode = Layout.Max;
                    vMode = Layout.Min;
                    break;
            }
            
        }

        private UnityEngine.UI.Windows.Components.Side GetMode(Layout hMode, Layout vMode) {

            UnityEngine.UI.Windows.Components.Side side = UnityEngine.UI.Windows.Components.Side.Bottom;
            switch (vMode) {
                
                case Layout.Min:
                    switch (hMode) {
                        
                        case Layout.Min:
                            side = UnityEngine.UI.Windows.Components.Side.TopLeft;
                            break;

                        case Layout.Middle:
                            side = UnityEngine.UI.Windows.Components.Side.Top;
                            break;

                        case Layout.Max:
                            side = UnityEngine.UI.Windows.Components.Side.TopRight;
                            break;

                    }
                    break;

                case Layout.Middle:
                    switch (hMode) {
                        
                        case Layout.Min:
                            side = UnityEngine.UI.Windows.Components.Side.Left;
                            break;

                        case Layout.Middle:
                            side = UnityEngine.UI.Windows.Components.Side.Middle;
                            break;

                        case Layout.Max:
                            side = UnityEngine.UI.Windows.Components.Side.Right;
                            break;

                    }
                    break;

                case Layout.Max:
                    switch (hMode) {
                        
                        case Layout.Min:
                            side = UnityEngine.UI.Windows.Components.Side.BottomLeft;
                            break;

                        case Layout.Middle:
                            side = UnityEngine.UI.Windows.Components.Side.Bottom;
                            break;

                        case Layout.Max:
                            side = UnityEngine.UI.Windows.Components.Side.BottomRight;
                            break;

                    }
                    break;
                
            }

            return side;

        }

    }

}