using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace AtPeopleDemo
{
    public class ChatEntry : Entry
    {
        public static readonly BindableProperty AtPersonCommandProperty = BindableProperty.Create(nameof(AtPersonCommand), typeof(ICommand), typeof(ChatEntry), default(ICommand));

        public ICommand AtPersonCommand
        {
            get { return (ICommand)GetValue(AtPersonCommandProperty); }
            set { SetValue(AtPersonCommandProperty, value); }
        }

        public event EventHandler<AtEventArgs> AtEvent;

        public static readonly BindableProperty IsAtEnableProperty = BindableProperty.Create(nameof(IsAtEnable), typeof(bool), typeof(ChatEntry), default(bool));

        public bool IsAtEnable
        {
            get => (bool)GetValue(IsAtEnableProperty);
            set
            {
                SetValue(IsAtEnableProperty, value);
            }
        }

        public static readonly BindableProperty AtPeopleProperty =
            BindableProperty.Create(nameof(AtPeople), typeof(IList), typeof(ChatEntry), null);

        public IList<AtPerson> AtPeople
        {
            get => (IList<AtPerson>)GetValue(AtPeopleProperty);
            set
            {
                SetValue(AtPeopleProperty, value);
            }
        }

        //@
        private readonly List<AtIndex> AtIndexList;


        public ChatEntry()
        {
            AtIndexList = new List<AtIndex>();
            AtPeople = new List<AtPerson>();
        }

        string txtCache = "";

        public void OnTextChanged(TextChangedArgs args)
        {
            if (txtCache == this.Text) return;
            txtCache = this.Text;
            if (string.IsNullOrEmpty(this.Text)) Clear();
            if (args == null) return;
            Debug.WriteLine(string.Format("OnTextChanged === Offset:{0},RemovedLength:{1},AddedLength:{2}", args.Offset, args.RemovedLength, args.AddedLength));
            if (IsAtEnable)
            {
                if (args.AddedLength > 0 && args.RemovedLength > 0 && !isDeleting && !isInserting)//复制粘贴
                {
                    var result = ChangeCharactorsHasAtPerson(args.Offset, args.AddedLength);
                    FixAtIndex(args.Offset, args.AddedLength);
                    if (result.Item1)
                    {
                        DeleteCharactors(result.Item2);
                    }
                    FixAtIndex(args.Offset, args.RemovedLength * -1);
                }
                else if (args.AddedLength > 0 && args.RemovedLength == 0)
                {
                    if (!isInserting)
                    {
                        FixAtIndex(args.Offset, args.AddedLength);
                        var result = ChangeCharactorsHasAtPerson(args.Offset, args.AddedLength);
                        if (result.Item1 && !isInserting)
                        {
                            DeleteCharactors(result.Item2);
                        }
                    }
                    if (args?.AddedLength == 1)
                    {
                        var isAt = IsAtPerson(Text, args.Offset);
                        if (isAt && !isInserting)
                        {
                            AtPersonCommand?.Execute(args.Offset);
                            AtEvent?.Invoke(this,new AtEventArgs { Offset = args.Offset});
                        }
                    }
                }
                else if (args.RemovedLength > 0 && args.AddedLength == 0)//删除
                {
                    if (args.RemovedLength == 1)
                    {
                        var result = IsCursorInAtPerson(args.Offset + 1);
                        var result2 = IsCursorInAtPerson(args.Offset);
                        if (result.Item1 || result2.Item1)
                        {
                            AtIndex atIndex = null;
                            if (result.Item1) atIndex = result.Item2;
                            else atIndex = result2.Item2;
                            if (atIndex != null) DeletePersion(atIndex);
                        }
                        else
                        {
                            if (!isDeleting) FixAtIndex(args.Offset, args.RemovedLength * -1);
                        }
                    }
                    else
                    {
                        var result = ChangeCharactorsHasAtPerson(args.Offset, args.RemovedLength);
                        if (result.Item1 && !isDeleting)
                        {
                            DeleteCharactors(result.Item2);
                            FixAtIndex(args.Offset, args.RemovedLength * -1);
                        }
                        else
                        {
                            if (!isDeleting) FixAtIndex(args.Offset, args.RemovedLength * -1);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 检测cursor位置是否合理（在@的名字中即为不合理），若不合理移动到合理位置
        /// </summary>
        /// <param name="cursor"></param>
        private void FixCursorPosition(int cursor)
        {
            var result = IsCursorInAtPerson(cursor);
            if (result.Item1)
            {
                Device.BeginInvokeOnMainThread(() => {
                    CursorPosition = result.Item2.End + 1;
                });
            }
        }

        private void Clear()
        {
            AtIndexList.Clear();
            AtPeople.Clear();
        }

        private void Insert(AtIndex atIndex, AtPerson atPerson)
        {
            AtIndexList.Add(atIndex);
            AtPeople.Add(atPerson);
        }

        bool isInserting = false;


        public void InsertAtPerson(AtPerson atPerson, int index)
        {
            isInserting = true;
            if (atPerson == null) return;
            FixAtIndex(index, atPerson.Name.Length);
            var atIndex = new AtIndex
            {
                Start = index,
                End = index + atPerson.Name.Length,
            };
            Insert(atIndex, atPerson);
            StringBuilder sb = new StringBuilder(this.Text);
            sb.Insert(index + 1, atPerson.Name);
            Device.BeginInvokeOnMainThread(() =>
            {
                this.Text = sb.ToString();
            });
            FixCursorPosition(atIndex.End + 1);
            Task.Run(async () =>
            {
                await Task.Delay(50);
                isInserting = false;
            });
        }

        bool isDeleting = false;

        private void DeleteCharactors(List<AtIndex> list)
        {
            foreach (var item in list)
            {
                var index = AtIndexList.IndexOf(item);
                Remove(index);
            }
        }

        private void Remove(int index)
        {
            var count = AtIndexList.Count;
            if (count > index && index >= 0)
                AtIndexList.RemoveAt(index);
            count = AtPeople.Count;
            if (count > index && index >= 0)
                AtPeople.RemoveAt(index);
        }

        /// <summary>
        /// 删除的文字段是否包含@的人
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        private Tuple<bool, List<AtIndex>> ChangeCharactorsHasAtPerson(int cursor, int len)
        {
            var end = cursor + len;
            var list = new List<AtIndex>();
            foreach (var at in AtIndexList)
            {
                if ((at.Start >= cursor && at.Start < end) || (at.End >= cursor && at.End < end) || (at.Start < cursor && at.End >= end))
                {
                    list.Add(at);
                }
            }
            return new Tuple<bool, List<AtIndex>>(list.Count > 0, list);
        }

        private void DeletePersion(AtIndex atIndex)
        {
            isDeleting = true;
            var c = AtIndexList.IndexOf(atIndex);
            Remove(c);
            StringBuilder sb = new StringBuilder(this.Text);
            var removeLength = atIndex.End - atIndex.Start;
            sb.Remove(atIndex.Start, removeLength);
            FixAtIndex(atIndex.Start, (removeLength + 1) * -1);
            Device.BeginInvokeOnMainThread(() =>
            {
                this.Text = sb.ToString();
            });
            var index = atIndex.Start;
            FixCursorPosition(index);
            Task.Run(async () =>
            {
                await Task.Delay(50);
                isDeleting = false;
            });
        }


        private void FixAtIndex(int cursor, int len)
        {
            foreach (var at in AtIndexList)
            {
                if (at.Start >= cursor)
                {
                    at.Start += len;
                    at.End += len;
                }
            }
            Debug.WriteLine("FixAtIndex AtIndexList.Count === " + AtIndexList.Count);
        }

        /// <summary>
        /// 判断 cursor 是否在@的人名中
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        private Tuple<bool, AtIndex> IsCursorInAtPerson(int cursor)
        {
            foreach (var at in AtIndexList)
            {
                if (cursor > at.Start && cursor <= at.End)
                    return new Tuple<bool, AtIndex>(true, at);
            }
            return new Tuple<bool, AtIndex>(false, null);
        }

        /// <summary>
        /// 判断是否@用户
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="cursor"></param>
        /// <returns></returns>
        private bool IsAtPerson(string newValue, int cursor)
        {
            var o = newValue.ToString();
            if (o.Length > cursor)
            {
                var data = o[cursor];
                if (data.ToString().Equals("@")) return true;
            }
            return false;
        }

    }

    public class AtIndex : IComparable<AtIndex>
    {
        public int Start { get; set; }
        public int End { get; set; }

        public int CompareTo(AtIndex other)
        {
            return Start - other.Start;
        }
    }

    public class TextChangedArgs
    {
        public int Offset { get; set; }

        public int AddedLength { get; set; }

        public int RemovedLength { get; set; }
    }

    public class AtEventArgs : EventArgs
    {
        public int Offset { get; set; }
    }
}
