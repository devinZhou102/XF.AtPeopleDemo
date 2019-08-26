using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AtPeopleDemo;
using AtPeopleDemo.iOS;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ChatEntry), typeof(ChatEntryRenderer))]
namespace AtPeopleDemo.iOS
{
    public class ChatEntryRenderer : EntryRenderer
    {
        UITextFieldChange _UITextFieldChange;
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                _UITextFieldChange = new UITextFieldChange(FunShouldChangeCharacters);
                Control.ShouldChangeCharacters += _UITextFieldChange;
                Control.EditingChanged += Control_EditingChanged;
            }
        }

        TextChangedArgs textChangedArgs;

        private void Control_EditingChanged(object sender, EventArgs e)
        {
            Debug.WriteLine(string.Format("Control_EditingChanged === {0}", e.ToString()));
            if (textChangedArgs != null)
            {
                (Element as ChatEntry).OnTextChanged(textChangedArgs);
            }
            textChangedArgs = null;
        }

        private bool FunShouldChangeCharacters(UITextField txt, NSRange range, string sampleTxt)
        {
            Debug.WriteLine(string.Format("FunShouldChangeCharacters === location:{0},length:{1},sampleTxt:{2}", range.Location, range.Length, sampleTxt));
            textChangedArgs = new TextChangedArgs
            {
                Offset = (int)range.Location,
                AddedLength = (int)sampleTxt.Length,
                RemovedLength = (int)range.Length
            };
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Control != null)
                {
                    Control.EditingChanged -= Control_EditingChanged;
                    Control.ShouldChangeCharacters -= _UITextFieldChange;
                }
            }
            base.Dispose(disposing);
        }
    }
}