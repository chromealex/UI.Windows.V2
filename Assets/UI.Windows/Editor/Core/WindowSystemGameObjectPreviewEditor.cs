using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.UI.Windows {

    [CustomPreview(typeof(GameObject))]
    public class WindowSystemGameObjectPreviewEditor : ObjectPreview {

        private Editor editor;

        private void ValidateEditor() {

            if (this.editor == null) {
                
                var targetGameObject = this.target as GameObject;
                if (targetGameObject == null) return;
            
                var hasPreview = targetGameObject.GetComponent<UnityEngine.UI.Windows.IHasPreview>();
                if (hasPreview != null) {

                    this.editor = Editor.CreateEditor((Object)hasPreview);

                }

            }
            
        }
        
        public override GUIContent GetPreviewTitle() {

            this.ValidateEditor();

            if (this.editor != null) {

                return this.editor.GetPreviewTitle();

            }
            
            return base.GetPreviewTitle();
            
        }

        public override bool HasPreviewGUI() {
            
            this.ValidateEditor();

            return this.editor != null;

        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) {
            
            if (this.editor != null) this.editor.OnInteractivePreviewGUI(r, background);
            
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background) {
            
            if (this.editor != null) this.editor.OnPreviewGUI(r, background);
            
        }

    }

}
