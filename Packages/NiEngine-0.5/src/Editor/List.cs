
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Nie.Editor
{


    class ListView2 : VisualElement
    {
        //public new class UxmlFactory : UxmlFactory<ListView2, UxmlTraits> { }

        //// Add the two custom UXML attributes.
        //public new class UxmlTraits : VisualElement.UxmlTraits
        //{
        //    UxmlStringAttributeDescription m_String =
        //        new UxmlStringAttributeDescription { name = "my-string", defaultValue = "default_value" };

        //    public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        //    {
        //        base.Init(ve, bag, cc);
        //        var ate = ve as List;

        //        ate.myString = m_String.GetValueFromBag(bag, cc);
        //    }
        //}

        //// Must expose your element class to a { get; set; } property that has the same name 
        //// as the name you set in your UXML attribute description with the camel case format
        //public string myString { get; set; }

        SerializedProperty Property;
        VisualElement VeContent;
        VisualElement VeFold;
        Toggle Toggle;
        Button BtAdd;
        Label LbText;
        Label LbCount;

        public class Item
        {
            public ListView2 List;
            public SerializedProperty ParentProperty;
            public SerializedProperty Property;
            public int Index;
            public VisualElement RootVisualElement;
            public PropertyField Content;

            public Item(ListView2 list, SerializedProperty parentProperty, int index)
            {
                List = list;
                ParentProperty = parentProperty;
                Index = index;
                Build();
            }
            void Build()
            {
                RootVisualElement = ReactionStateMachineEditor.ListItemAsset.CloneTree();
                var tfItemIndex = RootVisualElement.Query<TextField>("tfIndex").First();
                var btItemDelete = RootVisualElement.Query<Button>("btDelete").First();
                var veItemContent = RootVisualElement.Query<VisualElement>("veContent").First();
                tfItemIndex.RegisterCallback<KeyDownEvent>(x =>
                {
                    if (x.keyCode != KeyCode.Return) return;
                    if (!int.TryParse(tfItemIndex.value, out var newIndex)) return;
                    newIndex = Math.Clamp(newIndex, 0, ParentProperty.arraySize - 1);
                    if (newIndex == Index) return;
                    List.MoveItem(Index, newIndex);
                });
                btItemDelete.RegisterCallback<ClickEvent>(x => Delete());

                Content = new PropertyField();
                veItemContent.Add(Content);
            }
            public void Delete()
            {
                ParentProperty.DeleteArrayElementAtIndex(Index);
                ParentProperty.serializedObject.ApplyModifiedProperties();
                List.RefreshItemsAfter(Index);
            }
            public void RemoveFromParent()
            {
                RootVisualElement.parent.Remove(RootVisualElement);
            }
            public void MoveToIndex(int index)
            {
                var parent = RootVisualElement.parent;
                parent.Remove(RootVisualElement);
                Index = index;
                parent.Insert(Index, RootVisualElement);
            }
            public void BindProperty(SerializedProperty property, int index)
            {

                Index = index;
                var tfItemIndex = RootVisualElement.Query<TextField>("tfIndex").First();
                tfItemIndex.value = index.ToString();

                if(property != Property)
                {
                    Content.Unbind();
                    Content.BindProperty(property);
                    Property = property;
                }
            }
        }


        List<Item> Items = new();
        public Action<SerializedProperty> OnNewItem;

        public ListView2()
        {
            Build();
        }
        public void SetIcon(Texture2D icon)
        {
            this.Query<VisualElement>("veIcon").First().style.backgroundImage = icon;
        }
        public void SetColor(StyleColor color)
        {
            this.Query<VisualElement>("veColor").ForEach(x => x.style.backgroundColor = color);
        }
        public void SetText(string value)
        {
            LbText.text = value;
        }
        public void SetAddButtonText(string value)
        {
            BtAdd.text = value;
        }
        void Build()
        {
            ReactionStateMachineEditor.ListAsset.CloneTree(this);

            VeContent = this.Query<VisualElement>("veListContent").First();

            Toggle = this.Query<Toggle>().First();
            VeFold = this.Query<VisualElement>("veFold").First();
            BtAdd = this.Query<Button>("btAdd").First();
            LbCount = this.Query<Label>("lbCount").First();
            LbText = this.Query<Label>("lbText").First();
            BtAdd.RegisterCallback<ClickEvent>(x => AddToEnd());
            Toggle.RegisterCallback<ChangeEvent<bool>>(x => SetFold(x.newValue));

        }
        void SetFold(bool value)
        {
            VeFold.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            Property.isExpanded = value;
            Property.serializedObject.ApplyModifiedProperties();
        }

        //void RemoveAt(VisualElement veItem, int index)
        //{

        //    Property.DeleteArrayElementAtIndex(index);
        //    Property.serializedObject.ApplyModifiedProperties();


        //    VeContent.Remove(veItem);
        //}
        public void MoveItem(int fromIndex, int toIndex)
        {
            var item = Items[fromIndex];
            Items.RemoveAt(fromIndex);
            item.MoveToIndex(toIndex);
            Items.Insert(toIndex, item);

            Property.MoveArrayElement(fromIndex, toIndex);
            Property.serializedObject.ApplyModifiedProperties();

            RefreshItemsAfter(Math.Min(fromIndex, toIndex));
        }
        void AddToEnd()
        {
            Property.InsertArrayElementAtIndex(Property.arraySize);
            int index = Property.arraySize - 1;
            var element = Property.GetArrayElementAtIndex(index);
            if (OnNewItem != null)
                OnNewItem(element);
            Property.serializedObject.ApplyModifiedProperties();
            RefreshItemsAfter(index);
            //Refresh();
        }

        public void RefreshItemsAfter(int index)
        {
            int i = index;
            for (; i != Items.Count; ++i)
            {
                var item = Items[i];
                if (i < Property.arraySize)
                {
                    item.BindProperty(Property.GetArrayElementAtIndex(i), i);
                }
                else
                {
                    // delete the remaining of items
                    for (int iRemove = i; iRemove != Items.Count; ++iRemove)
                        Items[iRemove].RemoveFromParent();
                    Items.RemoveRange(i, Items.Count - i);
                    return;
                }
            }
            for (; i < Property.arraySize; ++i)
            {
                var e = Property.GetArrayElementAtIndex(i);
                var item = new Item(this, Property, i);
                item.BindProperty(e, i);
                VeContent.Add(item.RootVisualElement);
                Items.Add(item);
            }
        }
        void Refresh()
        {
            Toggle.value = Property.isExpanded;
            VeFold.style.display = Property.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            LbCount.text = Property.arraySize.ToString();
            VeContent.Clear();
            RefreshItemsAfter(0);
            //for (int i = 0; i != Property.arraySize; ++i)
            //{
            //    var e = Property.GetArrayElementAtIndex(i);
            //    var item = new Item(this, Property, i);
            //    item.BindProperty(e, i);
            //    VeContent.Add(item.RootVisualElement);
            //    Items.Add(item);
            //}
        }
        public void BindProperty(SerializedProperty property)
        {
            Property = property;
            Refresh();
            //VeContent.TrackPropertyValue(Property, x => 
            //{
            //    Property = x;
            //    Refresh();
            //    //LbCount.text = property.arraySize.ToString();
            //    //VeContent.Clear();
            //    //for (int i = 0; i != Property.arraySize; ++i)
            //    //{
            //    //    var e = Property.GetArrayElementAtIndex(i);
            //    //    var pf = new PropertyField();
            //    //    pf.BindProperty(e);
            //    //    VeContent.Add(pf);
            //    //}
            //});
        }
    }
}