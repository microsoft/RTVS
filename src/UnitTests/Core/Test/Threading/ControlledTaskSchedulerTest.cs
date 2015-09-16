using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.UnitTests.Core.Test.Threading
{
    public class ControlledTaskSchedulerTest
	{
		[Test]
		public void ControlledTaskScheduler_ThrowException()
		{
			ControlledTaskScheduler scheduler = UIThreadHelper.Instance.TaskScheduler;
			Func<Task> f = async () => await Task.Factory.StartNew(SleepAndThrow, CancellationToken.None, TaskCreationOptions.None, scheduler);
			f.ShouldThrow<CustomException>();
		}

		[Test]
		public void ControlledTaskScheduler_GetAwaiter_ThrowException()
		{
			ControlledTaskScheduler scheduler = UIThreadHelper.Instance.TaskScheduler;
			Task.Factory.StartNew(SleepAndThrow, CancellationToken.None, TaskCreationOptions.None, scheduler);
			Func<Task> f = async () => await scheduler;
			f.ShouldThrow<CustomException>();
		}

		[Test]
		public void ControlledTaskScheduler_Wait_ThrowException()
		{
			ControlledTaskScheduler scheduler = UIThreadHelper.Instance.TaskScheduler;
			Task.Factory.StartNew(SleepAndThrow, CancellationToken.None, TaskCreationOptions.None, scheduler);
			Action a = () => scheduler.Wait();
			a.ShouldThrow<CustomException>();
        }

		[Test]
		public void ControlledTaskScheduler_WaitForUpcomingTasks_ThrowException()
		{
			ControlledTaskScheduler scheduler = UIThreadHelper.Instance.TaskScheduler;
			Task.Delay(100).ContinueWith(t => Task.Factory.StartNew(SleepAndThrow, CancellationToken.None, TaskCreationOptions.None, scheduler));
			Action a = () =>
			{
				scheduler.WaitForUpcomingTasks(200);
			};
			a.ShouldThrow<CustomException>();
		}

		[Test]
		public void ControlledTaskScheduler_WaitForUpcomingTasks_ThrowTimeoutException()
		{
			ControlledTaskScheduler scheduler = UIThreadHelper.Instance.TaskScheduler;
			Task.Delay(200).ContinueWith(t => Task.Factory.StartNew(SleepAndThrow, CancellationToken.None, TaskCreationOptions.None, scheduler));
			Action a = () =>
			{
				scheduler.WaitForUpcomingTasks(100);
			};
			a.ShouldThrow<TimeoutException>();
		}

		[Test]
		public void ControlledTaskScheduler_WaitActionBlock_ThrowException()
		{
			ControlledTaskScheduler scheduler = UIThreadHelper.Instance.TaskScheduler;
			Func<object, Task> f = o => Task.Factory.StartNew(SleepAndThrow);
			ActionBlock<object> actionBlock = new ActionBlock<object>(f, new ExecutionDataflowBlockOptions {TaskScheduler = scheduler});

			Action a = () => UIThreadHelper.Instance.Wait(actionBlock);
			a.ShouldThrow<CustomException>();
        }

		private static void SleepAndThrow()
		{
			Thread.Sleep(10);
			throw new CustomException();
		}

		private class CustomException : Exception
		{
		}
    }
}
