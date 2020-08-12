using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(UnityEngine.UI.CanvasScaler))]
    public class WindowLayout : WindowObject {

        public Canvas canvas;
        public UnityEngine.UI.CanvasScaler canvasScaler;

        public WindowLayoutElement[] layoutElements;

        private int order;

        public void SetCanvasOrder(int order) {

            this.order = order;
            this.canvas.sortingOrder = order;

        }

        public int GetCanvasOrder() {

            return this.order;

        }

        internal override void Setup(WindowBase source) {

            base.Setup(source);

            this.ApplyRenderMode();

            this.canvas.worldCamera = source.workCamera;

            if (this.canvas.isRootCanvas == false) {

                this.canvasScaler.enabled = false;

            }

        }

        internal void ApplyRenderMode() {

            switch (this.window.preferences.renderMode) {

                case UIWSRenderMode.UseSettings:
                    this.canvas.renderMode = WindowSystem.GetSettings().canvas.renderMode;
                    break;

                case UIWSRenderMode.WorldSpace:
                    this.canvas.renderMode = RenderMode.WorldSpace;
                    break;

                case UIWSRenderMode.ScreenSpaceCamera:
                    this.canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    break;

                case UIWSRenderMode.ScreenSpaceOverlay:
                    this.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    break;

            }

        }

        public override void ValidateEditor() {

            base.ValidateEditor();

            this.canvas = this.GetComponent<Canvas>();
            this.canvasScaler = this.GetComponent<UnityEngine.UI.CanvasScaler>();
            var prevElements = this.layoutElements;
            this.layoutElements = this.GetComponentsInChildren<WindowLayoutElement>(true);

            this.canvas.renderMode = WindowSystem.GetSettings().canvas.renderMode;

            this.ApplyTags(prevElements);
            
        }

        private void ApplyTags(WindowLayoutElement[] prevElements) {

            foreach (var element in this.layoutElements) {

                if (element.tagId != 0) {

                    if (this.layoutElements.Count(x => x.tagId == element.tagId) > 1 && prevElements.Contains(element) == false) {

                        element.tagId = 0;

                    }
                    
                }
                
            }

            var localTagId = 0;
            foreach (var element in this.layoutElements) {

                element.windowId = this.windowId;
                if (element.tagId == 0) {

                    var reqId = ++localTagId;
                    while (this.layoutElements.Any(x => x.tagId == reqId)) {

                        ++reqId;

                    }

                    element.tagId = ++localTagId;

                } else {

                    
                    localTagId = element.tagId;

                }

            }

        }

        public WindowLayoutElement GetLayoutElementByTagId(int tagId) {

            for (int i = 0; i < this.layoutElements.Length; ++i) {

                if (this.layoutElements[i].tagId == tagId) {

                    return this.layoutElements[i];

                }

            }

            return default;

        }

        public bool HasLayoutElementByTagId(int tagId) {

            return this.GetLayoutElementByTagId(tagId) != null;

        }

    }

}