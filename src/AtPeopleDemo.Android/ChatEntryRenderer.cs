using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using AtPeopleDemo;
using AtPeopleDemo.Droid;
using Java.Lang;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ChatEntry), typeof(ChatEntryRenderer))]
namespace AtPeopleDemo.Droid
{
   public  class ChatEntryRenderer : EntryRenderer, ITextWatcher
    {
        public ChatEntryRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (EditText != null)
            {
                EditText.AddTextChangedListener(this);
            }
        }

        void ITextWatcher.AfterTextChanged(IEditable s)
        {
        }

        void ITextWatcher.BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("BeforeTextChanged === start:{0},count:{1},after:{2}", start, count, after));
        }

        void ITextWatcher.OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("OnTextChanged === start:{0},count:{1},before:{2}", start, count, before));
            ((IElementController)Element).SetValueFromRenderer(Entry.TextProperty, s.ToString());
            var args = new TextChangedArgs
            {
                Offset = start,
                AddedLength = count,
                RemovedLength = before
            };
            (Element as ChatEntry).OnTextChanged(args);
        }


        protected override void Dispose(bool disposing)
        {

            if (disposing)
            {
                EditText?.RemoveTextChangedListener(this);
            }
            base.Dispose(disposing);
        }
    }
}