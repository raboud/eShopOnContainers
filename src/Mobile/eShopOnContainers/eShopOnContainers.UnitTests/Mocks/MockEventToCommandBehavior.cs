using HMS.Core.Behaviors;

namespace HMS.UnitTests
{
	public class MockEventToCommandBehavior : EventToCommandBehavior
	{
		public void RaiseEvent(params object[] args)
		{
			_handler.DynamicInvoke(args);
		}
	}
}
