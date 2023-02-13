using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace CocRfidReader.WPF.Behaviors
{
    public class ScrollIntoViewBehavior : Behavior<ScrollViewer>
    {
        public object AutoScrollTrigger
        {
            get { return (object)GetValue(AutoScrollTriggerProperty); }
            set { SetValue(AutoScrollTriggerProperty, value); }
        }

        public static readonly DependencyProperty AutoScrollTriggerProperty =
        DependencyProperty.Register(
            "AutoScrollTrigger",
            typeof(object),
            typeof(ScrollIntoViewBehavior),
            new PropertyMetadata(null, ASTPropertyChanged));

        private static void ASTPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var ts = d as ScrollIntoViewBehavior;
            if (ts == null)
                return;

            // must be attached to a ScrollViewer
            var sv = ts.AssociatedObject as ScrollViewer;

            // check if we are attached to an ObservableCollection, in which case we
            // will subscribe to CollectionChanged so that we scroll when stuff is added/removed
            var ncol = args.NewValue as INotifyCollectionChanged;
            // new event handler
            if (ncol != null)
                ncol.CollectionChanged += ts.OnCollectionChanged;

            // remove old eventhandler
            var ocol = args.OldValue as INotifyCollectionChanged;
            if (ocol != null)
                ocol.CollectionChanged -= ts.OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, EventArgs args)
        {
            App.Current.Dispatcher.Invoke(delegate {
                (this.AssociatedObject as ScrollViewer).ScrollToBottom();
            });
        }
    }
}
