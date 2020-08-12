using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows.Utilities;

    [CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Components.DropdownComponent.Side))]
    public class WindowSystemSidePropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            //EditorGUI.DrawRect(position, Color.blue);
            var labelRect = position;
            //labelRect.
            //GUI.Label(labelRect);
            GUILayoutExt.DrawBoxNotFilled(position, 1f, Color.gray);
            
        }

    }

}