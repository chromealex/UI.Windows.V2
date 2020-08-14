﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEngine.UI.Windows.WindowTypes {

    using Modules;
    using Utilities;
    
    public class LayoutSelectorAttribute : PropertyAttribute {}
    
    [System.Serializable]
    public class LayoutItem {

        [System.Serializable]
        public struct LayoutComponentItem {

            public WindowLayout windowLayout;
            public int tag;
            public int localTag;
            [ResourceType(typeof(WindowComponent))]
            public Resource component;
            
            [System.NonSerialized]
            internal WindowComponent componentInstance;

        }

        [Tooltip("Current target filter for this layout. If no layout filtered - will be used layout with 0 index.")]
        public WindowSystemTargets targets;
        [LayoutSelector]
        public WindowLayout windowLayout;
        [SearchAssetsByTypePopup(typeof(WindowLayoutPreferences), menuName: "Layout Preferences")]
        public WindowLayoutPreferences layoutPreferences;
        public LayoutComponentItem[] components;

        internal WindowLayout windowLayoutInstance;
        private int localTag;

        public int GetCanvasOrder() {

            return this.windowLayoutInstance.GetCanvasOrder();

        }

        public void Validate() {

            this.localTag = 0;

        }

        public bool HasLocalTagId(int localTagId) {

            for (int i = 0; i < this.components.Length; ++i) {

                if (this.components[i].tag == localTagId) {

                    return true;

                }

            }

            return false;

        }

        public bool GetLayoutComponent<T>(out T component, int localTagId) where T : WindowComponent {

            for (int i = 0; i < this.components.Length; ++i) {

                var comp = this.components[i];
                if (comp.tag == localTagId) {

                    component = comp.componentInstance as T;
                    return true;

                }

            }

            component = default;
            return false;

        }

        public bool GetLayoutComponent<T>(out T component, ref int lastIndex, Algorithm algorithm) where T : WindowComponent {

            if (algorithm == Algorithm.GetFirstTypeAny || algorithm == Algorithm.GetNextTypeAny) {

                for (int i = 0; i < this.components.Length; ++i) {

                    var comp = this.components[i].componentInstance;
                    if (comp != null && comp is T c && lastIndex < i) {

                        lastIndex = i;
                        component = c;
                        return true;

                    }

                }

            } else if (algorithm == Algorithm.GetFirstTypeStrong || algorithm == Algorithm.GetNextTypeStrong) {

                var typeOf = typeof(T);
                for (int i = 0; i < this.components.Length; ++i) {

                    var comp = this.components[i].componentInstance;
                    if (comp != null && comp.GetType() == typeOf && lastIndex < i) {

                        lastIndex = i;
                        component = (T)comp;
                        return true;

                    }

                }

            }
            
            component = default;
            return false;

        }

        public void Unload(LayoutWindowType windowInstance) {

            var resources = WindowSystem.GetResources();
            resources.DeleteAll(windowInstance);

        }

        public void LoadAsync(LayoutWindowType windowInstance, System.Action onComplete) {

            windowInstance.Setup(windowInstance);
            
            var used = new HashSet<WindowLayout>();
            var layoutItem = this;
            Coroutines.Run(layoutItem.InitLayoutInstance(windowInstance, windowInstance, layoutItem.windowLayout, used, onComplete));

        }

        public void ApplyLayoutPreferences(WindowLayoutPreferences layoutPreferences) {

            if (layoutPreferences != null) layoutPreferences.Apply(this.windowLayoutInstance.canvasScaler);

        }

        private IEnumerator InitLayoutInstance(LayoutWindowType windowInstance, WindowObject root, WindowLayout windowLayout, HashSet<WindowLayout> used, System.Action onComplete, bool isInner = false) {

            if (((ILayoutInstance)root).windowLayoutInstance != null) {
                
                if (onComplete != null) onComplete.Invoke();
                yield break;
                
            }
            
            if (windowLayout.createPool == true) WindowSystem.GetPools().CreatePool(windowLayout);
            var windowLayoutInstance = WindowSystem.GetPools().Spawn(windowLayout, root.transform);
            
            if (isInner == true) {

                windowLayoutInstance.canvasScaler.enabled = false;

            }

            windowLayoutInstance.Setup(windowInstance);
            windowLayoutInstance.SetCanvasOrder(0);
            root.RegisterSubObject(windowLayoutInstance);
            ((ILayoutInstance)root).windowLayoutInstance = windowLayoutInstance;
            this.ApplyLayoutPreferences(this.layoutPreferences);

            windowLayoutInstance.SetTransformFullRect();
            
            used.Add(this.windowLayout);
            var arr = this.components;
            for (int i = 0; i < arr.Length; ++i) {

                var layoutComponent = arr[i];
                if (layoutComponent.windowLayout != windowLayout) continue;

                var layoutElement = windowLayoutInstance.GetLayoutElementByTagId(layoutComponent.tag);
                if (layoutComponent.componentInstance == null) {

                    layoutElement.Setup(windowInstance);

                    if (layoutComponent.component.IsEmpty() == false) {

                        var index = i;
                        var resources = WindowSystem.GetResources();
                        var loaded = false;
                        yield return resources.LoadAsync<WindowComponent>(windowInstance, layoutComponent.component, (asset) => {

                            if (asset == null) {

                                Debug.LogWarning("Component is null while component resource not empty. Skipped.");
                                return;

                            }

                            var instance = layoutElement.Load(asset);
                            instance.SetInvisible();
                            layoutComponent.componentInstance = instance;
                            arr[index] = layoutComponent;

                            instance.DoLoadScreenAsync(() => { loaded = true; });

                        });

                        while (loaded == false) yield return null;

                    }

                    arr[i] = layoutComponent;

                }

                if (layoutElement.innerLayout != null) {

                    if (used.Contains(layoutElement.innerLayout) == false) {

                        yield return this.InitLayoutInstance(windowInstance, layoutElement, layoutElement.innerLayout, used, null, isInner: true);

                    } else {

                        Debug.LogWarning("Ignoring inner layout because of a cycle");

                    }

                }

            }

            if (onComplete != null) onComplete.Invoke();

        }

        public void Add(int tag, WindowLayout windowLayout) {

            for (int i = 0; i < this.components.Length; ++i) {

                if (this.components[i].tag == tag && this.components[i].windowLayout == windowLayout) {

                    return;

                }

            }

            var list = this.components.ToList();
            list.Add(new LayoutComponentItem() {
                component = new Resource(),
                componentInstance = null,
                tag = tag,
                localTag = ++this.localTag,
                windowLayout = windowLayout,
            });
            this.components = list.ToArray();

        }

        public void Remove(int tag, WindowLayout windowLayout) {

            for (int i = 0; i < this.components.Length; ++i) {

                if (this.components[i].tag == tag && this.components[i].windowLayout == windowLayout) {

                    var list = this.components.ToList();
                    list.RemoveAt(i);
                    this.components = list.ToArray();

                }

            }

        }

        public bool GetLayoutComponentItemByTagId(int tagId, WindowLayout windowLayout, out LayoutComponentItem layoutComponentItem) {

            for (int i = 0; i < this.components.Length; ++i) {

                if (this.components[i].tag == tagId && this.components[i].windowLayout == windowLayout) {

                    layoutComponentItem = this.components[i];
                    return true;

                }

            }

            layoutComponentItem = default;
            return false;

        }

    }

    [System.Serializable]
    public struct Layouts {

        public LayoutItem[] items;

        private int activeIndex;

        public void SetActive() {

            var targetData = WindowSystem.GetTargetData();
            for (int i = this.items.Length - 1; i >= 0; --i) {

                if (this.items[i].targets.IsValid(targetData) == true) {

                    this.activeIndex = i;
                    return;

                }

            }

            this.activeIndex = 0;

        }

        public LayoutItem GetActive() {

            return this.items[this.activeIndex];

        }

    }

    public enum Algorithm {

        /// <summary>
        /// Returns next component derived from type T or of type T
        /// </summary>
        GetNextTypeAny,
        /// <summary>
        /// Returns next component strongly of type T
        /// </summary>
        GetNextTypeStrong,
        /// <summary>
        /// Returns first component derived from type T or of type T
        /// </summary>
        GetFirstTypeAny,
        /// <summary>
        /// Returns first component strongly of type T
        /// </summary>
        GetFirstTypeStrong,

    }
    
    public abstract class LayoutWindowType : WindowBase, ILayoutInstance {

        public Layouts layouts = new Layouts() {
            items = new LayoutItem[1]
        };

        private Dictionary<int, int> requestedIndexes = new Dictionary<int, int>();

        WindowLayout ILayoutInstance.windowLayoutInstance {
            get {
                return this.layouts.GetActive().windowLayoutInstance; }
            set {
                this.layouts.GetActive().windowLayoutInstance = value;
            }
        }

        public override int GetCanvasOrder() {

            return this.layouts.GetActive().GetCanvasOrder();

        }

        public bool GetLayoutComponent<T>(out T component, int localTagId) where T : WindowComponent {

            var currentItem = this.layouts.GetActive();
            return currentItem.GetLayoutComponent(out component, localTagId);

        }

        /// <summary>
        /// Returns component instance of type T
        /// </summary>
        /// <param name="component">Component instance</param>
        /// <param name="algorithm">Algorithm which will be used to return component</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Return true if component found, otherwise false</returns>
        public bool GetLayoutComponent<T>(out T component, Algorithm algorithm = Algorithm.GetNextTypeAny) where T : WindowComponent {

            component = default;
            bool result = false;
            switch (algorithm) {

                case Algorithm.GetNextTypeAny:
                case Algorithm.GetNextTypeStrong: {
                    
                    var key = typeof(T).GetHashCode();
                    var addNew = false;
                    var currentItem = this.layouts.GetActive();
                    if (this.requestedIndexes.TryGetValue(key, out var lastIndex) == false) {

                        addNew = true;
                        lastIndex = -1;

                    }

                    result = currentItem.GetLayoutComponent(out component, ref lastIndex, algorithm);

                    if (addNew == true) {

                        this.requestedIndexes.Add(key, lastIndex);

                    } else {

                        this.requestedIndexes[key] = lastIndex;

                    }

                }
                    break;

                case Algorithm.GetFirstTypeAny:
                case Algorithm.GetFirstTypeStrong: {
                    var idx = 0;
                    var currentItem = this.layouts.GetActive();
                    result = currentItem.GetLayoutComponent(out component, ref idx, algorithm);
                }
                    break;
                
            }
            
            return result;

        }

        public override void OnDeInit() {

            base.OnDeInit();

            var currentItem = this.layouts.GetActive();
            currentItem.Unload(this);

        }

        public override void LoadAsync(System.Action onComplete) {

            this.layouts.SetActive();

            var currentItem = this.layouts.GetActive();
            currentItem.LoadAsync(this, () => { base.LoadAsync(onComplete); });

        }

        public int GetNextTagId(LayoutItem layoutItem) {

            var tagId = 1;
            while (layoutItem.components.Any(x => x.tag == tagId)) {

                ++tagId;

            }

            return tagId;

        }

        public int GetNextLocalTagId(LayoutItem layoutItem) {

            var tagId = 1;
            while (layoutItem.components.Any(x => x.localTag == tagId)) {

                ++tagId;

            }

            return tagId;

        }

        private void ValidateLayout(ref LayoutItem layoutItem, WindowLayout windowLayout, HashSet<WindowLayout> used) {

            used.Add(windowLayout);

            // Validate tags
            for (int j = layoutItem.components.Length - 1; j >= 0; --j) {

                var com = layoutItem.components[j];
                if (com.localTag == 0 || layoutItem.components.Count(x => x.localTag == com.localTag) > 1) {

                    com.localTag = this.GetNextLocalTagId(layoutItem);

                }

                /*if (layoutItem.components.Count(x => x.tag == com.tag && x.windowLayout == com.windowLayout) > 1) {

                    com.tag = 0;

                }*/

                layoutItem.components[j] = com;

            }

            // Remove unused
            {

                for (int j = 0; j < layoutItem.components.Length; ++j) {

                    var tag = layoutItem.components[j].tag;
                    if (windowLayout.HasLayoutElementByTagId(tag) == false) {

                        layoutItem.Remove(tag, windowLayout);

                    }

                }

            }

            for (int j = 0; j < windowLayout.layoutElements.Length; ++j) {

                var layoutElement = windowLayout.layoutElements[j];
                if (layoutItem.GetLayoutComponentItemByTagId(layoutElement.tagId, windowLayout, out var layoutComponentItem) == false) {

                    layoutItem.Add(layoutElement.tagId, windowLayout);

                }

                if (layoutElement.innerLayout != null) {

                    if (used.Contains(layoutElement.innerLayout) == false) {

                        this.ValidateLayout(ref layoutItem, layoutElement.innerLayout, used);

                    } else {

                        Debug.LogWarning("Ignoring inner layout `" + layoutElement.innerLayout + "` because of a cycle. Remove innerLayout reference from " + layoutElement,
                                         layoutElement);

                    }

                }

            }

        }

        public override void ValidateEditor() {

            base.ValidateEditor();

            var items = this.layouts.items;
            if (items == null) return;
            
            for (int i = 0; i < items.Length; ++i) {

                ref var layoutItem = ref items[i];
                layoutItem.Validate();

                var windowLayout = layoutItem.windowLayout;
                if (windowLayout != null) {

                    { // Validate components list

                        for (int c = 0; c < layoutItem.components.Length; ++c) {

                            var com = layoutItem.components[c];
                            if ((windowLayout != com.windowLayout || windowLayout.HasLayoutElementByTagId(com.tag) == false) && windowLayout.layoutElements.Any(x => {
                                return x.innerLayout == com.windowLayout && x.innerLayout.HasLayoutElementByTagId(com.tag);
                            }) == false) {

                                var list = layoutItem.components.ToList();
                                list.RemoveAt(c);
                                layoutItem.components = list.ToArray();
                                --c;

                            }

                        }

                    }

                    var used = new HashSet<WindowLayout>();
                    this.ValidateLayout(ref layoutItem, windowLayout, used);

                    used.Clear();
                    this.ValidateLayout(ref layoutItem, windowLayout, used);

                }

            }

            this.layouts.items = items;

        }

    }

}