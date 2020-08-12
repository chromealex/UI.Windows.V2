using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {
    
    [ComponentModuleDisplayName("Scrollable Fades")]
    public class ListScrollableComponentModule : ListComponentModule {
        
        [Space(10f)]
        public ScrollRect scrollRect;

        [Space(10f)]
        public WindowComponent fadeBottom;
        public WindowComponent fadeTop;
        public WindowComponent fadeRight;
        public WindowComponent fadeLeft;

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.fadeBottom != null) {

                this.fadeBottom.ValidateEditor(updateParentObjects: false);
                this.fadeBottom.hiddenByDefault = true;
                this.fadeBottom.AddEditorParametersRegistry(new WindowObject.EditorParametersRegistry() {
                    holder = this.windowComponent,
                    hiddenByDefault = true, hiddenByDefaultDescription = "Value is hold by ListScrollableComponentModule"
                });

            }
            
            if (this.fadeTop != null) {
                
                this.fadeTop.ValidateEditor(updateParentObjects: false);
                this.fadeTop.hiddenByDefault = true;
                this.fadeTop.AddEditorParametersRegistry(new WindowObject.EditorParametersRegistry() {
                    holder = this.windowComponent,
                    hiddenByDefault = true, hiddenByDefaultDescription = "Value is hold by ListScrollableComponentModule"
                });
                
            }

            if (this.fadeRight != null) {
                
                this.fadeRight.ValidateEditor(updateParentObjects: false);
                this.fadeRight.hiddenByDefault = true;
                this.fadeRight.AddEditorParametersRegistry(new WindowObject.EditorParametersRegistry() {
                    holder = this.windowComponent,
                    hiddenByDefault = true, hiddenByDefaultDescription = "Value is hold by ListScrollableComponentModule"
                });
                
            }

            if (this.fadeLeft != null) {
                
                this.fadeLeft.ValidateEditor(updateParentObjects: false);
                this.fadeLeft.hiddenByDefault = true;
                this.fadeLeft.AddEditorParametersRegistry(new WindowObject.EditorParametersRegistry() {
                    holder = this.windowComponent,
                    hiddenByDefault = true, hiddenByDefaultDescription = "Value is hold by ListScrollableComponentModule"
                });
                
            }

            if (this.scrollRect == null) {

                this.scrollRect = this.GetComponentInChildren<ScrollRect>(true);

            } 

        }

        public override void OnInit() {
            
            base.OnInit();
            
            if (this.scrollRect != null) this.scrollRect.onValueChanged.AddListener(this.OnScrollValueChanged);
            
        }

        public override void OnDeInit() {
            
            if (this.scrollRect != null) this.scrollRect.onValueChanged.RemoveListener(this.OnScrollValueChanged);

            base.OnDeInit();
            
        }

        public override void OnElementsChanged() {
            
            base.OnElementsChanged();
            
            if (this.scrollRect != null) this.OnScrollValueChanged(this.scrollRect.normalizedPosition);

        }

        /*
        public void LateUpdate() {

            if (this.windowComponent.IsVisible() == true) {

                if (this.scrollRect != null) this.OnScrollValueChanged(this.scrollRect.normalizedPosition);

            }

        }*/

        private void OnScrollValueChanged(Vector2 position) {

            var contentRect = this.scrollRect.content.rect;
            var borderRect = (this.transform as RectTransform).rect;

            {
                var contentHeight = contentRect.height;
                var borderHeight = borderRect.height;
                var sizeY = borderHeight - contentHeight;
                if (sizeY >= 0f || this.scrollRect.vertical == false) {

                    if (this.fadeTop != null) this.fadeTop.Hide();
                    if (this.fadeBottom != null) this.fadeBottom.Hide();

                } else {

                    if (position.y <= 0.01f) {

                        if (this.fadeBottom != null) this.fadeBottom.Hide();

                    } else {

                        if (this.fadeBottom != null) this.fadeBottom.Show();

                    }

                    if (position.y >= 0.99f) {

                        if (this.fadeTop != null) this.fadeTop.Hide();

                    } else {

                        if (this.fadeTop != null) this.fadeTop.Show();

                    }

                }
            }
            {
                var contentWidth = contentRect.width;
                var borderWidth = borderRect.width;
                var sizeX = borderWidth - contentWidth;
                if (sizeX >= 0f || this.scrollRect.horizontal == false) {

                    if (this.fadeLeft != null) this.fadeLeft.Hide();
                    if (this.fadeRight != null) this.fadeRight.Hide();

                } else {

                    if (position.x <= 0.01f) {

                        if (this.fadeLeft != null) this.fadeLeft.Hide();

                    } else {

                        if (this.fadeLeft != null) this.fadeLeft.Show();

                    }

                    if (position.x >= 0.99f) {

                        if (this.fadeRight != null) this.fadeRight.Hide();

                    } else {

                        if (this.fadeRight != null) this.fadeRight.Show();

                    }

                }
            }

        }

    }

}
