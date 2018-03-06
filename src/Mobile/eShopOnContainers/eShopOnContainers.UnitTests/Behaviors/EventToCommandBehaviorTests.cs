using System;
using System.Globalization;
using Xamarin.Forms;
using Xunit;

namespace HMS.UnitTests
{
    public class EventToCommandBehaviorTests
	{
		[Fact]
		public void InvalidEventNameShouldThrowArgumentExceptionText()
		{
			MockEventToCommandBehavior behavior = new MockEventToCommandBehavior
			{
				EventName = "OnItemTapped"
			};
			ListView listView = new ListView();

			Assert.Throws<ArgumentException>(() => listView.Behaviors.Add(behavior));
		}

		[Fact]
		public void CommandExecutedWhenEventFiresText()
		{
			bool executedCommand = false;
			MockEventToCommandBehavior behavior = new MockEventToCommandBehavior
			{
				EventName = "ItemTapped",
				Command = new Command(() =>
				{
					executedCommand = true;
				})
			};
			ListView listView = new ListView();
			listView.Behaviors.Add(behavior);

			behavior.RaiseEvent(listView, null);

			Assert.True(executedCommand);
		}

		[Fact]
		public void CommandCanExecuteTest()
		{
			MockEventToCommandBehavior behavior = new MockEventToCommandBehavior
			{
				EventName = "ItemTapped",
				Command = new Command(() => Assert.True(false), () => false)
			};
			ListView listView = new ListView();
			listView.Behaviors.Add(behavior);

			behavior.RaiseEvent(listView, null);
		}

		[Fact]
		public void CommandCanExecuteWithParameterShouldNotExecuteTest()
		{
			bool shouldExecute = false;
			MockEventToCommandBehavior behavior = new MockEventToCommandBehavior
			{
				EventName = "ItemTapped",
				CommandParameter = shouldExecute,
				Command = new Command<string>(o => Assert.True(false), o => o.Equals(true))
			};
			ListView listView = new ListView();
			listView.Behaviors.Add(behavior);

			behavior.RaiseEvent(listView, null);
		}

		[Fact]
		public void CommandWithConverterTest()
		{
			const string item = "ItemProperty";
			bool executedCommand = false;
			MockEventToCommandBehavior behavior = new MockEventToCommandBehavior
			{
				EventName = "ItemTapped",
				EventArgsConverter = new ItemTappedEventArgsConverter(false),
				Command = new Command<string>(o =>
				{
					executedCommand = true;
					Assert.NotNull(o);
					Assert.Equal(item, o);
				})
			};
			ListView listView = new ListView();
			listView.Behaviors.Add(behavior);

			behavior.RaiseEvent(listView, new ItemTappedEventArgs(listView, item));

			Assert.True(executedCommand);
		}

		private class ItemTappedEventArgsConverter : IValueConverter
		{
			private readonly bool _returnParameter;

			public bool HasConverted { get; private set; }

			public ItemTappedEventArgsConverter(bool returnParameter)
			{
				_returnParameter = returnParameter;
			}

			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				HasConverted = true;
				return _returnParameter ? parameter : (value as ItemTappedEventArgs)?.Item;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				throw new NotImplementedException();
			}
		}
	}

}
